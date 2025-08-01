using LoDCompanion.Utilities;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Combat;
using LoDCompanion.Services.GameData;
using System;
using LoDCompanion.Services.Dungeon;
using System.Threading.Tasks;
using LoDCompanion.Models.Dungeon;
using System.Xml.Linq;
using System.Text;

namespace LoDCompanion.Services.Combat
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

        public event Func<Monster, Hero, Task<DefenseResult>>? OnEntangleAttack;
        public event Func<Monster, Hero, Task<AttackResult>>? OnKickAttack;

        public MonsterSpecialService(UserRequestService diceRoll)
        {
            _diceRoll = diceRoll;
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
                case SpecialActiveAbility.MultipleAttack:
                    // This one needs a specific count, assume it's passed with the monster's state or handled externally
                    // For now, let's use a default or an assumed property on monster.
                    int attackCount = 1; // Default
                    if (monster.SpecialRules.Contains("Multiple Attack x2")) attackCount = 2;
                    else if (monster.SpecialRules.Contains("Multiple Attack x3")) attackCount = 3;
                    // ... and so on for other counts
                    return MultipleAttack(monster, heroes, attackCount);
                case SpecialActiveAbility.Petrify:
                    return Petrify(monster, heroes);
                case SpecialActiveAbility.PoisonSpit:
                    return PoisonSpit(monster, heroes);
                case SpecialActiveAbility.Seduction:
                    return Seduction(monster, heroes);
                case SpecialActiveAbility.SummonChildren:
                    return SummonChildren(monster, heroes);
                case SpecialActiveAbility.TongueAttack:
                    return TongueAttack(monster, heroes);
                case SpecialActiveAbility.Swallow:
                    return Swallow(monster, heroes);
                case SpecialActiveAbility.SweepingStrike:
                    return SweepingStrike(monster, heroes);
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
                if (GridService.GetDistance(monster.Position, hero.Position) <= 4)
                {
                    int resolveRoll = await _diceRoll.RequestRollAsync("Roll a resolve test to resist the effects", "1d100");
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
            var validSquares = GridService.GetAllWalkableSquares(targetRoom, monster, dungeon.DungeonGrid);
            if (!validSquares.Any())
            {
                return $"{monster.Name} couldn't find an empty space to reappear in {targetRoom.Name}.";
            }

            GridPosition? newPosition = null;

            if (targetRoom.HeroesInRoom != null && !targetRoom.HeroesInRoom.Any())
            {
                // If the room has no heroes, find the spot with the best line of sight.
                int maxLosCount = -1;
                foreach (var square in validSquares)
                {
                    int currentLosCount = 0;
                    foreach (var hero in heroes)
                    {
                        if (GridService.HasLineOfSight(square, hero.Position, dungeon.DungeonGrid).CanShoot)
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
                // If there are heroes, just pick a random valid square.
                newPosition = validSquares[RandomHelper.GetRandomNumber(0, validSquares.Count - 1)];
            }

            if (newPosition != null)
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
            target.TakeDamage(primaryDamage, DamageType.Fire);
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
                    character.TakeDamage(splashDamage, DamageType.Fire);
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
                int resolveRoll = await _diceRoll.RequestRollAsync("Roll a resolve test to resist the effects", "1d100");
                if (resolveRoll > hero.GetStat(BasicStat.Resolve))
                {
                    int damage = RandomHelper.RollDie(DiceType.D8);
                    hero.TakeDamage(damage);
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
            var adjacentHeroes = heroes.Where(h => GridService.GetDistance(monster.Position, h.Position) <= 1 && h.CombatStance != CombatStance.Prone).ToList();
            var heroesBehind = adjacentHeroes.Where(h =>
            {
                var relativeDir = DirectionService.GetRelativeDirection(monster.Facing, monster.Position, h.Position);
                return relativeDir == RelativeDirection.Back || relativeDir == RelativeDirection.BackLeft || relativeDir == RelativeDirection.BackRight;
            }).ToList();

            if (heroesBehind.Any())
            {
                string outcome = $"{monster.Name} attempts a powerful kick!\n";
                var kickTarget = heroesBehind[RandomHelper.GetRandomNumber(0, heroesBehind.Count - 1)]; // Select a random hero from those behind
                var result = await OnKickAttack.Invoke(monster, kickTarget);

                outcome += $"{monster.Name} kicks {kickTarget.Name} from behind for {result.DamageDealt} damage.\n";

                return outcome;
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

        public string MultipleAttack(Monster monster, List<Hero> heroes, int attackCount)
        {
            string outcome = $"{monster.Name} unleashes {attackCount} rapid attacks!\n";
            //var monsterCombatService = new MonsterCombatService(this); // Assuming this service is available or injected
            for (int i = 0; i < attackCount; i++)
            {
                if (heroes.Count > 0)
                {
                    var target = heroes[RandomHelper.GetRandomNumber(0, heroes.Count - 1)]; // Target a random hero
                    // This would call the regular attack logic from MonsterCombatService
                    // For example: monsterCombatService.ProcessPhysicalAttack(monster, target, monster.Weapons[0]);
                    int damage = RandomHelper.GetRandomNumber(1, 6) + monster.GetStat(BasicStat.DamageBonus); // Simplified damage
                    target.TakeDamage(damage);
                    outcome += $"  Attack {i + 1}: {monster.Name} hits {target.Name} for {damage} damage.\n";
                }
            }
            return outcome;
        }

        public string Petrify(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts to turn its targets to stone!\n";
            foreach (var hero in heroes)
            {
                int conRoll = RandomHelper.GetRandomNumber(1, 20);
                if (conRoll < hero.GetStat(BasicStat.Constitution)) // Example: Constitution check to resist petrification
                {
                    StatusEffectService.AttemptToApplyStatus(hero, new ActiveStatusEffect(StatusEffectType.Petrified, RandomHelper.RollDie(DiceType.D6)));
                    outcome += $"{hero.Name} is turned to stone!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the petrifying gaze.\n";
                }
            }
            return outcome;
        }

        public string PoisonSpit(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} spits corrosive poison!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target
                int damage = RandomHelper.GetRandomNumber(1, 4); // Initial damage
                target.TakeDamage(damage);
                StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.Poisoned, RandomHelper.RollDie(DiceType.D10) + 1));
                outcome += $"{target.Name} is hit for {damage} damage and is poisoned!\n";
            }
            return outcome;
        }

        public string Regenerate(Monster monster, List<Hero> heroes)
        {
            int regenAmount = RandomHelper.GetRandomNumber(1, 6); // Example regeneration amount
            monster.CurrentHP += regenAmount;
            if (monster.CurrentHP > monster.GetStat(BasicStat.HitPoints)) monster.CurrentHP = monster.GetStat(BasicStat.HitPoints);
            return $"{monster.Name} regenerates {regenAmount} HP!\n";
        }

        public string RiddleMaster(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} poses a perplexing riddle!\n";
            // This would typically involve a UI interaction and a wisdom/intellect check.
            // For now, a placeholder.
            outcome += "The heroes must answer correctly or face consequences...\n";
            return outcome;
        }

        public string Seduction(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts to charm a hero!\n";
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < hero.GetStat(BasicStat.Resolve)) // Example: Resolve check to resist charm
                {
                    //hero.ActiveStatusEffect.Add("Charmed"); // Or some other effect
                    outcome += $"{hero.Name} is charmed by {monster.Name}!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the charm.\n";
                }
            }
            return outcome;
        }

        public string Stupid(Monster monster, List<Hero> heroes)
        {
            // This typically implies a debuff on the monster, or a special action that fails.
            // If it's a monster's "action", it usually means they do nothing or a simple attack.
            return $"{monster.Name} stares blankly. It's too stupid to do anything complex this turn, simply attacks if able.\n";
        }

        public string SummonChildren(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} calls forth its vile offspring!\n";
            // Similar to MasterOfTheDead, this would involve creating new Monster instances.
            outcome += "Several smaller monsters emerge!\n"; // Placeholder
            return outcome;
        }

        public string TongueAttack(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} lashes out with a sticky tongue!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target
                int damage = RandomHelper.GetRandomNumber(1, 4);
                target.TakeDamage(damage);
                outcome += $"{target.Name} is hit for {damage} damage and ensnared by the tongue!\n";
                StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.Ensnared, -1)); // Apply status effect
            }
            return outcome;
        }

        public string Swallow(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} attempts to swallow a hero whole!\n";
            if (heroes.Count > 0)
            {
                var target = heroes[0]; // Assume single target
                // This would involve a grapple/strength check and potentially instant death or heavy damage
                int swallowRoll = RandomHelper.GetRandomNumber(1, 20);
                if (swallowRoll > target.GetStat(BasicStat.Dexterity)) // Example: Dexterity check to avoid being swallowed
                {
                    StatusEffectService.AttemptToApplyStatus(target, new ActiveStatusEffect(StatusEffectType.BeingSwallowed, -1)); // Apply swallowed status (e.g., for ongoing damage)
                    outcome += $"{target.Name} is swallowed by {monster.Name}!\n";
                }
                else
                {
                    outcome += $"{target.Name} narrowly avoids being swallowed.\n";
                }
            }
            return outcome;
        }

        public string SweepingStrike(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} performs a wide sweeping strike!\n";
            int damage = RandomHelper.GetRandomNumber(1, 8); // Example AOE damage
            foreach (var hero in heroes)
            {
                hero.TakeDamage(damage);
                outcome += $"{hero.Name} takes {damage} damage from the sweeping strike.\n";
            }
            return outcome;
        }

        public string ApplyFear(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} emanates an aura of fear!\n";
            int fearLevel = 1; // Default, or get from monster.FearLevel property
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < (hero.GetStat(BasicStat.Resolve) - fearLevel)) // Example: Resolve vs. FearLevel
                {
                    StatusEffectService.AttemptToApplyStatus(hero, new ActiveStatusEffect(StatusEffectType.Fear, -1));
                    outcome += $"{hero.Name} is gripped by fear!\n";
                }
                else
                {
                    outcome += $"{hero.Name} resists the fear.\n";
                }
            }
            return outcome;
        }

        public string ApplyTerror(Monster monster, List<Hero> heroes)
        {
            string outcome = $"{monster.Name} inspires abject terror!\n";
            int terrorLevel = 2; // Default, or get from monster.TerrorLevel property
            foreach (var hero in heroes)
            {
                int resolveRoll = RandomHelper.GetRandomNumber(1, 20);
                if (resolveRoll < (hero.GetStat(BasicStat.Resolve) - terrorLevel)) // Example: Resolve vs. TerrorLevel
                {
                    StatusEffectService.AttemptToApplyStatus(hero, new ActiveStatusEffect(StatusEffectType.Terror, -1));
                    outcome += $"{hero.Name} is terrified and tries to flee!\n";
                }
                else
                {
                    outcome += $"{hero.Name} manages to overcome the terror.\n";
                }
            }
            return outcome;
        }

        internal List<SpecialActiveAbility> GetSpecialAttacks(List<string> specialRules)
        {
            var activeAbilities = new List<SpecialActiveAbility>();

            if (specialRules == null)
            {
                return activeAbilities;
            }

            foreach (string ruleString in specialRules)
            {
                if (Enum.TryParse<SpecialActiveAbility>(ruleString, true, out var ability))
                {
                    activeAbilities.Add(ability);
                }
            }

            return activeAbilities;
        }
    }
}