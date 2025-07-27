using LoDCompanion.Models;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Combat;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.Game;
using System.Threading;
using LoDCompanion.Services.GameData;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Player
{
    /// <summary>
    /// Defines the types of actions a player can take.
    /// </summary>
    public enum ActionType
    {
        StandardAttack,
        StandardAttackWhileAiming,
        Move,
        SearchRoom,
        SearchFurniture,
        SearchCorpse,
        OpenDoor,
        PickLock,
        DisarmTrap,
        HealSelf,
        HealOther,
        EquipGear,
        AddItemToQuickSlot,
        IdentifyItem,
        SetOverwatch,
        PowerAttack,
        ChargeAttack,
        Shove,
        CastSpell,
        EndTurn,
        Reload,
        ReloadWhileMoving,
        Aim,
        Parry,
        Pray
    }

    public class ActionInfo
    {
        public ActionType ActionType { get; set; }
        public int ApCost { get; set; }
        public object? Target { get; set; }
        public object? SecondaryTarget { get; set; } // For actions that may require a secondary target, like healing or attacking multiple targets
    }

    /// <summary>
    /// Handles the execution of actions performed by heroes.
    /// </summary>
    public class ActionService
    {
        private readonly DungeonManagerService _dungeonManager;
        private readonly SearchService _search;
        private readonly HealingService _healing;
        private readonly InventoryService _inventory;
        private readonly IdentificationService _identification;
        private readonly AttackService _attack;
        private readonly DiceRollService _diceRoll;
        private readonly SpellCastingService _spellCastingService;

        public ActionService(
            DungeonManagerService dungeonManagerService, 
            SearchService searchService,
            HealingService healingService,
            InventoryService inventoryService,
            IdentificationService identificationService,
            AttackService attackService,
            DiceRollService diceRollService,
            SpellCastingService spellCastingService)
        {
            _dungeonManager = dungeonManagerService;
            _search = searchService;
            _healing = healingService;
            _inventory = inventoryService;
            _identification = identificationService;
            _attack = attackService;
            _diceRoll = diceRollService;
            _spellCastingService = spellCastingService;
        }

        /// <summary>
        /// Attempts to perform an action for a hero, checking and deducting AP.
        /// </summary>
        /// <param name="character">The hero performing the action.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <param name="target">The target of the action (e.g., a Monster, DoorChest, or another Hero).</param>
        /// <returns>True if the action was successfully performed, false otherwise.</returns>
        public async Task<string> PerformActionAsync(DungeonState dungeon, Character character, ActionType actionType, object? primaryTarget = null, object? secondaryTarget = null)
        {
            string resultMessage = "";
            int startingAP = character.CurrentAP;
            int apCost = GetActionCost(actionType);
            if (character.CurrentAP < apCost)
            {
                resultMessage = $"{character.Name} does not have enough AP for {actionType}.";
                return resultMessage;
            }

            resultMessage = $"{character.Name} performed {actionType}.";
            bool actionWasSuccessful = true;
            Weapon? weapon = new Weapon();
            if (character.Weapons.FirstOrDefault() is Weapon w)
            {
                weapon = w;
            }

            // check to see if the character is in the middle of their and is choosing a different action type.
            // This cancels the remaining move and sets as finsihing their move.
            if ( actionType != ActionType.Move && ( character.CurrentMovePoints < character.GetStat(BasicStat.Move) && !character.HasMadeFirstMoveAction))
            {
                character.HasMadeFirstMoveAction = true;
                character.ResetMovementPoints(); // Reset movement points if first move action is made
                character.CurrentAP--; // Deduct 1 AP for finishing the move action
                if (character.CurrentAP <= 0)
                {
                    return $"{character.Name} has no AP left to perform {actionType}.";
                }
            }

            // Execute the action logic
            switch (actionType)
            {
                case ActionType.StandardAttack:
                    if (primaryTarget is Character standardAttackTarget && weapon != null)
                    {
                        resultMessage = await PerformActionAsync(dungeon, character, ActionType.Reload);
                        if (character.CurrentAP <= 0) break;

                        AttackResult attackResult = await _attack.PerformStandardAttackAsync(character, weapon, standardAttackTarget, dungeon);
                        if (startingAP > character.CurrentAP)
                        {
                            resultMessage += "\n" + attackResult.OutcomeMessage; 
                        }
                        else
                        {
                            resultMessage = attackResult.OutcomeMessage;
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid target or no weapon equipped for attack.";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.StandardAttackWhileAiming:
                    if (primaryTarget is Character standardAttackTarget1 && weapon != null)
                    {
                        resultMessage = await PerformActionAsync(dungeon, character, ActionType.Reload);
                        if (character.CurrentAP <= 0) break;

                        AttackResult attackResult = await _attack.PerformStandardAttackAsync(character, weapon, standardAttackTarget1, dungeon, new CombatContext { HasAimed = true });
                        if (startingAP > character.CurrentAP)
                        {
                            resultMessage += "\n" + attackResult.OutcomeMessage;
                        }
                        else
                        {
                            resultMessage = attackResult.OutcomeMessage;
                        }
                        character.CombatStance = CombatStance.Normal;
                    }
                    else
                    {
                        resultMessage = "Invalid target or no weapon equipped for attack.";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.PowerAttack:
                    if (character.CurrentAP >= GetActionCost(actionType) && character.Weapons.FirstOrDefault(w => w.IsMelee) is MeleeWeapon meleeWeapon && primaryTarget is Character powerAttackTarget)
                    {
                        AttackResult attackResult = await _attack.PerformPowerAttackAsync(character, meleeWeapon, powerAttackTarget, dungeon);
                        character.IsVulnerableAfterPowerAttack = true; // Set the vulnerability flag
                        resultMessage = attackResult.OutcomeMessage;
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.ChargeAttack:
                    if (character.CurrentAP >= GetActionCost(actionType) && primaryTarget is Character chargeAttackTarget && character.Weapons.FirstOrDefault(w => w.IsMelee) is MeleeWeapon chargeWeapon)
                    {
                        AttackResult attackResult = await _attack.PerformChargeAttackAsync(character, chargeWeapon, chargeAttackTarget, dungeon);
                        resultMessage = attackResult.OutcomeMessage;
                        Room? room = _dungeonManager.FindRoomAtPosition(character.Position);
                        if (room != null)
                        {
                            character.Room = room; 
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.Shove:
                    if (primaryTarget is Character targetToShove && _dungeonManager.DungeonState != null)
                    {
                        resultMessage = GridService.ShoveCharacter(character, targetToShove, _dungeonManager.DungeonState.DungeonGrid); // Pass current room

                        Room? room = _dungeonManager.FindRoomAtPosition(character.Position);
                        if (room != null)
                        {
                            character.Room = room;
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.Move:
                    if (primaryTarget is GridPosition targetPosition && character.Room != null)
                    {
                        // Determine available movement points for this action
                        int availableMovement = character.CurrentMovePoints;
                        if (character.HasMadeFirstMoveAction) // Rule: Second move is half distance
                        {
                            availableMovement /= 2;
                        }

                        List<GridPosition> path = GridService.FindShortestPath(character.Position, targetPosition, dungeon.DungeonGrid);

                        // Determine enemies for ZOC calculation
                        List<Character> enemies = new List<Character>();
                        if (_dungeonManager.DungeonState != null)
                        {
                            if (character is Hero)
                            {
                                enemies = _dungeonManager.DungeonState.RevealedMonsters.Cast<Character>().ToList();
                                if(enemies.Count <= 0 && character.Room.MonstersInRoom != null)
                                {
                                    enemies = character.Room.MonstersInRoom.Cast<Character>().ToList();
                                }
                            }
                            else if (character is Monster)
                            {
                                if (_dungeonManager.DungeonState.HeroParty != null)
                                {
                                    enemies = _dungeonManager.DungeonState.HeroParty.Heroes.Cast<Character>().ToList(); 
                                }
                                if (enemies.Count <= 0 && character.Room.HeroesInRoom != null)
                                {
                                    enemies = character.Room.HeroesInRoom.Cast<Character>().ToList();
                                }
                            } 
                        }

                        MovementResult moveResult = GridService.MoveCharacter(character, path, dungeon.DungeonGrid, enemies, availableMovement);

                        if (moveResult.WasSuccessful)
                        {
                            character.SpendMovementPoints(moveResult.MovementPointsSpent); // A new method you'll add to Character
                            availableMovement = character.CurrentMovePoints;
                            resultMessage = moveResult.Message;
                            if (availableMovement <= 0)
                            {
                                character.HasMadeFirstMoveAction = true;
                                character.ResetMovementPoints();
                            }
                            else
                            {
                                resultMessage = moveResult.Message;
                                actionWasSuccessful = false; // Don't deduct AP if movement points remain
                            }
                        }
                        else
                        {
                            resultMessage = moveResult.Message;
                            actionWasSuccessful = false; // Don't deduct AP if no move was made
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid destination for move action.";
                        actionWasSuccessful = false;
                    }
                    break;

                case ActionType.OpenDoor:
                    if (primaryTarget is DoorChest door)
                    {
                        _dungeonManager.InteractWithDoor(door, character);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.HealSelf:
                    if (character is Hero self)
                    {
                        resultMessage = _healing.ApplyBandage(self, self);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.HealOther:
                    if (character is Hero actingHero && primaryTarget is Hero targetHero)
                    {
                        resultMessage = _healing.ApplyBandage(actingHero, targetHero);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.EquipGear:
                    if (character is Hero inventoryHero && primaryTarget is Equipment item)
                    { 
                        if(_inventory.EquipItem(inventoryHero, item)) resultMessage = $"{item.Name} was equipped";
                        else 
                        { 
                            resultMessage = $"{item.Name} could not be equipped";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.AddItemToQuickSlot:
                    if (character is Hero inventoryHero1 && primaryTarget is Equipment item1)
                    {
                        if (_inventory.EquipItem(inventoryHero1, item1)) resultMessage = $"{item1.Name} was equipped";
                        else
                        {
                            resultMessage = $"{item1.Name} could not be equipped";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.IdentifyItem:
                    if (character is Hero identifyingHero && primaryTarget is Equipment itemToIdentify)
                    { 
                        resultMessage = _identification.IdentifyItem(identifyingHero, itemToIdentify);
                    }
                    else
                    {
                        resultMessage = "Action was unsuccessful";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.SetOverwatch:
                    var equippedWeapon = character.Weapons.FirstOrDefault();
                    if (equippedWeapon == null) return $"{character.Name} does not have a weapon equipped";
                    if (equippedWeapon is RangedWeapon ranged && !ranged.IsLoaded) return $"{character.Name} needs to reload their weapon";
                    character.CombatStance = CombatStance.Overwatch;
                    apCost = character.CurrentAP;
                    resultMessage = $"{character.Name} takes an Overwatch stance, ready to react.";
                    break;
                case ActionType.EndTurn:
                    resultMessage = $"{character.Name} ends their turn.";
                    apCost = character.CurrentAP;
                    break;
                case ActionType.Parry:
                    character.CombatStance = CombatStance.Parry;
                    apCost = character.CurrentAP;
                    resultMessage = $"{character.Name} entered parry stance";
                    break;
                case ActionType.Aim:
                    character.CombatStance = CombatStance.Aiming;
                    resultMessage = $"{character.Name} takes careful aim.";
                    break;
                case ActionType.Reload:
                    if (weapon is RangedWeapon rangedWeapon)
                    {
                        if (!rangedWeapon.IsLoaded)
                        {
                            rangedWeapon.reloadAmmo();
                            if (character is Monster) rangedWeapon.IsLoaded = true;
                            resultMessage = $"{character.Name} spends a moment to reload their {rangedWeapon.Name}."; 
                        }
                        else
                        {
                            resultMessage = $"{character.Name} weapon is already loaded";
                            actionWasSuccessful = false;
                        }
                    }
                    else
                    {
                        resultMessage = $"{character.Name} does not have a ranged weapon equipped";
                        actionWasSuccessful = false;
                    }
                    break;
                case ActionType.ReloadWhileMoving:
                    if (weapon is RangedWeapon rangedWeapon1 && !rangedWeapon1.IsLoaded)
                    {
                        rangedWeapon1.reloadAmmo();
                        if(character is Monster) rangedWeapon1.IsLoaded = true;
                        resultMessage = $" and reloads their {rangedWeapon1.Name}.";
                    }
                    else
                    {
                        resultMessage = string.Empty;
                    }
                    break;
                case ActionType.CastSpell:
                    if (character is Hero heroCasting && secondaryTarget is Spell spellToCast)
                    {
                        var options = await _spellCastingService.RequestCastingOptionsAsync(heroCasting, spellToCast);

                        if (options.WasCancelled)
                        {
                            resultMessage = $"{heroCasting.Name} decided not to cast the spell.";
                            actionWasSuccessful = false;
                        }
                        else
                        {
                            SpellCastResult spellCastResult = await spellToCast.CastSpellAsync(heroCasting, _diceRoll, options.FocusPoints, options.PowerLevels);
                            resultMessage = spellCastResult.OutcomeMessage;

                            if (spellCastResult.ManaSpent <= 0)
                            {
                                actionWasSuccessful = false;
                            }
                            else
                            {
                                if (options.FocusPoints <= 0)
                                {
                                    if (spellToCast.Properties != null && spellToCast.Properties.Contains(SpellProperty.QuickSpell))
                                    {
                                        apCost = 1; // Quick spells cost 1 AP
                                    }
                                    else
                                    {
                                        apCost = 2; // Regular spells cost 2 AP if there is no focus points added
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        resultMessage = "Invalid target for CastSpell action.";
                        actionWasSuccessful = false;
                    }
                    break;
            }

            if (actionWasSuccessful)
            {
                character.CurrentAP -= apCost;
            }

            return $"{character.Name} performed {actionType}, {resultMessage}.";
        }

        /// <summary>
        /// Gets the AP cost for a specific action type.
        /// </summary>
        public int GetActionCost(ActionType actionType, object? context = null)
        {
            return actionType switch
            {
                ActionType.StandardAttack => 1,
                ActionType.PowerAttack => 2,
                ActionType.ChargeAttack => 2,
                ActionType.Shove => 1,
                ActionType.Move => 1,
                ActionType.OpenDoor => 1,
                ActionType.PickLock => 2,
                ActionType.DisarmTrap => 2,
                ActionType.SearchRoom => 2,
                ActionType.SearchFurniture => 1,
                ActionType.SearchCorpse => 1,
                ActionType.HealOther => 1,
                ActionType.HealSelf => 2,
                ActionType.EquipGear => 2,
                ActionType.AddItemToQuickSlot => 2,
                ActionType.IdentifyItem => 0,
                ActionType.Reload => 1,
                ActionType.Aim => 1,
                ActionType.ReloadWhileMoving => 0,
                ActionType.Pray => 0,
                ActionType.CastSpell => (context is Spell spell && spell.Properties != null && spell.Properties.Contains(SpellProperty.QuickSpell)) ? 1 : 2,
                _ => 1,
            };
        }
    }
}
