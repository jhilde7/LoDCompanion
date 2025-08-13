using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text;

namespace LoDCompanion.BackEnd.Services.Combat
{
    public enum SpecialActiveAbility
    {
        Bellow,
        FreeBellow,
        Camouflage,
        Entangle,
        FireBreath,
        GhostlyHowl,
        Kick,
        Leech,
        MasterOfTheDead,
        MultipleAttack,
        Petrify,
        PoisonSpit,
        Seduction,
        SummonChildren,
        TongueAttack,
        Swallow,
        SweepingStrike,
        Web
    }

    public class MonsterSpecialService
    {
        private readonly UserRequestService _diceRoll;
        private readonly EncounterService _encounter;
        private readonly InitiativeService _initiative;
        private readonly FloatingTextService _floatingText;

        public event Func<Monster, Hero, Task<DefenseResult>>? OnEntangleAttack;
        public event Func<Monster, Hero, Task<DefenseResult>>? OnSwallowAttack;
        public event Func<Monster, Hero, Task<AttackResult>>? OnKickAttack;
        public event Func<Monster, Hero, Task<AttackResult>>? OnSpitAttack;
        public event Func<Monster, List<Hero>, DungeonState, Task<AttackResult>>? OnSweepingStrikeAttack;
        public event Func<Monster, Hero, DungeonState, Task<AttackResult>>? OnTongueAttack;
        public event Func<Monster, Hero, Task<AttackResult>>? OnWebAttack;

        public MonsterSpecialService(
            UserRequestService diceRoll,
            EncounterService encounter,
            InitiativeService initiative,
            FloatingTextService floatingText)
        {
            _diceRoll = diceRoll;
            _encounter = encounter;
            _initiative = initiative;
            _floatingText = floatingText;
        }

        /// <summary>
        /// Executes a specific monster special ability.
        /// </summary>
        /// <param name="monster">The monster performing the special action.</param>
        /// <param name="heroes">The list of heroes targeted by the action.</param>
        /// <param name="abilityType">The type of special ability to execute (e.g., "Bellow", "FireBreath").</param>
        /// <returns>A string describing the outcome of the special action.</returns>
        public async Task<string> ExecuteSpecialAbilityAsync(
            Monster monster,
            List<Hero> heroes,
            Hero target,
            SpecialActiveAbility abilityType,
            DungeonState dungeon)
        {
            switch (abilityType)
            {
                case SpecialActiveAbility.Bellow:
                case SpecialActiveAbility.FreeBellow:
                    return await BellowAsync(monster, heroes);
                case SpecialActiveAbility.Camouflage:
                    return Camouflage(monster, heroes, dungeon);
                case SpecialActiveAbility.Entangle:
                    return await EntangleAsync(monster, target);
                case SpecialActiveAbility.FireBreath:
                    return await FireBreathAsync(monster, target, heroes, dungeon);
                case SpecialActiveAbility.GhostlyHowl:
                    return await GhostlyHowlAsync(monster, heroes);
                case SpecialActiveAbility.Kick:
                    return await KickAsync(monster, heroes);
                case SpecialActiveAbility.MasterOfTheDead:
                    return MasterOfTheDead(monster, dungeon);
                case SpecialActiveAbility.Petrify:
                    return await PetrifyAsync(monster, heroes);
                case SpecialActiveAbility.PoisonSpit:
                    return await PoisonSpitAsync(monster, target);
                case SpecialActiveAbility.Seduction:
                    return await SeductionAsync(monster, target);
                case SpecialActiveAbility.SummonChildren:
                    return SummonChildren(monster, dungeon);
                case SpecialActiveAbility.Swallow:
                    return await SwallowAsync(monster, target);
                case SpecialActiveAbility.SweepingStrike:
                    return await SweepingStrikeAsync(monster, heroes, dungeon);
                case SpecialActiveAbility.TongueAttack:
                    return await TongueAttackAsync(monster, target, dungeon);
                case SpecialActiveAbility.Web:
                    return await WebAsync(monster, target);
                default:
                    return $"{monster.Name} attempts an unknown special ability: {abilityType}. Nothing happens.";
            }
        }

        // --- Individual Special Ability Implementations ---

        public async Task<string> BellowAsync(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} lets out a thunderous bellow!\n";
            foreach (var hero in heroes)
            {
                if (monster.Position != null && hero.Position != null && GridService.GetDistance(monster.Position, hero.Position) <= 4)
                {
                    var rollResult = await _diceRoll.RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
                    hero.CheckPerfectRoll(rollResult.Roll, stat: BasicStat.Resolve);
                    int resolveRoll = rollResult.Roll;
                    if (resolveRoll > hero.GetStat(BasicStat.Resolve))
                    {
                        StatusEffectService.AttemptToApplyStatus(hero, new ActiveStatusEffect(StatusEffectType.Stunned, 1));
                        outcome += $"{hero.Name} is stunned by the roar!\n";
                    }
                    else
                    {
                        outcome += $"{hero.Name} resists the bellow.\n";
                    }
                }
            }
            return outcome;
        }

        private string Camouflage(Monster monster, List<Hero> heroes, DungeonState dungeon)
        {
            var allRooms = dungeon.RoomsInDungeon;
            var heroRooms = new HashSet<Room>();
            foreach (var hero in heroes)
            {
                if (hero.Room != null)
                {
                    heroRooms.Add(hero.Room);
                }
            }

            if (!heroRooms.Any())
            {
                return $"{monster.Name} tries to camouflage, but there's nowhere to hide!";
            }

            var adjacentRooms = new HashSet<Room>();
            foreach (var room in heroRooms)
            {
                foreach (var door in room.Doors)
                {
                    adjacentRooms.UnionWith(door.ConnectedRooms);
                }
            }

            var potentialRooms = heroRooms.Union(adjacentRooms).ToList();
            potentialRooms.Remove(monster.Room); // Can't reappear in the same room it vanished from

            if (!potentialRooms.Any())
            {
                return $"{monster.Name} finds no suitable place to reappear.";
            }

            potentialRooms.Shuffle();
            Room targetRoom = potentialRooms.First();

            // Find a valid, empty square in the target room.
            // This assumes a helper method to get all valid squares.
            var validSquares = GridService.GetAllWalkableSquares(monster, dungeon.DungeonGrid, heroes.Cast<Character>().ToList());
            if (!validSquares.Any())
            {
                return $"{monster.Name} couldn't find an empty space to reappear in {targetRoom.Name}.";
            }

            GridPosition? newPosition = null;

            if (targetRoom.HeroesInRoom != null && !targetRoom.HeroesInRoom.Any())
            {
                // If the room has no heroes, find the spot with the best line of sight.
                int maxLosCount = -1;
                foreach (var square in validSquares.Keys)
                {
                    int currentLosCount = 0;
                    foreach (var hero in heroes)
                    {
                        if (hero.Position != null && GridService.HasLineOfSight(square, hero.Position, dungeon.DungeonGrid).CanShoot)
                        {
                            currentLosCount++;
                        }
                    }
                    if (currentLosCount > maxLosCount)
                    {
                        maxLosCount = currentLosCount;
                        newPosition = square;
                    }
                }
            }
            else
            {
                var validSquareList = validSquares.Keys.ToList();
                validSquareList.Shuffle();
                // If there are heroes, just pick a random valid square.
                newPosition = validSquareList[0];
            }

            if (newPosition != null && monster.Position != null)
            {
                // Remove the monster from its current location
                var oldSquare = GridService.GetSquareAt(monster.Position, dungeon.DungeonGrid);
                if (oldSquare != null)
                {
                    oldSquare.OccupyingCharacterId = null;
                }

                // Place the monster in the new location
                monster.Position = newPosition;
                monster.Room = targetRoom;
                var newSquare = GridService.GetSquareAt(newPosition, dungeon.DungeonGrid);
                if (newSquare != null)
                {
                    newSquare.OccupyingCharacterId = monster.Id;
                }

                return $"{monster.Name} vanishes into the shadows and reappears in {targetRoom.Name}!";
            }

            return $"{monster.Name} tries to camouflage but fails.";
        }

        private async Task<string> EntangleAsync(Monster monster, Hero hero)
        {
            string outcome = $"{monster.Name} attempts to entangle {hero.Name}!\n";

            if (OnEntangleAttack != null)
            {
                var defenseResult = await OnEntangleAttack.Invoke(monster, hero);
                if (defenseResult.WasSuccessful)
                {
                    return outcome + defenseResult.OutcomeMessage;
                }
            }

            StatusEffectService.AttemptToApplyStatus(hero, new ActiveStatusEffect(StatusEffectType.Entangled, 0));
            outcome += $"{hero.Name} is entangled!\n";

            return outcome;
        }

        public async Task<string> FireBreathAsync(Monster monster, Hero target, List<Hero> allHeroes, DungeonState dungeon)
        {
            if (target.Position == null) return string.Empty;
            if (!target.HasDodgedThisBattle)
            {
                var defenseResult = await DefenseService.AttemptDodge(target, _diceRoll);
                if (defenseResult.WasSuccessful)
                {
                    return $"{monster.Name} unleashes a cone of fiery breath, but {target.Name} dives out of the way!";
                }
            }

            var outcome = new StringBuilder($"{monster.Name} unleashes a cone of fiery breath!\n");

            // Apply primary damage to the main target
            int primaryDamage = RandomHelper.RollDie(DiceType.D10);
            target.TakeDamage(primaryDamage, (_floatingText, target.Position), damageType: DamageType.Fire);
            outcome.AppendLine($"{target.Name} is caught in the blast and takes {primaryDamage} fire damage.");

            // Find adjacent squares
            var adjacentSquares = GridService.GetNeighbors(target.Position, dungeon.DungeonGrid);
            var allCharacters = new List<Character>(allHeroes);
            if (dungeon.CurrentRoom?.MonstersInRoom != null)
            {
                allCharacters.AddRange(dungeon.CurrentRoom.MonstersInRoom);
            }


            // Find and damage characters in adjacent squares
            foreach (var character in allCharacters)
            {
                // Skip the primary target and the caster
                if (character == target || character == monster) continue;

                if (adjacentSquares.Contains(character.Position))
                {
                    int splashDamage = RandomHelper.RollDie(DiceType.D6);
                    character.TakeDamage(splashDamage, (_floatingText, target.Position), damageType: DamageType.Fire);
                    outcome.AppendLine($"{character.Name} is caught in the splash and takes {splashDamage} fire damage.");
                }
            }

            return outcome.ToString();
        }

        public async Task<string> GhostlyHowlAsync(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} emits a chilling, ghostly howl!\n";
            foreach (var hero in heroes)
            {
                var rollResult = await _diceRoll.RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
                hero.CheckPerfectRoll(rollResult.Roll, stat: BasicStat.Resolve);
                int resolveRoll = rollResult.Roll;
                if (resolveRoll > hero.GetStat(BasicStat.Resolve) && hero.Position != null)
                {
                    int damage = RandomHelper.RollDie(DiceType.D8);
                    hero.TakeDamage(damage, (_floatingText, hero.Position));
                    hero.CurrentSanity -= 1;
                    outcome += $"{hero.Name} takes {damage} damage and loses 1 Sanity!\n";
                }
                else
                {
                    outcome += $"{hero.Name} remains unfazed.\n";
                }
            }
            return outcome;
        }

        public async Task<string> KickAsync(Monster monster, List<Hero> heroes)
        {
            if (monster.Position == null) return string.Empty;
            var adjacentHeroes = heroes.Where(h => h.Position != null && GridService.IsAdjacent(monster.Position, h.Position) && h.CombatStance != CombatStance.Prone).ToList();
            var heroesBehind = adjacentHeroes.Where(h =>
            {
                if (h.Position != null)
                {
                    var relativeDir = DirectionService.GetRelativeDirection(monster.Facing, monster.Position, h.Position);
                    return relativeDir == RelativeDirection.Back || relativeDir == RelativeDirection.BackLeft || relativeDir == RelativeDirection.BackRight;
                }
                else return false; // Skip heroes without a position
            }).ToList();

            if (heroesBehind.Any())
            {
                string outcome = $"{monster.Name} attempts a powerful kick!\n";
                var kickTarget = heroesBehind[RandomHelper.GetRandomNumber(0, heroesBehind.Count - 1)]; // Select a random hero from those behind
                if (OnKickAttack != null)
                {
                    var result = await OnKickAttack.Invoke(monster, kickTarget);
                    outcome += $"{monster.Name} kicks {kickTarget.Name} from behind for {result.DamageDealt} damage.\n";

                    return outcome;
                }
                else return string.Empty; // event is null
            }
            else
            {
                return string.Empty; // No heroes behind to kick
            }
        }

        public string MasterOfTheDead(Monster monster, DungeonState dungeon)
        {
            // Find a fallen undead ally
            var fallenUndead = dungeon.RevealedMonsters.FirstOrDefault(m => m.IsUndead && m.CurrentHP <= 0);
            var woundedUndead = dungeon.RevealedMonsters
                                .Where(m => m.IsUndead && m.CurrentHP < m.GetStat(BasicStat.HitPoints))
                                .OrderBy(m => m.CurrentHP)
                                .FirstOrDefault();
            if (fallenUndead != null)
            {
                fallenUndead.Heal(fallenUndead.GetStat(BasicStat.HitPoints));
                return $"{monster.Name} uses its dark power to raise {fallenUndead.Name} from the dead!";
            }
            else if (woundedUndead != null)
            {
                int healing = woundedUndead.GetStat(BasicStat.HitPoints) - woundedUndead.CurrentHP;
                woundedUndead.Heal(healing);
                return $" Dark energy knits the wounds of {woundedUndead.Name}, healing {healing} HP.";
            }

            // If no fallen undead, heal self
            if (monster.CurrentHP < monster.GetStat(BasicStat.HitPoints))
            {
                int healing = RandomHelper.RollDie(DiceType.D6);
                monster.Heal(healing);
                return $"{monster.Name} drains life force to heal itself for {healing} HP.";
            }

            // If at full health, perform a standard attack instead
            return $"{monster.Name} finds no target for its dark magic, so performs a standard attack instead";
        }

        public async Task<string> PetrifyAsync(Monster monster, List<Hero> heroes)
        {
            if(monster.Position == null) return string.Empty; // Ensure monster has a position
            string outcome = $"{monster.Name} attempts to turn its targets to stone!\n";

            var adjacentHeroes = heroes.Where(h => h.Position != null && GridService.IsAdjacent(monster.Position, h.Position)).ToList();

            if (adjacentHeroes.Any())
            {
                adjacentHeroes.Shuffle(); // Randomize the order of heroes to target
                var targetHero = adjacentHeroes[0];

                var rollResult = await _diceRoll.RequestRollAsync($"Roll a resolve test for {targetHero.Name} to resist being petrified.", "1d100"); await Task.Yield();
                targetHero.CheckPerfectRoll(rollResult.Roll, stat: BasicStat.Resolve);
                int resolveRoll = rollResult.Roll;

                if (resolveRoll > targetHero.GetStat(BasicStat.Resolve))
                {
                    int duration = RandomHelper.RollDie(DiceType.D6);
                    StatusEffectService.AttemptToApplyStatus(targetHero, new ActiveStatusEffect(StatusEffectType.Petrified, duration));
                    outcome += $"{targetHero.Name} is turned to stone for {duration} turns!\n";
                }
                else
                {
                    outcome += $"{targetHero.Name} resists the petrifying gaze.\n";
                }
            }
            else
            {
                outcome += "There are no adjacent heroes to target.";
            }

            return outcome;
        }

        public async Task<string> PoisonSpitAsync(Monster monster, Hero target)
        {
            string outcome = $"{monster.Name} spits poison at {target.Name}!\n";
            var spitWeapon = new RangedWeapon { Name = "Poison Spit", AmmoType = AmmoType.SlingStone }; // Create a temporary weapon for the attack
            var context = new CombatContext();

            // Use AttackService to see if the attack hits
            if (OnSpitAttack != null)
            {
                var result = await OnSpitAttack.Invoke(monster, target);
                if (result.IsHit)
                {
                    outcome += $"{target.Name} is hit and must resist the poison!\n";
                    // Apply poison effect
                    var rollResult = await _diceRoll.RequestRollAsync("Roll a constitution test to resist the effects", "1d100"); await Task.Yield();
                    int resistRoll = rollResult.Roll;

                    // The logic for applying poison, including the CON test, is now handled in StatusEffectService
                    StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.Poisoned, RandomHelper.RollDie(DiceType.D10) + 1));
                }
                else
                {
                    outcome += result.OutcomeMessage;
                }
            }
            else return string.Empty; // event is null


            return outcome;
        }

        public async Task<string> SeductionAsync(Monster monster, Hero target)
        {
            string outcome = $"{monster.Name} attempts to seduce {target.Name}!\n";

            // The hero must make a RES test.
            outcome += "Roll a resolve test to resist the effects.";
            var rollResult = await _diceRoll.RequestRollAsync("Roll a resolve test to resist the effects", "1d100"); await Task.Yield();
            target.CheckPerfectRoll(rollResult.Roll, stat: BasicStat.Resolve);
            int resolveRoll = rollResult.Roll;

            if (resolveRoll > target.GetStat(BasicStat.Resolve))
            {
                // On failure, the hero is incapacitated.
                StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.Incapacitated, -1)); // -1 for indefinite duration until saved.
                outcome += $"{target.Name} is seduced by {monster.Name} and is incapacitated, losing all AP!\n";
            }
            else
            {
                outcome += $"{target.Name} resists the seduction.\n";
            }

            return outcome;
        }

        public string SummonChildren(Monster monster, DungeonState dungeon)
        {
            string outcome = $"{monster.Name} calls forth its vile offspring!\n";

            var parameters = new Dictionary<string, string> { { "Name", "Giant Spider" }, { "Count", "1" } };
            var summonedSpiders = _encounter.GetEncounterByParams(parameters);

            if (summonedSpiders == null || !summonedSpiders.Any())
            {
                return outcome + "But none answer the call.";
            }

            var newSpider = summonedSpiders.First();

            var roomGrid = monster.Room.Grid.ToList(); // Create a copy to shuffle
            roomGrid.Shuffle();
            GridSquare? placementSquare = roomGrid.FirstOrDefault(sq => !sq.IsOccupied && !sq.IsWall && !sq.MovementBlocked);

            if (placementSquare != null && monster.Room.MonstersInRoom != null)
            {
                newSpider.Position = placementSquare.Position;
                newSpider.Room = monster.Room;

                // Add the new monster to the current combat environment
                monster.Room.MonstersInRoom.Add(newSpider);
                dungeon.RevealedMonsters.Add(newSpider);
                _initiative.AddToken(ActorType.Monster);

                outcome += $"A Giant Spider appears at {placementSquare.Position}!";
            }
            else
            {
                outcome += "But there is no space for it to appear.";
            }

            return outcome;
        }

        public async Task<string> SwallowAsync(Monster monster, Hero target)
        {
            string outcome = $"{monster.Name} attempts to swallow {target.Name} whole!\n";

            if(OnSwallowAttack != null)
            {
                var defenseResult = await OnSwallowAttack.Invoke(monster, target);
                if (defenseResult.WasSuccessful)
                {
                    return outcome + $"{target.Name} dodges the attempt to be swallowed!";
                }
            }

            outcome += $"{target.Name} is caught!\n";
            StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.BeingSwallowed, 2));

            return outcome;
        }

        public async Task<string> SweepingStrikeAsync(Monster monster, List<Hero> heroes, DungeonState dungeon)
        {
            string outcome = $"{monster.Name} performs a wide sweeping strike!\n";
            var heroesInZoc = heroes.Where(h => h.Position != null && DirectionService.IsInZoneOfControl(h.Position, monster)).ToList();

            if (!heroesInZoc.Any())
            {
                return $"{monster.Name} sweeps its weapon, but no heroes are in its reach.";
            }

            if (OnSweepingStrikeAttack != null)
            {
                var result = await OnSweepingStrikeAttack.Invoke(monster, heroes, dungeon);

                return outcome;
            }
            else return string.Empty; // event is null
        }

        public async Task<string> TongueAttackAsync(Monster monster, Hero target, DungeonState dungeon)
        {
            string outcome = $"{monster.Name} lashes out with its tongue at {target.Name}!\n";

            if (OnTongueAttack != null)
            {
                var result = await OnTongueAttack.Invoke(monster, target, dungeon);

                return outcome;
            }
            else return string.Empty; // event is null
        }

        private async Task<string> WebAsync(Monster monster, Hero target)
        {
            string outcome = $"{monster.Name} shoots a web at {target.Name}!\n";

            if(OnWebAttack != null)
            {
                var result = await OnWebAttack.Invoke(monster, target);
                if (result.IsHit)
                {
                    outcome += $"{target.Name} is covered with a sticky web!\n";
                    StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.Ensnared, -1));
                    return outcome;
                }
                else
                {
                    return result.OutcomeMessage;
                }
            }
            return string.Empty;
        }
    }
}