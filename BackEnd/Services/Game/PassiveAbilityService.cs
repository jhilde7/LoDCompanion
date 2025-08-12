using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Utilities;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LoDCompanion.BackEnd.Services.Game
{
    public class PassiveAbility
    {
        public string Description { get; set; } = string.Empty;
    }

    public class PassiveAbilityService
    {
        public List<Talent> Talents => GetTalents();
        public List<Talent> PhysicalTalents => GetTalentsByCategory(TalentCategory.Physical);
        public List<Talent> CombatTalents => GetTalentsByCategory(TalentCategory.Combat);
        public List<Talent> FaithTalents => GetTalentsByCategory(TalentCategory.Faith);
        public List<Talent> AlchemistTalents => GetTalentsByCategory(TalentCategory.Alchemist);
        public List<Talent> CommonTalents => GetTalentsByCategory(TalentCategory.Common);
        public List<Talent> MagicTalents => GetTalentsByCategory(TalentCategory.Magic);
        public List<Talent> SneakyTalents => GetTalentsByCategory(TalentCategory.Sneaky);
        public List<Talent> MentalTalents => GetTalentsByCategory(TalentCategory.Mental);
        public List<MonsterPassiveSpecial> MonsterPassiveSpecials => GetMonsterPassiveAbilities();


        public List<Talent> GetTalents()
        {
            return new List<Talent>()
            {
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.CatLike,
                    Description = "Your hero moves with grace and has almost supernatural balance. Your hero gains +5 DEX.",
                    StatBonus = (BasicStat.Dexterity, 5)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.Fast,
                    Description = "Your hero moves unusually fast and gains a permanent +1 bonus to their Movement stat.",
                    StatBonus = (BasicStat.Move, 1)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.Resilient,
                    Description = "Your hero's brawny physique grants a +5 bonus to the Constitution stat.",
                    StatBonus = (BasicStat.Constitution, 5)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.ResistDisease,
                    Description = "Your hero seems to have a natural ability to resist diseases. Your hero gets a +10 bonus on Constitution Tests to resist disease.",
                    StatBonus = (BasicStat.Constitution, 10)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.ResistPoison,
                    Description = "Your hero seems to have a natural ability to resist poison. Your hero gets a +10 bonus on Constitution Tests to resist poison.",
                    StatBonus = (BasicStat.Constitution, 10)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.Strong,
                    Description = "Your hero's exercises have paid off and your hero gains a +5 bonus to them Strength stat.",
                    StatBonus = (BasicStat.Strength, 5)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.StrongBuild,
                    Description = "Your hero gains a +2 bonus to them Hit Points stat.",
                    StatBonus = (BasicStat.HitPoints, 5)
                },
                new Talent(){
                    Category = TalentCategory.Physical,
                    Name = TalentName.Tank,
                    Description = "Wearing heavy armour has little effect on your hero's ability to move. The hero ignores the Clunky Special Rule.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.Axeman,
                    Description = "Preferring the balance of a good axe, this hero has become a master of using this weapon. He gains +5 CS when using all kinds of axes.",
                    SkillBonus = (Skill.CombatSkill, 5)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.Bruiser,
                    Description = "The hero excels at fighting with blunt weapons and gains +5 CS with all hammers, flails, staffs, and morning stars.",
                    SkillBonus = (Skill.CombatSkill, 5)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.DeathLament,
                    Description = "When others fall, this hero still stands, refusing to give in. Each time your hero is reduced to 0 Hit Points, roll 1d6: on a result of 1-3, the hero regains 1 Hit Point.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.Disarm,
                    Description = "This is a special attack, using the target's DEX as a negative modifier to the attack. If the attack succeeds it inflicts no damage, but causes the enemy to drop their weapon. The enemy must spend their next action trying to pick it up. In order to do so, the enemy will have to succeed with an DEX Test. The enemy will continue until successful. If the hero's attack fails, nothing happens. This can only be used on enemies that are carrying weapons.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.DualWield,
                    Description = "This talent requires a DEX of 60. Any hero with this talent may use a weapon with the Dual Wield Special Rule in its offhand. The attacks are still done as usual with the main weapon, but any hit will add +X DMG to the target. The X is defined in the Weapon Table. Parrying with two weapons is also easier, and any parry while using two weapons has a +5 modifier.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.FastReload,
                    Description = "Years of practice makes your hero faster than most and she can reload in the blink of an eye. She may reload bows and sling in the same action as she shoots once per turn. Crossbows may be reloaded in 1 action and fired in the next. An Arbalest can be reloaded in 2 actions, and fired in the next turn.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.Marksman,
                    Description = "Fighting from afar comes naturally to your hero. The hero gains +5 RS.",
                    SkillBonus = (Skill.RangedSkill, 5)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.MightyBlow,
                    Description = "Your hero is an expert at finding the weak spots of the enemy. Your hero gets a +1 bonus on Damage Rolls with melee weapons.",
                    StatBonus = (BasicStat.DamageBonus, 1)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.ParryMaster,
                    Description = "Your hero is adept at protecting himself with a weapon. If the hero has taken the Parry Stance, he may parry twice with a weapon during one turn.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.PerfectShot,
                    Description = "Identifying the weak spots in enemy armour can sometimes make the difference when firing an arrow or bolt from afar. If the Damage Roll is odd, your hero ignores armour (But not NA).",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.RiposteMaster,
                    Description = "When successfully parrying a strike with them weapon, the hero may automatically cause 2 Points of Damage to that Enemy. May only be done with weapons of class 3 or lower.",
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.Sniper,
                    Description = "With practiced ease, your hero cannot seem to miss when taking careful aim. The aim action gives your hero a +15 modifier instead of +10.",
                    SkillBonus = (Skill.RangedSkill, 5)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.Swordsman,
                    Description = "This hero is very skilled with a blade and gains +5 CS with all types of swords.",
                    SkillBonus = (Skill.CombatSkill, 5)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.TightGrip,
                    Description = "With unusually strong hands, the hero may add +5 STR when calculating what weapon class he or she can use.",
                    StatBonus = (BasicStat.Strength, 5)
                },
                new Talent(){
                    Category = TalentCategory.Combat,
                    Name = TalentName.TunnelFighter,
                    Description = "Your hero is accustomed to fighting in tight spaces. +10 CS when fighting in a corridor.",
                    SkillBonus = (Skill.CombatSkill, 10)
                },
                new Talent(){
                    Category = TalentCategory.Faith,
                    Name = TalentName.Devoted,
                    Description = "The hero gains an extra Energy Point that can only be used for praying.",
                },
                new Talent(){
                    Category = TalentCategory.Faith,
                    Name = TalentName.GodsChosen,
                    Description = "As if by the will of the gods, nothing seems to hurt this priest. +1 Luck.",
                    StatBonus = (BasicStat.Luck, 1)
                },
                new Talent(){
                    Category = TalentCategory.Faith,
                    Name = TalentName.Healer,
                    Description = "This priest has tended many wounds and applies bandages with practiced hands. A bandage applied by this priest will heal +1 HP",
                },
                new Talent(){
                    Category = TalentCategory.Faith,
                    Name = TalentName.Messiah,
                    Description = "With a confidence that radiates through the room, no one can help but be inspired. All heroes within LOS of the priest gain +5 Resolve.",
                    StatBonus = (BasicStat.Resolve, 5)
                },
                new Talent(){
                    Category = TalentCategory.Faith,
                    Name = TalentName.Pure,
                    Description = "The radiance of this priest hurts the eyes of all demons. Any demon trying to attack the priest does so at -10 CS.",
                },
                new Talent(){
                    Category = TalentCategory.Faith,
                    Name = TalentName.Reliquary,
                    Description = "So strong is their faith in the holy relics, that this priest can channel the power of 3 relics, rather than the standard two.",
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.Gatherer,
                    Description = "Finding good ingredients in the wild comes naturally to the hero. +10 Alchemy when searching for ingredients in the wild.",
                    SkillBonus = (Skill.Alchemy, 10)
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.Harvester,
                    Description = "With precise incisions, the hero can harvest good quality components from fallen enemies. +10 Alchemy when harvesting parts.",
                    SkillBonus = (Skill.Alchemy, 10)
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.KeenEye,
                    Description = "The Alchemist has a keen eye when it comes to finding ingredients. The hero may reroll the result when rolling to see what has been gathered. The second result stands.",
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.MasterHealer,
                    Description = "This hero has perfected the art of making Healing Potions. All potions brewed heal +2 Hit Points more than normal.",
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.PerfectToss,
                    Description = "The hero has a knack for lobbying bottles in a perfect arc over friends and foes alike. +10 RS when lobbying a potion over the heads of others.",
                    SkillBonus = (Skill.RangedSkill, 10)
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.Poisoner,
                    Description = "The hero is very adept at making all sorts of poisons. Poisons created by this hero always inflict 1 additional Hit Point per turn.",
                },
                new Talent(){
                    Category = TalentCategory.Alchemist,
                    Name = TalentName.PowerfulPotions,
                    Description = "The strength of these heroes' potions is remarkable. All basic stat (Not M) enhancing potions grants an additional +5 bonus.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.Charming,
                    Description = "This hero seems to get along with everyone and always draws a smile from those to whom he talks. Well aware of the party lets this hero negotiate all rewards and gains +5% Reward Bonus on all quests.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.Disciplined,
                    Description = "Thanks to a military background, this hero has an increased degree of calmness under pressure. This also spreads to the rest of the party. The hero gains +10 RES and the other members of the party gain +5 RES as long as the hero is not knocked out. The effect on the party is not cumulative if other heroes have the same talent. Furthermore, a hero with this talent will not benefit from the effect of this talent from another hero.",
                    StatBonus = (BasicStat.Resolve, 10)
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.Hunter,
                    Description = "The hero has a knack for finding wild game and knows how best to hunt them. The hero gains +10 to Foraging.",
                    SkillBonus = (Skill.Foraging, 10)
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.Lucky,
                    Description = "Some are just luckier than others. Everything seems to go your way. You gain +1 Luck Point.",
                    StatBonus = (BasicStat.Luck, 1)
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.MasterCook,
                    Description = "During a rest, the party members will regain +2 extra HP if they have rations, due to your hero's expert cooking skills. This is not cumulative if more than one hero has this Talent.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.NaturalLeader,
                    Description = "The hero's natural ability to lead will add +2 to the Party Moral permanently. This is not cumulative if more than one hero has this talent.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.RingBearer,
                    Description = "Somehow, this hero has managed to tame the effect of magic imbued items. Instead of being limited to one ring, your hero can now use two rings simultaneously.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.Survivalist,
                    Description = "This talent lets your hero forage one ration from any monster in the Beast category (in a dungeon or after a skirmish), as long as the Forage roll is successful.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.SwiftLeader,
                    Description = "The party may always add one initiative token to the bag. This is only used to increase chance of activation and all heroes may still only act once per turn. Only one hero per party can have this talent.",
                },
                new Talent(){
                    Category = TalentCategory.Common,
                    Name = TalentName.Veteran,
                    Description = "You have your gear in perfect order, making changes in equipment very easy. You can use equipment from a Quick Slot without spending an Action Point (once per turn).",
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.BloodMagic,
                    Description = "The wizard can spend their own life blood to create Mana. For every 2 HP spent, the wizard gains 5 Mana. This transformation can be done for free during the wizard's turn.",
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Conjurer,
                    Description = "The wizard is an expert conjurer and gains +5 Arcane Arts whenever casting a Conjuration Spell. Furthermore, the Mana cost for such a spell is reduced with 5.",
                    SkillBonus = (Skill.ArcaneArts, 5)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Divinator,
                    Description = "The wizard gets +5 Arcane Arts whenever casting a Divination Spell. Furthermore, the Mana cost for such a spell is reduced with 5.",
                    SkillBonus = (Skill.ArcaneArts, 5)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.FastReflexes,
                    Description = "With lightning-fast reflexes, your hero can reach out and touch your enemies when casting spells. Your hero gains a +15 Combat Skill Bonus when casting Touch Spells.",
                    SkillBonus = (Skill.CombatSkill, 15)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Focused,
                    Description = "Well attuned to the void, your hero is adept at tapping into it to gain maximum power. Your hero gets +15 Arcane Arts when focusing.",
                    SkillBonus = (Skill.ArcaneArts, 15)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Restorer,
                    Description = "Restoration spells are the favorite spells of your hero, and this results in all Healing Spells healing +2 Hit Points in addition to the spell's normal result. You cannot have this talent at the same time as you have the Necromancer Talent.",
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Mystic,
                    Description = "The wizard is truly skilled with Mysticism Spells and gets +5 Arcane Arts whenever casting a Mysticism Spell. Furthermore, the Mana cost for such a spell is reduced with 5.",
                    SkillBonus = (Skill.ArcaneArts, 5)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Necromancer,
                    Description = "The hero gets +5 Arcane Arts whenever casting a Necromantic Spell. Furthermore, the Mana cost for such a spell is reduced with 5. You cannot have this Talent at the same time as you have the Restorer Talent.",
                    SkillBonus = (Skill.ArcaneArts, 5)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.PowerfulMissiles,
                    Description = "Your hero has perfected the use of Magic Missiles, knowing where to aim for maximum effect. Magic Missile Spells do +1 Damage.",
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Summoner,
                    Description = "Reaching into other realms and bringing other beings to their aid has become easier with years of practice. Your hero gets +5 Arcane Arts on all Summoning Spells.",
                    SkillBonus = (Skill.ArcaneArts, 5)
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Sustainer,
                    Description = "Upkeep for the wizard's spells is reduced by 1.",
                },
                new Talent(){
                    Category = TalentCategory.Magic,
                    Name = TalentName.Thrifty,
                    Description = "The wizard requires 2 Mana less on every spell cast.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.Assassin,
                    Description = "With uncanny precision, the hero will automatically hit any target from behind with a class 1 or 2 weapon.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.Backstabber,
                    Description = "Accustomed to optimizing the odds, your hero ignores enemy armour and NA when attacking from behind.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.Cutpurse,
                    Description = "Once per visit in a settlement, your hero may try to steal the purse from some unsuspecting victim. This must be done as the first thing when entering a settlement. Roll 1d6. On a result of 1-2 the hero gains 1d100 coins. On a result of 6, the attempt is detected, and the hero is immediately chased out of the settlement. The hero may do nothing until the rest of the party decides to leave the settlement. Rations must be used as normal, and if rations are lacking, the hero becomes hungry. Foraging is allowed while waiting.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.Evaluate,
                    Description = "Your hero has a good sense for the value of things. A successful Barter Roll will give your hero +15% instead of the usual +10%.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.LockPicker,
                    Description = "No lock seems to hinder this hero from beating them. +5 Pick Locks skill.",
                    SkillBonus = (Skill.PickLocks, 5)
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.MechanicalGenius,
                    Description = "Your hero is a master at understanding mechanical contraptions and gain +10 when disarming traps.",
                    SkillBonus = (Skill.Perception, 10)
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.Nimble,
                    Description = "The hero may dodge twice per battle instead of only once.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.QuickFingers,
                    Description = "Accustomed to working under pressure, your hero has mastered the skill of reading a lock and picking it. Picking a lock now takes 1AP instead of 2.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.SharpEyed,
                    Description = "Your hero has an extreme sense for details and can easily notice anything out of the ordinary. Your hero gains a +10 bonus on Perception Tests.",
                    SkillBonus = (Skill.Perception, 10)
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.SenseForGold,
                    Description = "It seems this hero can almost smell their way to treasures. When rolling on the Furniture Table for treasures, the hero may subtract -1 to the roll.",
                },
                new Talent(){
                    Category = TalentCategory.Sneaky,
                    Name = TalentName.TrapFinder,
                    Description = "Your hero is an expert at dealing with traps. Your hero gains a +10 PER bonus when avoiding traps. This is cumulative with Sharp-eyed.",
                },
                new Talent(){
                    Category = TalentCategory.Mental,
                    Name = TalentName.Braveheart,
                    Description = "Your hero is braver than most. +10 bonus on Fear and Terror Tests.",
                    StatBonus = (BasicStat.Resolve, 10)
                },
                new Talent(){
                    Category = TalentCategory.Mental,
                    Name = TalentName.Confident,
                    Description = "No enemy or task is too difficult. Your hero gains a +5 bonus to the Resolve stat.",
                    StatBonus = (BasicStat.Resolve, 5)
                },
                new Talent(){
                    Category = TalentCategory.Mental,
                    Name = TalentName.Fearless,
                    Description = "Your hero is completely immune to the effects of fear and treats terror as fear. This talent requires that the hero already has the Braveheart Mental Talent.",
                },
                new Talent()
                {
                    Category = TalentCategory.Mental,
                    Name = TalentName.Hate,
                    Description = "This hate fuels their fighting, granting a +5 bonus to CS when attacking these enemies. However, so blind is their hatred that their focus on parrying and dodging diminishes (-5 penalty) when struck by them.",
                    SkillBonus = (Skill.CombatSkill, 5),
                },
                new Talent(){
                    Category = TalentCategory.Mental,
                    Name = TalentName.StrongMinded,
                    Description = "Your hero is less affected by the horrors he faces in the dungeons than their comrades. He gains +1 Sanity Point.",
                    StatBonus = (BasicStat.Sanity, 1)
                },
                new Talent(){
                    Category = TalentCategory.Mental,
                    Name = TalentName.Wise,
                    Description = "Your hero gains a permanent +5 bonus to the Wisdom stat.",
                    StatBonus = (BasicStat.Wisdom, 1)
                }
            };
        }

        public Talent GetRandomTalent()
        {
            return GetRandomTalentByCategory();
        }

        public Talent GetRandomTalentByCategory(TalentCategory? category = null)
        {
            switch (category)
            {
                case TalentCategory.Physical:
                    return PhysicalTalents[RandomHelper.GetRandomNumber(0, PhysicalTalents.Count - 1)];
                case TalentCategory.Combat:
                    return CombatTalents[RandomHelper.GetRandomNumber(0, CombatTalents.Count - 1)];
                case TalentCategory.Faith:
                    return FaithTalents[RandomHelper.GetRandomNumber(0, FaithTalents.Count - 1)];
                case TalentCategory.Alchemist:
                    return AlchemistTalents[RandomHelper.GetRandomNumber(0, AlchemistTalents.Count - 1)];
                case TalentCategory.Common:
                    return CommonTalents[RandomHelper.GetRandomNumber(0, CommonTalents.Count - 1)];
                case TalentCategory.Magic:
                    return MagicTalents[RandomHelper.GetRandomNumber(0, MagicTalents.Count - 1)];
                case TalentCategory.Sneaky:
                    return SneakyTalents[RandomHelper.GetRandomNumber(0, SneakyTalents.Count - 1)];
                case TalentCategory.Mental:
                    return MentalTalents[RandomHelper.GetRandomNumber(0, MentalTalents.Count - 1)];
                default:
                    return Talents[RandomHelper.GetRandomNumber(0, Talents.Count - 1)];
            }
        }

        public List<Talent> GetTalentsByCategory(TalentCategory category)
        {
            return Talents.Where(t => t.Category == category).ToList();
        }

        public Talent GetTalentByName(TalentName? name)
        {
            return Talents.FirstOrDefault(t => t.Name == name) ?? new Talent();
        }


        public Talent GetHateTalentByCategory(HateCategory category)
        {
            string baseDescription = "This hate fuels their fighting, granting a +5 bonus to CS when attacking these enemies. However, so blind is their hatred that their focus on parrying and dodging diminishes (-5 penalty) when struck by them.";

            // The base talent object with common properties
            var talent = new Talent()
            {
                Category = TalentCategory.Mental,
            };

            // Use a switch to set the specific name and description based on the category
            switch (category)
            {
                case HateCategory.Bandits:
                    talent.Name = TalentName.HateBandits;
                    talent.Description = $"This talent applies to any enemy with 'Bandit' in its name. {baseDescription}";
                    break;
                case HateCategory.Bats:
                    talent.Name = TalentName.HateBats;
                    talent.Description = $"This talent applies to any enemy with 'Bat' in its name. {baseDescription}";
                    break;
                case HateCategory.Beastmen:
                    talent.Name = TalentName.HateBeastmen;
                    talent.Description = $"This talent applies to any enemy with 'Beastman' in its name. {baseDescription}";
                    break;
                case HateCategory.Centipedes:
                    talent.Name = TalentName.HateCentipedes;
                    talent.Description = $"This talent applies to any enemy with 'Centipede' in its name. {baseDescription}";
                    break;
                case HateCategory.DarkElves:
                    talent.Name = TalentName.HateDarkElves;
                    talent.Description = $"This talent applies to any enemy with 'Dark Elf' in its name. {baseDescription}";
                    break;
                case HateCategory.Demons:
                    talent.Name = TalentName.HateDemons;
                    talent.Description = $"This talent applies to any enemy with 'Demon' in its name. {baseDescription}";
                    break;
                case HateCategory.Dragons:
                    talent.Name = TalentName.HateDragons;
                    talent.Description = $"This talent applies to any enemy with 'Dragon' in its name. {baseDescription}";
                    break;
                case HateCategory.Elementals:
                    talent.Name = TalentName.HateElementals;
                    talent.Description = $"This talent applies to any enemy with 'Elemental' in its name. {baseDescription}";
                    break;
                case HateCategory.Froglings:
                    talent.Name = TalentName.HateFroglings;
                    talent.Description = $"This talent applies to any enemy with 'Frogling' in its name. {baseDescription}";
                    break;
                case HateCategory.Geckos:
                    talent.Name = TalentName.HateGeckos;
                    talent.Description = $"This talent applies to any enemy with 'Gecko' in its name. {baseDescription}";
                    break;
                case HateCategory.Ghosts:
                    talent.Name = TalentName.HateGhosts;
                    talent.Description = $"This talent applies to ethereal undead like Ghosts, Banshees, and Wraiths. {baseDescription}";
                    break;
                case HateCategory.Ghouls:
                    talent.Name = TalentName.HateGhouls;
                    talent.Description = $"This talent applies to any enemy with 'Ghoul' in its name. {baseDescription}";
                    break;
                case HateCategory.Giants:
                    talent.Name = TalentName.HateGiants;
                    talent.Description = $"This talent applies to any enemy with 'Giant' in its name. {baseDescription}";
                    break;
                case HateCategory.Gnolls:
                    talent.Name = TalentName.HateGnolls;
                    talent.Description = $"This talent applies to any enemy with 'Gnoll' in its name. {baseDescription}";
                    break;
                case HateCategory.Goblins:
                    talent.Name = TalentName.HateGoblins;
                    talent.Description = $"This talent applies to any enemy with 'Goblin' in its name. {baseDescription}";
                    break;
                case HateCategory.Golems:
                    talent.Name = TalentName.HateGolems;
                    talent.Description = $"This talent applies to any enemy with 'Golem' in its name. {baseDescription}";
                    break;
                case HateCategory.Minotaurs:
                    talent.Name = TalentName.HateMinotaurs;
                    talent.Description = $"This talent applies to any enemy with 'Minotaur' in its name. {baseDescription}";
                    break;
                case HateCategory.Mummies:
                    talent.Name = TalentName.HateMummies;
                    talent.Description = $"This talent applies to any enemy with 'Mummy' in its name. {baseDescription}";
                    break;
                case HateCategory.Ogres:
                    talent.Name = TalentName.HateOgres;
                    talent.Description = $"This talent applies to any enemy with 'Ogre' in its name. {baseDescription}";
                    break;
                case HateCategory.Orcs:
                    talent.Name = TalentName.HateOrcs;
                    talent.Description = $"This talent applies to any enemy with 'Orc' in its name. {baseDescription}";
                    break;
                case HateCategory.Rats:
                    talent.Name = TalentName.HateRats;
                    talent.Description = $"This talent applies to any enemy with 'Rat' in its name. {baseDescription}";
                    break;
                case HateCategory.Saurians:
                    talent.Name = TalentName.HateSaurians;
                    talent.Description = $"This talent applies to any enemy with 'Saurian' in its name. {baseDescription}";
                    break;
                case HateCategory.Scorpions:
                    talent.Name = TalentName.HateScorpions;
                    talent.Description = $"This talent applies to any enemy with 'Scorpion' in its name. {baseDescription}";
                    break;
                case HateCategory.Skeletons:
                    talent.Name = TalentName.HateSkeletons;
                    talent.Description = $"This talent applies to any enemy with 'Skeleton' in its name. {baseDescription}";
                    break;
                case HateCategory.Snakes:
                    talent.Name = TalentName.HateSnakes;
                    talent.Description = $"This talent applies to any enemy with 'Snake' in its name. {baseDescription}";
                    break;
                case HateCategory.Spiders:
                    talent.Name = TalentName.HateSpiders;
                    talent.Description = $"This talent applies to any enemy with 'Spider' in its name. {baseDescription}";
                    break;
                case HateCategory.Toads:
                    talent.Name = TalentName.HateToads;
                    talent.Description = $"This talent applies to any enemy with 'Toad' in its name. {baseDescription}";
                    break;
                case HateCategory.Trolls:
                    talent.Name = TalentName.HateTrolls;
                    talent.Description = $"This talent applies to any enemy with 'Troll' in its name. {baseDescription}";
                    break;
                case HateCategory.Vampires:
                    talent.Name = TalentName.HateVampires;
                    talent.Description = $"This talent applies to any enemy with 'Vampire' in its name. {baseDescription}";
                    break;
                case HateCategory.Werewolves:
                    talent.Name = TalentName.HateWerewolves;
                    talent.Description = $"This talent applies to any enemy with 'Werewolf' in its name. {baseDescription}";
                    break;
                case HateCategory.Wights:
                    talent.Name = TalentName.HateWights;
                    talent.Description = $"This talent applies to any enemy with 'Wight' in its name. {baseDescription}";
                    break;
                case HateCategory.Wolves:
                    talent.Name = TalentName.HateWolves;
                    talent.Description = $"This talent applies to any enemy with 'Wolf' in its name. {baseDescription}";
                    break;
                case HateCategory.Zombies:
                    talent.Name = TalentName.HateZombies;
                    talent.Description = $"This talent applies to any enemy with 'Zombie' in its name. {baseDescription}";
                    break;
            }

            return talent;
        }

        public List<MonsterPassiveSpecial> GetMonsterPassiveAbilities()
        {
            return new List<MonsterPassiveSpecial>
            {
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.CauseFear,
                    Description = "This enemy imparts Fear to all heroes of level X and lower. All heroes must take a RES test as soon as it is placed. A failure will give a -10 CS/RS to all attacks against it. Magic cast against the enemy will also suffer -10 Arcane Arts. Test once per battle and creature type."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.CauseTerror,
                    Description = "This enemy inflicts Terror on all heroes of level X and lower. Heroes with a higher level than X experience this as fear. A terror test is made at RES-20 and a failure will give same effect as fear, but the hero will also be stunned for 1 AP. Test as soon as it is placed on the table."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Corrosive,
                    Description = "The enemy has a corrosive effect on all metals except for Mithril and silver. Any metal armour struck by an enemy with this rule automatically loses 1 point of durability, even if the damage is less than the armour's defence value. Any metal weapon striking a creature with this rule and causing an odd amount of damage loses 1 point of durability per strike that hits."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.CursedWeapons,
                    Description = "Cursed Weapons are tainted by foul magic, or they may have some part of the user's evil soul infused into it. A wound caused by a cursed weapon also removes 1 point of Sanity from the target."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Demon,
                    Description = "Demons are not of this world, and magic can send these creatures back to their own realm. Any magic damage of 10 forces the demon to make a RES test. A failure will send the demon back to its own realm. Remove the model and count it as killed. It cannot be looted, however."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.DiseaseRidden,
                    Description = "These creatures are so full of disease that just standing next to them is dangerous. At the start of every turn, a hero standing adjacent to one of these creatures must pass a CON+2O test to resist disease."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Ethereal,
                    Description = "Indicates that the enemy is immune to damage from normal weapons. You will need magic weapons, holy water or magic to inflict the slightest harm on this creature. Ethereal creatures will never be locked in a ZOC and can move through squares with heroes. Heroes must still follow the normal rules for ZOC."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.ExtraDamageFromFire,
                    Description = "These creatures are susceptible to damage caused by fire and take 1d6 extra hit points from any wound caused by fire."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.ExtraDamageFromWater,
                    Description = "These creatures are susceptible to damage caused by water and take 1d6 extra hit points from any wound caused by water."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.ExtraDamageFromSilver,
                    Description = "These creatures are susceptible to damage caused by silver and take 1d6 extra hit points from any wound caused by silver."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.FearElves,
                    Description = "These enemies fear Elves and suffer the standard modifier for fear if they fail their Resolve test."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.FerociousCharge,
                    Description = "This creature charges with such power that it causes a further 1d4 point of damage during the charge attack."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.FireDamage,
                    Description = "This creature causes fire damage."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.FrostDamage,
                    Description = "This creature causes frost damage."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Floater,
                    Description = "This creature hovers above the ground; therefore, it is able to avoid any pitfalls or traps. They will never spring any traps and they can move over pits as if they were solid ground. They may even end their movement in such a square."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Flyer,
                    Description = "These enemies can fly and can move through other models as they do not stop in a square containing a model. They also do not incur penalties for moving through a hero's ZOC. They will never spring any traps and they can move over pits as if they were solid ground. They may even end their movement in such a square."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.FlyerOutdoors,
                    Description = "Some flyers are so large that they can only fly outdoors in skirmish battles."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Frenzy,
                    Description = "This enemy is always in frenzy mode and gains the benefit of an extra strike when they cause damage."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.GhostlyTouch,
                    Description = "These ethereal creatures do not use normal weapons but instead reach into the very soul of their target. These attacks cannot be parried, but they may be dodged. Armour and NA does nothing to help; however, the target may try to avoid damage by passing a Resolve test. If this fails, the target takes 1d8 damage and also loses 1 sanity point."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Gust,
                    Description = "A creature with this special ability will create a powerful gust around them making it hard to use ranged weapons. All creatures in the room or corridor where this creature is located suffer a -15 penalty to their RS.",

                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.HardAsRock,
                    Description = "Stone Golems are magic beings created from ensorcelled stone and are extremely difficult to damage. They are completely immune to all ranged weapons. Bladed weapons only do half damage (RDD) unless they are magic or made of Mithril."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.HateHeroes,
                    Description = "In much the same way as heroes may hate their enemies, the enemy may also hate your heroes. +5 CS, but luckily for them, it does not come with the negative for dodging.",

                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.JustBones,
                    Description = "Skeletons and Wights do not have much other than bones to show the world, and that makes some weapons more or less effective against them. Arrows, bolts and sling stones suffer a -2 DAM penalty, and crushing weapons (such as Hammers, Flails, Staffs, Morning Stars, and the like) gain +2 damage bonus."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Large,
                    Description = "This creature is large; therefore, it is easier to hit with ranged weapons. Their size also allows them to deal great damage. Thus, they roll damage twice and select the best roll. These monsters take up 4 squares instead of one. A large creature can only shove if there is enough room for it to fit in the space occupied by the hero. If there is another character standing adjacent to the shoved hero that blocks the way, this character will also be pushed back automatically. A large creature cannot pass single file squares except for bridges."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.XLarge,
                    Description = "An X-large creature uses 2x3 squares. It follows the same rules as Large creatures."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Leech,
                    Description = "A successful attack from this creature will enable it to stick to its target. The target will be unable to move, or to attack anything other than the leech. The leech will automatically drain 1d4 points of hit points from the target at the start of its turn. The hero must also roll for disease for every turn that the leech is latched on to its target. If more than one leech has latched on to a hero, the hero must roll once for each leech. Attacking a leech that is latched onto a hero is done just as normal, but any other hero trying to attack it must be extra careful not to hit his companion and thus only ever deals half damage (RDD). While attached, the leech has the Regeneration (1d3) rule."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.MagicBeing,
                    Description = "These creatures are the creations of pure magic. As such, once they are destroyed, they leave nothing behind to loot."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.MagicUser,
                    Description = "This enemy can cast spells. The exact spells are mentioned in the Encounter Tables. Enemies use RS to determine if the spell succeeds."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.MultipleAttacks,
                    Description = "This creature can assail its targets with multiple attacks. Whenever the creature makes a standard attack, it will instead strike X number of times. Roll to hit and DAM separately. This attack still only counts as 1 AP."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.MultipleAttacksHydra,
                    Description = "Hydra can attack multiple targets with its different heads, and the long necks give it a good reach as well. The Hydra has 5 heads, and each head can attack at a separate target within 4 squares of the body, just as if it was 5 different creatures."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.PerfectHearing,
                    Description = "This enemy is acutely aware of its surroundings. When rolling for initiative, add +15 to this enemy's DEX.",

                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Poisonous,
                    Description = "This enemy is poisonous or venomous. If wounded, the hero must pass a CON test or suffer the consequences of the poison."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Psychic,
                    Description = "A creature with psychic ability which automatically decreases the RES of all heroes by -20 as soon as it is placed on the table. The effect is only cancelled when the creature dies. The effect is not cumulative if two creatures have the same effect.",

                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Regeneration,
                    Description = "This enemy regenerates hit points at the start of every turn."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Rend,
                    Description = "If the creature manages to seize its target in its jaws, it will violently shake its head causing further damage. If the hero fails STR, roll another 1d6 of DAM."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.RiddleMaster,
                    Description = "Whenever you encounter a Sphinx (or multiple) without any other enemies nearby, you will have the option to answer one riddle. This requires a WIS test from one chosen hero. A successful answer will remove the Sphinx from the table without a fight. 150 XP granted. A failure will anger the Sphinx and lead to battle."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Scurry,
                    Description = "Rats are too fast to be locked in a ZOC and may move through a square with heroes. Heroes must still pass a DEX test to move in the rats' ZOC."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Silent,
                    Description = "These enemies are very difficult to hear. Perfect hearing will not help when calculating surprise."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.SimpleWeapons,
                    Description = "These creatures use crude weapons instead of the standard weapons that are usually encountered. This may include logs, or even large stones, but for game terms, all Simple Weapons are treated as Warhammers, and they are granted the same attributes."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Slow,
                    Description = "This enemy moves so slowly that they may only make one move per turn. It still has 2 AP though, and if it cannot use its second AP to any suitable action, it will be forfeit."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Sneaky,
                    Description = "These creatures are experts at ambushing enemies and receive an extra 15 bonus points when rolling for initiative."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Stench,
                    Description = "So foul is the stench of this creature that it quickly becomes difficult to focus. All close combat attacks targeting this creature suffer a -10 CS penalty.",

                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.Stupid,
                    Description = "Any creature affected by stupidity must roll 1d6 at the start of their turn. On a roll of 1, they will do nothing that turn, except look around them in confusion."
                },
                new MonsterPassiveSpecial
                {
                    Name = MonsterSpecialName.WallCrawler,
                    Description = "Spiders can move on walls to bypass heroes. They cannot end their turn in such a position (even though that would be cool, it is not very practical in a game like this). Spiders ignore ZOC when moving on walls."
                }
            };
        }

        public MonsterPassiveSpecial GetMonsterPassiveSpecialByName(MonsterSpecialName name)
        {
            return MonsterPassiveSpecials.FirstOrDefault(m => m.Name == name) ?? new MonsterPassiveSpecial();
        }
    }

    public enum HateCategory
    {
        Bandits,
        Bats,
        Beastmen,
        Centipedes,
        DarkElves,
        Demons,
        Dragons,
        Elementals,
        Froglings,
        Geckos,
        Ghosts,
        Ghouls,
        Giants,
        Gnolls,
        Goblins,
        Golems,
        Minotaurs,
        Mummies,
        Ogres,
        Orcs,
        Rats,
        Saurians,
        Scorpions,
        Skeletons,
        Snakes,
        Spiders,
        Toads,
        Trolls,
        Vampires,
        Werewolves,
        Wights,
        Wolves,
        Zombies
    }

    public enum TalentCategory
    {
        Magic,
        Physical,
        Combat,
        Faith,
        Alchemist,
        Common,
        Sneaky,
        Mental
    }

    public enum TalentName
    {
        // Physical Talents
        CatLike,
        Fast,
        NightVision,
        PerfectHearing,
        Resilient,
        ResistDisease,
        ResistPoison,
        Strong,
        StrongBuild,
        Tank,
        // Combat Talents
        Axeman,
        Bruiser,
        DeathLament,
        Disarm,
        DualWield,
        FastReload,
        Marksman,
        MightyBlow,
        ParryMaster,
        PerfectShot,
        RiposteMaster,
        Sniper,
        Swordsman,
        TightGrip,
        TunnelFighter,
        // Faith Talents
        Devoted,
        GodsChosen,
        Healer,
        Messiah,
        Pure,
        Reliquary,
        // Alchemist Talents
        Gatherer,
        Harvester,
        KeenEye,
        MasterHealer,
        PerfectToss,
        Poisoner,
        PowerfulPotions,
        // Common Talents
        Charming,
        Disciplined,
        Hunter,
        Lucky,
        MasterCook,
        NaturalLeader,
        RingBearer,
        Survivalist,
        SwiftLeader,
        Veteran,
        // Magic Talents
        BloodMagic,
        Conjurer,
        Divinator,
        FastReflexes,
        Focused,
        Restorer,
        Mystic,
        Necromancer,
        PowerfulMissiles,
        Summoner,
        Sustainer,
        Thrifty,
        // Sneaky Talents
        Assassin,
        Backstabber,
        Cutpurse,
        Evaluate,
        LockPicker,
        MechanicalGenius,
        Nimble,
        QuickFingers,
        SharpEyed,
        SenseForGold,
        Streetwise,
        TrapFinder,
        // Mental Talents
        Braveheart,
        Confident,
        Fearless,
        Hate,
        StrongMinded,
        Wise,
        // Hate Talents
        HateBandits,
        HateBats,
        HateBeastmen,
        HateCentipedes,
        HateDarkElves,
        HateDemons,
        HateDragons,
        HateElementals,
        HateFroglings,
        HateGeckos,
        HateGhosts,
        HateGhouls,
        HateGiants,
        HateGnolls,
        HateGoblins,
        HateGolems,
        HateMinotaurs,
        HateMummies,
        HateOgres,
        HateOrcs,
        HateRats,
        HateSaurians,
        HateScorpions,
        HateSkeletons,
        HateSnakes,
        HateSpiders,
        HateToads,
        HateTrolls,
        HateVampires,
        HateWerewolves,
        HateWights,
        HateWolves,
        HateZombies,
        // Background Talents
        BadTempered,
        Poverty,
        TheFraud,
        TheNoble,
        TheApprentice,
        Weak,
        Arachnophobia,
        Claustrophobia,
        AfraidOfHeights
    }

    public class Talent : PassiveAbility
    {
        public TalentName Name { get; set; }
        public TalentCategory Category { get; set; }
        public (BasicStat, int)? StatBonus { get; set; }
        public (Skill, int)? SkillBonus { get; set; }


        public override string ToString()
        {
            return $"{Regex.Replace(Name.ToString(), "(\\B[A-Z])", " $1")}: {Description}";
        }
    }

    public enum MonsterSpecialName
    {
        CauseFear,
        CauseTerror,
        Corrosive,
        CursedWeapons,
        Demon,
        Diseased,
        DiseaseRidden,
        Ethereal,
        ExtraDamageFromFire,
        ExtraDamageFromWater,
        ExtraDamageFromSilver,
        FearElves,
        FerociousCharge,
        FireDamage,
        FrostDamage,
        Floater,
        Flyer,
        FlyerOutdoors,
        Frenzy,
        GhostlyTouch,
        Gust,
        HardAsRock,
        HateHeroes,
        HateDwarves,
        JustBones,
        Large,
        XLarge,
        Leech,
        MagicBeing,
        MagicUser,
        MultipleAttacks,
        MultipleAttacksHydra,
        PerfectHearing,
        Poisonous,
        Psychic,
        Regeneration,
        Rend,
        RiddleMaster,
        Scurry,
        Silent,
        SimpleWeapons,
        Slow,
        Sneaky,
        Stench,
        Stupid,
        WallCrawler,
        WeakToFrost,
        WeakToFire,
        WeakToSilver
    }

    public class MonsterPassiveSpecial : PassiveAbility
    {
        public MonsterSpecialName Name { get; set; }

        public override string ToString()
        {
            return $"{Regex.Replace(Name.ToString(), "(\\B[A-Z])", " $1")}: {Description}";
        }

    }
}
