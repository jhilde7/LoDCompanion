using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.Utilities;

namespace LoDCompanion.BackEnd.Services.Combat
{
    public class CombatManagerService
    {
        private readonly InitiativeService _initiative;
        private readonly ActionService _playerAction;
        private readonly MonsterAIService _monsterAI;
        private readonly DungeonManagerService _dungeonManager;
        private readonly FacingDirectionService _facing;
        private readonly SpellResolutionService _spellResolution;
        private readonly FloatingTextService _floatingText;
        private readonly MovementHighlightingService _movementHighlighting;
        private readonly PowerActivationService _powerActivation;


        private static readonly GridPosition ScreenCenterPosition = new GridPosition(-1, -1, -1);
        private List<Hero> HeroesInCombat = new List<Hero>();
        private List<Monster> MonstersInCombat = new List<Monster>();
        private List<Monster> MonstersThatHaveActedThisTurn = new List<Monster>();
        public bool IsAwaitingHeroSelection { get; private set; } = false;
        public bool IsCombatOver => !HeroesInCombat.Any(h => h.CurrentHP > 0) || !MonstersInCombat.Any(m => m.CurrentHP > 0);
        public List<string> CombatLog { get; set; } = new List<string>();
        public event Action? OnCombatStateChanged;
        public Hero? ActiveHero { get; private set; }
        private HashSet<string> UnwieldlyBonusUsed = new HashSet<string>();
        private HashSet<Character> CharactersWithProcessedEffectsThisTurn = new HashSet<Character>();
        public DungeonState _dungeon => _dungeonManager.Dungeon;

        public CombatManagerService(
            InitiativeService initiativeService,
            ActionService playerActionService,
            MonsterAIService monsterAIService,
            DungeonManagerService dungeonManagerService,
            FacingDirectionService facingDirectionService,
            SpellResolutionService spellResolutionService,
            FloatingTextService floatingTextService,
            MovementHighlightingService movementHighlightingService,
            PowerActivationService powerActivationService)
        {
            _initiative = initiativeService;
            _playerAction = playerActionService;
            _monsterAI = monsterAIService;
            _dungeonManager = dungeonManagerService;
            _facing = facingDirectionService;
            _spellResolution = spellResolutionService;
            _floatingText = floatingTextService;
            _movementHighlighting = movementHighlightingService;
            _powerActivation = powerActivationService;

            _spellResolution.OnTimeFreezeCast += HandleTimeFreeze;
            _playerAction.OnMonsterMovement += HandleMonsterMovement;
        }


        public void SetupCombat(List<Hero> heroes, List<Monster> monsters, bool didBashDoor = false)
        {
            HeroesInCombat = heroes;
            MonstersInCombat = monsters;
            UnwieldlyBonusUsed.Clear();
            CombatLog.Clear();
            MonstersThatHaveActedThisTurn.Clear();
            ActiveHero = null;
            IsAwaitingHeroSelection = false;
            List<Character> characters = [.. heroes, .. monsters];

            foreach (var character in characters)
            {
                character.OnDeath += HandleDeath;
            }

            foreach (var hero in heroes)
            {
                hero.HasDodgedThisBattle = false;
                if (hero.ProfessionName == "Wizard" && hero.Spells != null) hero.CanCastSpell = true;
            }

            if(monsters.Where(m => m.Species == SpeciesName.Demon).Any())
            {
                _dungeonManager.PartyManager.UpdateMorale(changeEvent:MoraleChangeEvent.CombatWithDemons);
            }

            PrepareCharactersForCombat(heroes, monsters);
            _initiative.SetupInitiative(HeroesInCombat, MonstersInCombat, didBashDoor);

            CombatLog.Add("The battle begins!");

            OnCombatStateChanged?.Invoke();
        }

        private void HandleDeath(Character deceasedCharacter)
        {
            if (deceasedCharacter is Monster deceasedMonster)
            {
                MonstersInCombat.Remove(deceasedMonster);
                CombatLog.Add($"{deceasedMonster.Name} has been slain!");

                if (deceasedMonster.Body != null)
                {
                    Corpse corpse = deceasedMonster.Body;
                    corpse.Position = deceasedMonster.Position ?? new GridPosition(0, 0, 0);
                    corpse.Room = deceasedMonster.Room;
                    corpse.UpdateOccupiedSquares(); 
                }

                deceasedMonster.OnDeath -= HandleDeath;

                OnCombatStateChanged?.Invoke();
            }
        }

        private void PrepareCharactersForCombat(List<Hero> heroes, List<Monster> monsters)
        {
            foreach (var hero in heroes)
            {
                hero.HasDodgedThisBattle = false;
                hero.HasMadeFirstMoveAction = false;
                hero.ResetMovementPoints();

                // Load all ranged weapons.
                if (hero.Inventory.EquippedWeapon is RangedWeapon rangedWeapon && !rangedWeapon.IsLoaded)
                {
                    rangedWeapon.reloadAmmo();
                }
            }

            foreach (var monster in monsters)
            {
                monster.ResetMovementPoints();

                // Load all ranged weapons.
                foreach (var weapon in monster.Weapons)
                {
                    if (weapon is RangedWeapon rangedWeapon && !rangedWeapon.IsLoaded)
                    {
                        rangedWeapon.reloadAmmo();
                    }
                }
            }
        }

        public async Task StartFirstTurnAsync()
        {
            CombatLog.Add("--- Turn 1 ---");
            await ProcessNextInInitiativeAsync();
        }

        /// <summary>
        /// Sets up a new turn by resetting AP and preparing the initiative bag.
        /// </summary>
        private async Task StartNewTurnAsync()
        {
            CombatLog.Add("--- New Turn ---");
            MonstersThatHaveActedThisTurn.Clear();
            CharactersWithProcessedEffectsThisTurn.Clear();
            await _dungeonManager.HandleScenarioRoll(isInBattle: true);

            foreach (var hero in HeroesInCombat)
            {
                hero.HasParriedThisTurn = false;
                if (hero.CombatStance != CombatStance.Overwatch)
                {
                    hero.ResetActionPoints();
                    hero.HasBeenTargetedThisTurn = false;
                    hero.HasMadeFirstMoveAction = false;
                    hero.ResetMovementPoints();
                }
                if (CharactersWithProcessedEffectsThisTurn.Add(hero))
                {
                    await StatusEffectService.ProcessActiveStatusEffectsAsync(hero, _powerActivation);
                }
                CharactersWithProcessedEffectsThisTurn.Add(hero);
                if (hero.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.SmiteTheHeretics) 
                    || hero.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.StayThyHand))
                {
                    foreach(var monster in MonstersInCombat)
                    {
                        if (hero.Position != null && monster.Position != null)
                        {
                            int distance = GridService.GetDistance(hero.Position, monster.Position);
                            if (distance <= 4 && monster.TestResolve(RandomHelper.RollDie(DiceType.D100)))
                            {
                                if (hero.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.SmiteTheHeretics))
                                {
                                    await monster.TakeDamageAsync(1, (_floatingText, monster.Position), _powerActivation, ignoreAllArmour: true); 
                                }
                                if (hero.ActiveStatusEffects.Any(e => e.Category == StatusEffectType.StayThyHand))
                                {
                                    monster.CurrentAP = 1;
                                }
                            }
                        }
                    }
                }
            }
            foreach (var monster in MonstersInCombat)
            {
                monster.ResetActionPoints();
                monster.HasMadeFirstMoveAction = false;
                monster.ResetMovementPoints();
                await StatusEffectService.ProcessActiveStatusEffectsAsync(monster, _powerActivation);
                if (CharactersWithProcessedEffectsThisTurn.Add(monster))
                {
                    await StatusEffectService.ProcessActiveStatusEffectsAsync(monster, _powerActivation);
                }
            }

            _initiative.SetupInitiative(HeroesInCombat, MonstersInCombat);
            await ProcessNextInInitiativeAsync();
        }

        /// <summary>
        /// Processes the next actor in the initiative order.
        /// </summary>
        public async Task ProcessNextInInitiativeAsync()
        {
            while (!IsCombatOver)
            {
                _movementHighlighting.ClearHighlights();
                if (_initiative.IsTurnOver())
                {
                    await StartNewTurnAsync();
                    OnCombatStateChanged?.Invoke();
                    return;
                }

                var nextActorType = _initiative.DrawNextToken();

                if (nextActorType == ActorType.Hero)
                {
                    var availableHeroes = HeroesInCombat
                        .Where(h => h.CurrentAP > 0 && h.CombatStance != CombatStance.Overwatch && h.CurrentHP > 0)
                        .ToList();

                    // Check if there are any heroes who can act.
                    if (availableHeroes.Any())
                    {
                        _floatingText.ShowText("Player Turn!", ScreenCenterPosition, "turn-announcement-text");
                        IsAwaitingHeroSelection = true;
                        ActiveHero = null; // Clear the previously active hero
                        CombatLog.Add("Hero's turn. Select an available hero to act.");
                        OnCombatStateChanged?.Invoke();
                        return;
                    }
                    else
                    {
                        // Log it and immediately process the next token in the bag.
                        CombatLog.Add("A hero action was drawn, but no heroes are able to act.");
                        OnCombatStateChanged?.Invoke();
                        continue;
                    }

                }
                else // It's a Monster's turn
                {
                    IsAwaitingHeroSelection = false;
                    ActiveHero = null;

                    var monstersToAct = MonstersInCombat.Except(MonstersThatHaveActedThisTurn).ToList();
                    foreach (var monster in MonstersThatHaveActedThisTurn)
                    {
                        monstersToAct.Remove(monster);
                    }

                    var monsterToAct = SelectMonsterToAct(monstersToAct, HeroesInCombat);
                    if (monsterToAct != null)
                    {
                        _floatingText.ShowText("Monster Turn!", ScreenCenterPosition, "turn-announcement-text");
                        Task.Delay(1000).Wait();
                        monsterToAct.IsVulnerableAfterPowerAttack = false;

                        CombatLog.Add($"{monsterToAct.Name} prepares to act...");
                        if (CharactersWithProcessedEffectsThisTurn.Add(monsterToAct))
                        {
                            await StatusEffectService.ProcessActiveStatusEffectsAsync(monsterToAct, _powerActivation); 
                        }
                        MonstersThatHaveActedThisTurn.Add(monsterToAct);

                        CombatLog.Add(await _monsterAI.ExecuteMonsterTurnAsync(monsterToAct, HeroesInCombat, monsterToAct.Room));
                        OnCombatStateChanged?.Invoke();
                    }
                }

                OnCombatStateChanged?.Invoke();
            }

            if (IsCombatOver)
            {
                await ResolveCombatAsync();

                OnCombatStateChanged?.Invoke();
                return;
            }
        }

        private async Task ResolveCombatAsync()
        {
            CombatLog.Add("Combat is over!");

            if(HeroesInCombat.Any(h => h.CurrentHP > 0))
            {
                _dungeonManager.WinBattle();
                CombatLog.Add("The heroes have won the battle!");
            }

            foreach (var hero in HeroesInCombat)
            {
                HandleEndOfCombatWeaponCoating(hero);

                await HandleEndOfCombatActiveEffectAsync(hero);
            }
        }

        private void HandleEndOfCombatWeaponCoating(Hero hero)
        {
            if (hero.Inventory.EquippedWeapon != null && hero.Inventory.EquippedWeapon.WeaponCoating != null && hero.Inventory.EquippedWeapon.WeaponCoating.RemoveAfterCombat)
            {
                hero.Inventory.EquippedWeapon.WeaponCoating = null;
            }

            foreach (var item in hero.Inventory.Backpack)
            {
                if (item is Weapon weapon && weapon.WeaponCoating != null && weapon.WeaponCoating.RemoveAfterCombat)
                {
                    weapon.WeaponCoating = null;
                }
            }

            foreach (var item in hero.Inventory.QuickSlots)
            {
                if (item is Weapon weapon && weapon.WeaponCoating != null && weapon.WeaponCoating.RemoveAfterCombat)
                {
                    weapon.WeaponCoating = null;
                }
            }
        }

        private async Task HandleEndOfCombatActiveEffectAsync(Hero hero)
        {
            foreach (var effect in hero.ActiveStatusEffects)
            {
                if (effect.Category == StatusEffectType.GodsChampion)
                {
                    hero.CurrentEnergy--;
                    if (hero.CurrentEnergy < 0)
                    {
                        hero.CurrentEnergy = 0;
                        await StatusEffectService.AttemptToApplyStatusAsync(hero,
                            new ActiveStatusEffect(StatusEffectType.NeedRest, -1, statBonus: (BasicStat.Constitution, -(int)Math.Floor(hero.GetStat(BasicStat.Constitution) / 2d))),
                            _powerActivation);
                    }
                }
                //Remove all active combat effects
                if (effect.RemoveAfterCombat)
                {
                    StatusEffectService.RemoveActiveStatusEffect(hero, effect);
                }
                //Activate after effects battle is over
                if (effect.Category == StatusEffectType.Diseased)
                {
                    hero.ActivateDiseasedEffect();
                }
                //update remove after next battle to remove next battle
                if (effect.RemoveAfterNextBattle)
                {
                    effect.RemoveAfterCombat = true;
                }
            }
        }

        private Monster? SelectMonsterToAct(List<Monster> availableMonsters, List<Hero> heroes)
        {
            if (!availableMonsters.Any()) return null;

            // Magic User or Ranged Weapon
            var magicOrRanged = availableMonsters
                .FirstOrDefault(m => m.Spells.Any() || m.Weapons.Any(w => w is RangedWeapon));
            if (magicOrRanged != null) return magicOrRanged;

            // Adjacent to a hero and could make room
            var canMakeRoom = availableMonsters
                .FirstOrDefault(m => IsAdjacentToHero(m, heroes) && CanMakeRoom(m, heroes));
            if (canMakeRoom != null) return canMakeRoom;

            // Adjacent to a hero
            var adjacent = availableMonsters
                .FirstOrDefault(m => IsAdjacentToHero(m, heroes));
            if (adjacent != null) return adjacent;

            // Closest to a hero and can charge
            var canCharge = availableMonsters
                .Where(m => CanCharge(m, heroes))
                .OrderBy(m => GetDistanceToClosestHero(m, heroes))
                .FirstOrDefault();
            if (canCharge != null) return canCharge;

            // Can move its full movement
            var canMoveFull = availableMonsters
                .OrderByDescending(m => m.GetStat(BasicStat.Move)) // Prioritize faster monsters
                .FirstOrDefault(m => CanMoveFullPath(m));
            if (canMoveFull != null) return canMoveFull;

            // Default: return the first available monster if no other condition is met
            return availableMonsters.FirstOrDefault();
        }

        private bool CanMakeRoom(Monster monster, List<Hero> heroes)
        {
            var grid = _dungeon.DungeonGrid;
            var allCharacters = _dungeon.AllCharactersInDungeon;
            if (monster.Position == null) return false;

            // Get all squares the monster can move to
            var reachableSquares = GridService.GetAllWalkableSquares(monster, grid, allCharacters);

            foreach (var potentialPosition in reachableSquares.Keys)
            {
                // Temporarily move the monster to the new position
                var originalPosition = monster.Position;
                monster.Position = potentialPosition;

                // Check if this new position opens up a line of sight for another monster to attack a hero
                foreach (var otherMonster in MonstersInCombat.Where(m => m != monster && m.Position != null))
                {
                    foreach (var hero in heroes)
                    {
                        if (hero.Position != null && otherMonster.Position != null 
                            && GridService.HasLineOfSight(otherMonster.Position, hero.Position, grid).CanShoot)
                        {
                            // If a line of sight is opened, this is a valid "make room" move
                            monster.Position = originalPosition; // Move the monster back
                            return true;
                        }
                    }
                }

                // Move the monster back to its original position
                monster.Position = originalPosition;
            }

            return false;
        }

        private bool CanCharge(Monster monster, List<Hero> heroes)
        {
            var grid = _dungeon.DungeonGrid;
            var target = _monsterAI.ChooseTarget(monster, heroes);
            if (target == null || monster.Position == null || target.Position == null) return false;

            int distance = GridService.GetDistance(monster.Position, target.Position);
            if (GridService.IsAdjacent(monster.Position, target.Position) || distance > monster.GetStat(BasicStat.Move))
            {
                return false;
            }

            // Use GetLine to ensure a straight path for the charge
            var lineOfCharge = GridService.GetLine(monster.Position, target.Position);
            // Check if any square in the line of charge is blocked
            foreach (var square in lineOfCharge)
            {
                var gridSquare = GridService.GetSquareAt(square, grid);
                if (gridSquare == null || gridSquare.MovementBlocked)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanMoveFullPath(Monster monster)
        {
            var grid = _dungeon.DungeonGrid;
            var allCharacters = _dungeon.AllCharactersInDungeon;
            // Use GetAllWalkableSquares to see if the monster has enough open space
            var reachableSquares = GridService.GetAllWalkableSquares(monster, grid, allCharacters);
            return reachableSquares.Count >= monster.GetStat(BasicStat.Move);
        }

        /// <summary>
        /// Helper method to check if a monster is adjacent to any hero.
        /// </summary>
        private bool IsAdjacentToHero(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return false;
            foreach (var hero in heroes.Where(h => h.CurrentHP > 0 && h.Position != null))
            {
                if (hero.Position != null && Math.Abs(monster.Position.X - hero.Position.X) <= 1 &&
                    Math.Abs(monster.Position.Y - hero.Position.Y) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetDistanceToClosestHero(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return int.MaxValue;
            return heroes
                .Where(h => h.Position != null)
                .Min(h => GridService.GetDistance(monster.Position, h.Position!));
        }

        private Hero? GetClosestHero(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return null;
            return heroes
               .Where(h => h.Position != null && h.CurrentHP > 0 && h.ActiveStatusEffects.FirstOrDefault(a => a.Category == StatusEffectType.Pit) != null)
               .OrderBy(h => GridService.GetDistance(monster.Position, h.Position!))
               .FirstOrDefault();
        }

        /// <summary>
        /// Checks if any hero on Overwatch can interrupt a monster moving along a specific path.
        /// </summary>
        /// <param name="movingMonster">The monster that is taking its turn.</param>
        /// <param name="path">The sequence of GridPositions the monster intends to move through.</param>
        /// <returns>The hero that can interrupt, or null if none can.</returns>
        private async Task<bool> HandleMonsterMovement(Monster movingMonster, List<GridPosition> path)
        {
            var interruptingHero = CheckForOverwatchInterrupt(movingMonster, path);
            if (interruptingHero != null)
            {
                CombatLog.Add($"{interruptingHero.Name} on Overwatch spots {movingMonster.Name}!");
                await _playerAction.PerformActionAsync(_dungeon, interruptingHero, ActionType.StandardAttack, movingMonster);
                interruptingHero.CombatStance = CombatStance.Normal;
                OnCombatStateChanged?.Invoke();
                return true; // Movement was interrupted
            }
            return false; // Movement was not interrupted
        }

        /// <summary>
        /// Checks if any hero on Overwatch can interrupt a monster moving along a specific path.
        /// </summary>
        /// <param name="movingMonster">The monster that is taking its turn.</param>
        /// <param name="path">The sequence of GridPositions the monster intends to move through.</param>
        /// <returns>The hero that can interrupt, or null if none can.</returns>
        private Hero? CheckForOverwatchInterrupt(Monster movingMonster, List<GridPosition> path)
        {
            // Get all heroes currently on Overwatch who are able to act.
            var overwatchHeroes = HeroesInCombat
                .Where(h => h.CombatStance == CombatStance.Overwatch && h.CurrentHP > 0)
                .ToList();

            if (!overwatchHeroes.Any())
            {
                return null; // No one is on overwatch, so no interrupt is possible.
            }

            // We check each square in the monster's path (excluding its starting square).
            foreach (var pathSquare in path.Skip(1))
            {
                // Check each hero on overwatch to see if they can interrupt at this square.
                foreach (var hero in overwatchHeroes)
                {
                    var weapon = hero.Inventory.EquippedWeapon;
                    if (weapon == null) continue; // Hero has no weapon to attack with.

                    // Ranged Overwatch Check: Can the hero see the square?
                    // According to the rules, a ranged weapon cannot be used if an enemy is adjacent.
                    if (weapon.IsRanged && hero.Position != null)
                    {
                        // Check for adjacent enemies
                        bool isEnemyAdjacent = HeroesInCombat.Any(h => h.Position != null && GridService.IsAdjacent(hero.Position, h.Position));
                        if (isEnemyAdjacent) continue;

                        var losResult = GridService.HasLineOfSight(hero.Position, pathSquare, _dungeon.DungeonGrid);
                        if (losResult.CanShoot)
                        {
                            // This hero has a clear shot and can interrupt.
                            return hero;
                        }
                    }
                    // Melee Overwatch Check: Did the monster enter the hero's Zone of Control?
                    // Dodging and parrying can only be done if the attack comes through the hero's ZOC
                    // It is implied that Overwatch follows the same principle.
                    else
                    {
                        if (DirectionService.IsInZoneOfControl(pathSquare, hero))
                        {
                            // This hero can make a melee attack and can interrupt.
                            return hero;
                        }
                    }
                }
            }

            // If we've checked every square and no hero could interrupt.
            return null;
        }

        /// <summary>
        /// This NEW public method is called by the UI when a player clicks on a valid hero.
        /// </summary>
        public void SetActiveHeroAsync(Hero hero)
        {
            // Ensure we are in the correct state and the hero is valid
            if (!IsAwaitingHeroSelection || !HeroesInCombat.Contains(hero) || hero.CurrentAP <= 0)
            {
                CombatLog.Add("Invalid hero selection.");
                OnCombatStateChanged?.Invoke();
                return;
            }

            if (ActiveHero == hero)
            {
                return;
            }
            // Set the selected hero as active and exit the selection state
            ActiveHero = hero;
            _movementHighlighting.HighlightWalkableSquares(hero, _dungeon);

            CombatLog.Add($"It's {ActiveHero.Name}'s turn. They have {ActiveHero.CurrentAP} AP.");
            OnCombatStateChanged?.Invoke();
        }

        // This method would be called by the UI when the player selects an action.
        public async Task HeroPerformsActionAsync(ActionType action, object? target = null, object? secondaryTarget = null)
        {
            if (ActiveHero != null && ActiveHero.CurrentAP > 0)
            {
                // Process status effects only once per hero per turn, before their first action.
                if (CharactersWithProcessedEffectsThisTurn.Add(ActiveHero))
                {
                    await StatusEffectService.ProcessActiveStatusEffectsAsync(ActiveHero, _powerActivation);
                    CombatLog.Add($"Processing status effects for {ActiveHero.Name}.");
                }

                _movementHighlighting.ClearHighlights();
                CombatLog.Add(await _playerAction.PerformActionAsync(_dungeon, ActiveHero, action, target, secondaryTarget));
                //if hero performs an action then no other hero can be selected until next hero selection phase
                IsAwaitingHeroSelection = false;

                OnCombatStateChanged?.Invoke();

                if (ActiveHero.CurrentAP <= 0)
                {
                    CombatLog.Add($"{ActiveHero.Name}'s turn is over.");
                    ActiveHero.Facing = await _facing.RequestFacingDirectionAsync(ActiveHero); await Task.Yield();
                    ActiveHero = null;
                    await Task.Yield(); // Allow UI to process modal closing
                    await ProcessNextInInitiativeAsync();
                }
                else _movementHighlighting.HighlightWalkableSquares(ActiveHero, _dungeon);
            }
        }

        internal List<Hero> GetActivatedHeroes()
        {
            var returnList = new List<Hero>();

            var heroes = _dungeon.HeroParty?.Heroes
                .Where(h => h.CurrentHP > 0 && h.CombatStance != CombatStance.Overwatch)
                .ToList() ?? new List<Hero>();

            foreach (var hero in heroes)
            {
                if (!hero.CanAct)
                {
                    returnList.Add(hero);
                }
            }

            return returnList;
        }

        private void HandleTimeFreeze()
        {
            foreach (Hero hero in GetActivatedHeroes())
            {
                if (hero.CurrentAP <= 0)
                {
                    hero.ResetActionPoints();
                    _initiative.AddToken(ActorType.Hero);
                }
            }
        }
    }
}
