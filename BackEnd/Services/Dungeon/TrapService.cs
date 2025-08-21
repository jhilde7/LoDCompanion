using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;
using Microsoft.AspNetCore.Rewrite;
using System;
using System.Text;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    public enum TrapType
    {
        Arrows,
        PoisonDarts,
        Click,
        CollapsingRoof,
        FireBall,
        Labyrinth,
        Mimic,
        PoisonGas,
        SpearTrap,
        TrapDoor,
        Skeletons
    }
    public class Trap
    {
        public TrapType Name { get; set; } = TrapType.Arrows;
        public int SkillModifier { get; set; } // Modifier for detecting/avoiding the trap
        public int DisarmModifier { get; set; } // Modifier for disarming the trap
        public string Description { get; set; } = string.Empty;
        public string? SpecialDescription { get; set; } // Description of the special effect if triggered
        public int SanityLoss { get; set; } = 2; // Default sanity loss when a trap is triggered
        public string DamageDice { get; set; } = "1d6"; // Default damage dice for the trap
        public GridPosition? Position { get; set; } // Position of the trap in the dungeon

        public Trap()
        {
            GetRandomTrap();
        }
        public Trap(TrapType name, int skillModifier, int disarmModifier, string description, string? specialDescription = null)
        {
            Name = name;
            SkillModifier = skillModifier;
            DisarmModifier = disarmModifier;
            Description = description;
            SpecialDescription = specialDescription;
        }

        public override string ToString()
        {
            return "Trap: " + Name + 
                   "| Description: " + Description +
                   "| Skill Modifier: " + SkillModifier +
                   "| Disarm Modifier: " + DisarmModifier +
                   "| Special Effect: " + (SpecialDescription ?? "None") +
                   "| Damage Dice: " + DamageDice;
        }

        // Static factory method to create common trap types
        public Trap GetRandomTrap(bool isChest = false)
        {
            var dice = DiceType.D10;
            if (isChest) dice = DiceType.D12;

            int roll = RandomHelper.RollDie(dice);
            return roll switch
            {
                1 => new Trap(TrapType.Arrows, 0, 5, "With a whizzing sound a set of arrows is fired across the room!", "1d6 arrows are shot.") { DamageDice = "1d8" },
                2 => new Trap(TrapType.PoisonDarts, 0, -5, "Small, razor-sharp darts shoot across the room.", "1d6 darts are shot. Each dart has risk of poison.") { DamageDice = "1d6" },
                3 => new Trap(TrapType.Click, 0, 0, "Totally oblivious to the impending danger, you are saved by a malfunctioning mechanism.", "No trap is sprung, and you may continue the turn as if nothing has happened."),
                4 => new Trap(TrapType.CollapsingRoof, 0, -10, "With a terrifying rumble, parts of the roof collapse upon the heroes.", "Randomize 1d6 squares on the tile that are struck by falling debris. Any character in such a square takes DMG unless they succeed on a DEX test.") { DamageDice = "3d6" },
                5 => new Trap(TrapType.FireBall, 5, -10, "The darkness is suddenly replaced by bright light as a fireball shoots from one side of the wall.", "Randomize a square on the tile that will be hit. Any character in that square takes fire DMG. Adjacent characters take half fire DMG.") { DamageDice = "1d12" },
                6 => new Trap(TrapType.Labyrinth, 0, +15, "Somehow you have triggered a mechanism that seems to change the very design of the dungeon.", "Add 2 random room cards on top of each pile of room cards."),
                7 => new Trap(TrapType.PoisonGas, 5, 0, "Suddenly, all doors to the tile slam shut as gas starts to fill the chamber.", "Doors must be broken down (20 HP). Each turn, characters must pass a CON test or take 1d3 DMG.") { DamageDice = "1d3" },
                8 => new Trap(TrapType.SpearTrap, 5, 10, "The floor gives way revealing a deep pit filled with sharp stakes.", "A random character must pass a DEX test or fall.") { DamageDice = "2d6" },
                9 => new Trap(TrapType.Skeletons, 10, -10, "Small niches appear in the walls, and a number of dusty old skeletons appear.", "Place 1d6+2 skeletons (broadswords, shields, armour 1) randomly along the walls."),
                10 => new Trap(TrapType.TrapDoor, 5, 10, "Without much warning, the floor gives way as a trap door opens.", "A random character must pass a DEX test or fall, taking 1d10 DMG (no armour/NA).") { DamageDice = "1d10" },
                11 => new Trap(TrapType.Mimic, 10, 0, "All is not what it seems. The chest you tried to open starts to writhe and a huge maw opens with razor sharp teeth.", "Only applicable to chests. Cannot be disarmed. If noticed, the party avoids it."),
                _ => GetRandomTrap() // Fallback to get another random trap if the roll is out of range
            };

        }
    }

    public class TrapService
    {
        private readonly UserRequestService _diceRoll;
        private readonly PowerActivationService _powerActivation;
        private readonly DungeonManagerService _dungeonManager;

        public TrapService (UserRequestService diceRoll, PowerActivationService powerActivation, DungeonManagerService dungeonManager)
        {
            _diceRoll = diceRoll;
            _powerActivation = powerActivation;
            _dungeonManager = dungeonManager;
        }


        /// <summary>
        /// Checks if a hero successfully detects a trap based on their Perception.
        /// </summary>
        /// <param name="hero">The hero attempting to detect the trap.</param>
        /// <param name="trap">The trap to be detected.</param>
        /// <returns>True if the trap is detected, false otherwise.</returns>
        public async Task<bool> DetectTrapAsync(Hero hero, Trap trap)
        {
            var perceptionSkill = hero.GetSkill(Skill.Perception);
            if (await _powerActivation.RequestPerkActivationAsync(hero, PerkName.SixthSense))
            {
                perceptionSkill += 20;
            }

            int perceptionRoll = RandomHelper.RollDie(DiceType.D100);
            return perceptionRoll <= 80 && perceptionRoll <= perceptionSkill + trap.SkillModifier;
        }

        /// <summary>
        /// Attempts to disarm a detected trap using the hero's Pick Lock skill.
        /// </summary>
        /// <param name="hero">The hero attempting to disarm the trap.</param>
        /// <param name="trap">The trap to be disarmed.</param>
        /// <returns>True if the trap is successfully disarmed.</returns>
        public async Task<bool> DisarmTrapAsync(Hero hero, Trap trap)
        {
            var rollResult = await _diceRoll.RequestRollAsync("Roll pick locks test", "1d100");
            int trapDisarmTarget = hero.GetSkill(Skill.PickLocks) + trap.DisarmModifier;

            var cleverFingers = hero.ActiveStatusEffects.FirstOrDefault(e => e.Category == Combat.StatusEffectType.CleverFingers);
            if (cleverFingers != null) hero.ActiveStatusEffects.Remove(cleverFingers);

            if (rollResult.Roll <= 80 && rollResult.Roll <= trapDisarmTarget)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Triggers the trap's effect on a hero. In a full implementation, this would apply damage or status effects.
        /// </summary>
        /// <param name="hero">The hero who triggered the trap.</param>
        /// <param name="trap">The trap that was triggered.</param>
        /// <returns>A string describing the outcome of the trap.</returns>
        public async Task<string> TriggerTrapAsync(Character character, Trap trap, Chest? chest = null)
        {
            if (trap.Name == TrapType.Click) return $"{character.Name} triggered a trap, {trap.Description}.";
            if (character.Position == null) return $"{character.Name} is not in a valid position to trigger a trap.";

            if (character is Hero hero && await DetectTrapAsync(hero, trap))
            {
                var choiceResult = await _diceRoll.RequestYesNoChoiceAsync($"Trap was detected. Does {hero.Name} wish to attempt to disarm the trap?");
                if (choiceResult)
                {
                    if (await DisarmTrapAsync(hero, trap))
                    {
                        return $"{hero.Name} successfully disarmed the {trap.Name} trap!";
                    }
                }
                else
                {
                    trap.Position = hero.Position;
                    return $"{hero.Name} chose not to disarm the {trap.Name} trap.";
                }
            }

            if(chest != null) trap.Position = chest.Position;
            else trap.Position = character.Position;

            var outcome = new StringBuilder();
            bool newTrap = false;
            while (newTrap)
            {
                switch (trap.Name)
                {
                    case TrapType.Arrows:
                    case TrapType.PoisonDarts:
                        outcome.AppendLine(trap.Description);
                        outcome.Append(await HandleProjectileTrapAsync(character, trap));
                        newTrap = false;
                        break;
                    case TrapType.CollapsingRoof:
                        outcome.AppendLine(trap.Description);
                        outcome.Append(await HandleCollapsingRoofTrapAsync(character, trap));
                        newTrap = false;
                        break;
                    case TrapType.FireBall:
                        outcome.AppendLine(trap.Description);
                        outcome.AppendLine(await HandleFireBallTrapAsync(character, trap));
                        newTrap = false;
                        break;
                    case TrapType.Labyrinth:
                        outcome.AppendLine(trap.Description);
                        _dungeonManager.AddExplorationCardsToPiles(2);
                        newTrap = false;
                        break;
                    case TrapType.Mimic:
                        if (chest != null)
                        {
                            outcome.AppendLine(trap.Description);
                            _dungeonManager.SpawnMimicEncounter(chest);
                            newTrap = false;
                        }
                        else
                        {
                            newTrap = true;
                        }
                        break;
                    case TrapType.PoisonGas:
                        outcome.AppendLine(trap.Description);
                        outcome.AppendLine(HandlePoisonGasTrap(character));
                        newTrap = false;
                        break;
                    case TrapType.TrapDoor:
                    case TrapType.SpearTrap:
                        outcome.AppendLine(trap.Description);
                        outcome.AppendLine(await HandlePitTrapAsync(character, trap));
                        newTrap = false;
                        break;
                    case TrapType.Skeletons:
                        outcome.AppendLine(trap.Description);
                        _dungeonManager.SpawnSkeletonsTrapEncounter(character.Room, RandomHelper.RollDice(trap.DamageDice) + 2);
                        newTrap = false;
                        break;
                } 
            }

            if (character is Hero)
            {
                hero = (Hero)character;
                await hero.TakeSanityDamage(trap.SanityLoss, (new FloatingTextService(), hero.Position), _powerActivation);
                if (hero.Party != null && hero.Party.PartyManager != null) hero.Party.PartyManager.UpdateMorale(changeEvent: MoraleChangeEvent.SprungTrap); 
            }
            return outcome.ToString();
        }

        private async Task<string> HandlePitTrapAsync(Character character, Trap trap)
        {
            var randomCharacter = character.Room.CharactersInRoom
                .FirstOrDefault(c => c.Position == GetRandomPositions(character, 1)
                .FirstOrDefault(k => c.Position != null && k.Key == c.Position).Key);
            if (randomCharacter != null)
            {
                int roll = 0;
                if (randomCharacter is Hero)
                {
                    var hero = (Hero)randomCharacter;
                    var rollResult = await _diceRoll.RequestRollAsync($"Roll DEX test for {hero.Name} to avoid spear trap.", "1d100",
                        stat: (hero, BasicStat.Dexterity));
                    await Task.Yield();
                    roll = rollResult.Roll;
                }
                else
                {
                    roll = RandomHelper.RollDie(DiceType.D100);
                }

                if (!randomCharacter.TestDexterity(roll))
                {
                    int damage = RandomHelper.RollDice(trap.DamageDice);
                    if (trap.Name == TrapType.SpearTrap)
                    {
                        damage = await randomCharacter.TakeDamageAsync(damage, (new FloatingTextService(), randomCharacter.Position), _powerActivation); 
                    }
                    else if (trap.Name == TrapType.TrapDoor)
                    {
                        damage = await randomCharacter.TakeDamageAsync(damage, (new FloatingTextService(), randomCharacter.Position), _powerActivation, ignoreAllArmour: true);
                    }
                    await StatusEffectService.AttemptToApplyStatusAsync(randomCharacter, new ActiveStatusEffect(StatusEffectType.Pit, -1), _powerActivation);
                    return $"{randomCharacter.Name} falls into the pit and takes {damage} damage!";
                }
                else
                {
                    return $"{randomCharacter.Name} successfully dodged the spear trap!";
                }
            }

            return $"Could not find a character to trigger the spear trap.";
        }

        private static string HandlePoisonGasTrap(Character character)
        {
            if (character.Room.Doors.Where(d => d.State == DoorState.BashedDown).Any())
            {
                character.Room.Doors.ForEach(d => d.State = DoorState.Closed);
                character.Room.Doors.ForEach(d => d.Lock.LockHP = 20);
                character.Room.CharactersInRoom.ForEach(c => c.ActiveStatusEffects.Add(new ActiveStatusEffect(StatusEffectType.PoisonGas, -1)));
                return string.Empty;
            }
            else
            {
                return "This room has a bashed down door, so the trap has no effect.";
            }
        }

        private async Task<string> HandleFireBallTrapAsync(Character character, Trap trap)
        {
            var outcome = new StringBuilder();
            var positionKVP = GetRandomPositions(character, 1)[0];
            if (character.Position == null) return $"{character.Name} is not in a valid position to trigger the fireball trap.";
            var targetSquare = positionKVP.Value;

            int damage = RandomHelper.RollDice(trap.DamageDice);
            await character.TakeDamageAsync(damage, (new FloatingTextService(), character.Position), _powerActivation, damageType: DamageType.Fire);
            outcome.AppendLine($"{character.Name} is hit by a fireball for {damage} damage!");

            var adjacentPositions = GridService.GetNeighbors(character.Position, character.Room.Grid).ToList();
            foreach (var splashPosition in adjacentPositions.Where(p => GridService.GetSquareAt(p, character.Room.Grid)?.OccupyingCharacterId != null))
            {
                var target = character.Room.CharactersInRoom.FirstOrDefault(c => c.Position != null && c.Position.Equals(splashPosition));
                if (target != null)
                {
                    int splashDamage = (int)Math.Ceiling(RandomHelper.RollDice(trap.DamageDice) / 2d);
                    await target.TakeDamageAsync(splashDamage, (new FloatingTextService(), target.Position), _powerActivation, damageType:DamageType.Fire);
                    outcome.AppendLine($"{character.Name} is hit by the fireballs splash for {damage} damage!");
                }
            }
            return outcome.ToString();
        }

        private async Task<string> HandleCollapsingRoofTrapAsync(Character character, Trap trap)
        {
            int roll = RandomHelper.RollDie(DiceType.D6);
            StringBuilder outcome = new StringBuilder();
            var gridListKVP = GetRandomPositions(character, roll);
            foreach (var itemKVP in gridListKVP)
            {
                var square = itemKVP.Value;
                if (square.IsWall || square.Furniture?.Name == "Pillar") continue;
                outcome.AppendLine($"A section of the roof collapses on {square.Position}. Any character in this square takes {trap.DamageDice} damage.");
                foreach (var target in character.Room.CharactersInRoom.Where(c => c.Position != null && c.Position.Equals(square.Position)))
                {
                    if (character is Hero)
                    {
                        var rollResult = await _diceRoll.RequestRollAsync(
                            $"Roll DEX test for {character.Name} to avoid damage from collapsing roof.", "1d100",
                            stat: ((Hero)character, BasicStat.Dexterity));
                        await Task.Yield();
                        roll = rollResult.Roll;
                    }
                    else
                    {
                        roll = RandomHelper.RollDie(DiceType.D100);
                    }

                    if (!character.TestDexterity(roll))
                    {
                        int damage = RandomHelper.RollDice(trap.DamageDice);
                        await character.TakeDamageAsync(damage, (new FloatingTextService(), character.Position), _powerActivation);
                    }
                    else
                    {
                        if (character.Position != null)
                        {
                            var adjacentPositions = GridService.GetNeighbors(character.Position, character.Room.Grid).ToList();
                            adjacentPositions.Shuffle();
                            foreach (var adjacentPosition in adjacentPositions)
                            {
                                if (GridService.GetSquareAt(adjacentPosition, character.Room.Grid) is { IsWall: false, OccupyingCharacterId: null })
                                {
                                    outcome.AppendLine($"{character.Name} dodges the falling debris and moves to {adjacentPosition}.");
                                    GridService.MoveCharacterToPosition(character, adjacentPosition, character.Room.Grid);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return outcome.ToString();
        }

        private static List<KeyValuePair<GridPosition, GridSquare>> GetRandomPositions(Character character, int amount)
        {
            var gridList = character.Room.Grid.ToList();
            gridList.Shuffle();
            var selectedPositions = new List<KeyValuePair<GridPosition, GridSquare>>();
            for (int i = 0; i < amount; i++)
            {
                selectedPositions.Add(gridList[i]);
            }
            return selectedPositions;
        }

        /// <summary>
        /// Handles the logic for firing a volley of projectiles across a room based on entry point.
        /// </summary>
        private async Task<string> HandleProjectileTrapAsync(Character character, Trap trap)
        {
            if (character.Position == null) return $"{character.Name} is not in a valid position to trigger a trap.";
            var pointOfEntry = character.Room.Doors
                .Where(doors => doors.State == DoorState.Open)
                .OrderBy(d => GridService.GetDistance(character.Position, d.PassagewaySquares[0]))
                .FirstOrDefault();

            var entryPosition = pointOfEntry?.PassagewaySquares[0] ?? character.Position;
            var outcome = new StringBuilder();
            var room = character.Room;
            var roomCenter = new GridPosition(room.GridOffset.X + room.Width / 2, room.GridOffset.Y + room.Height / 2, room.GridOffset.Z);

            // Determine which wall the entry point is on to find the firing edge.
            int dx = entryPosition.X - roomCenter.X;
            int dy = entryPosition.Y - roomCenter.Y;

            List<GridPosition> firingSquares = new List<GridPosition>();
            GridPosition fireDirection = new GridPosition(0, 0, 0);

            if (Math.Abs(dx) > Math.Abs(dy)) // Entry is on East or West wall
            {
                if (dx > 0) // Entry is East, so fire from North wall (relative left)
                {
                    fireDirection = new GridPosition(0, 1, 0); // South
                    for (int x = 0; x < room.Width; x++) firingSquares.Add(new GridPosition(room.GridOffset.X + x, room.GridOffset.Y, room.GridOffset.Z));
                }
                else // Entry is West, so fire from South wall (relative left)
                {
                    fireDirection = new GridPosition(0, -1, 0); // North
                    for (int x = 0; x < room.Width; x++) firingSquares.Add(new GridPosition(room.GridOffset.X + x, room.GridOffset.Y + room.Height - 1, room.GridOffset.Z));
                    firingSquares.Reverse(); // Start from the "far corner"
                }
            }
            else // Entry is on North or South wall
            {
                if (dy > 0) // Entry is North, so fire from West wall (relative left)
                {
                    fireDirection = new GridPosition(1, 0, 0); // East
                    for (int y = 0; y < room.Height; y++) firingSquares.Add(new GridPosition(room.GridOffset.X, room.GridOffset.Y + y, room.GridOffset.Z));
                    firingSquares.Reverse(); // Start from the "far corner"
                }
                else // Entry is South, so fire from East wall (relative left)
                {
                    fireDirection = new GridPosition(-1, 0, 0); // West
                    for (int y = 0; y < room.Height; y++) firingSquares.Add(new GridPosition(room.GridOffset.X + room.Width - 1, room.GridOffset.Y + y, room.GridOffset.Z));
                }
            }

            // Roll for the number of projectiles
            int numberOfProjectiles = RandomHelper.RollDie(DiceType.D6);
            outcome.AppendLine($"With a whizzing sound, {numberOfProjectiles} {(trap.Name == TrapType.Arrows ? "arrows" : "darts")} are fired across the room!");

            // Step 3: Fire from every other square
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                int firingIndex = i * 2; // Fire from squares 0, 2, 4, etc.
                if (firingIndex < firingSquares.Count)
                {
                    outcome.AppendLine(await FireSingleProjectileAsync(firingSquares[firingIndex], fireDirection, room, trap));
                }
            }

            return outcome.ToString();
        }

        /// <summary>
        /// Traces the path of a single projectile until it hits something.
        /// </summary>
        private async Task<string> FireSingleProjectileAsync(GridPosition startPos, GridPosition direction, Room room, Trap trap)
        {
            var currentPos = startPos;
            if(_dungeonManager.Dungeon == null) return "Dungeon is not initialized.";

            while (true)
            {
                var square = GridService.GetSquareAt(currentPos, _dungeonManager.Dungeon.DungeonGrid);
                if (square == null || square.IsWall || square.Furniture?.Name == "Pillar")
                {
                    return "A projectile strikes a wall.";
                }

                var target = room.CharactersInRoom.FirstOrDefault(c => c.Position != null && c.Position.Equals(currentPos));
                if (target != null)
                {
                    int damage = RandomHelper.RollDice(trap.DamageDice);
                    await target.TakeDamageAsync(damage, (new FloatingTextService(), target.Position), _powerActivation);
                    string message = $"{target.Name} is hit by a projectile for {damage} damage!";

                    if (trap.Name == TrapType.PoisonDarts)
                    {
                        message += await StatusEffectService.AttemptToApplyStatusAsync(target, new ActiveStatusEffect(StatusEffectType.Poisoned, -1), _powerActivation);
                    }
                    return message;
                }

                currentPos = currentPos.Add(direction);
            }
        }
    }
}
