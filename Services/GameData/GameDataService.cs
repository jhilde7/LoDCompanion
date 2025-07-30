using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Models.Combat;
using LoDCompanion.Models;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Utilities;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace LoDCompanion.Services.GameData
{
    public class GameDataService
    {
        public List<Prayer> Prayers => GetPrayers();
        public List<Species> Species => GetSpecies();
        public List<Profession> Professions => GetProfessions();
        public List<Perk> Perks => GetPerks();
        public List<Perk> LeaderPerks => GetPerksByCategory(PerkCategory.Leader);
        public List<Perk> CommonPerks => GetPerksByCategory(PerkCategory.Common);
        public List<Perk> CombatPerks => GetPerksByCategory(PerkCategory.Combat);
        public List<Perk> SneakyPerks => GetPerksByCategory(PerkCategory.Sneaky);
        public List<Perk> FaithPerks => GetPerksByCategory(PerkCategory.Faith);
        public List<Perk> ArcanePerks => GetPerksByCategory(PerkCategory.Arcane);
        public List<Perk> AlchemistPerks => GetPerksByCategory(PerkCategory.Alchemist);


        public List<Prayer> GetPrayers()
        {
            return new List<Prayer>()
                {
                new Prayer(){
                  Name = "Bringer of Light",
                  Level = 1,
                  PrayerEffect = "The light of the gods shines through the priest, causing the Undead to waver. Any Undead trying to attack the Warrior Priest suffers -10 CS.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "The Power of Iphy",
                  Level = 1,
                  PrayerEffect = "This empowering psalm strengthens your resolve. The party gets +10 RES on any Fear or Terror Test during the battle. If they already have failed these tests, they may retake them with this bonus.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Charas, Walk with Us",
                  Level = 1,
                  PrayerEffect = "This prayer goes to Charus and as long as he listens, all heroes regain an Energy Point on any skill roll of 01-10, instead of the normal 01-05. Note that this only affects energy, not the other options you have if you roll 01-05. However, the priest will be too busy with the prayer to benefit from this.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Metheia's Ward",
                  Level = 1,
                  PrayerEffect = "Under the protection of Metheia, the priest regains 1 lost HP at the start of their activation, for the rest of the battle.",
                  Duration = "For the rest of the battle."
                },
                new Prayer(){
                  Name = "Power of the Gods",
                  Level = 1,
                  PrayerEffect = "By channelling the power of the gods and diverting it to a wizard, the priest can help conjure a spell. As long as the prayer is active, any hero wizard gains +10 Arcane Arts.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Litany of Metheia",
                  Level = 2,
                  PrayerEffect = "Metheia watches over the heroes and grants them them power of life. Every hero that passes a RES test at the start of their activation regains 1 HP.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Power of Faith",
                  Level = 2,
                  PrayerEffect = "The gods grant your party inner strength, making them immune to fear, and treating terror as fear. Heroes already scared will regain their courage if this prayer succeeds.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Smite The Heretics!",
                  Level = 2,
                  PrayerEffect = "The wrath of the gods renders the flesh of your enemies. At the start of each turn, the enemies within 4 squares of the priest must pass a RES test or lose 1 HP.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Verse of The Sane",
                  Level = 2,
                  PrayerEffect = "As long as this verse is read, the heroes are less prone to mental scars. Each event that would trigger a loss of a Sanity Point is negated by a RES test. If the test succeeds, the Sanity Point is not lost.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Shield of the Gods",
                  Level = 2,
                  PrayerEffect = "The gods will protect the pious, and as long as this prayer is active, any wizard will be protected from miscast. Any Miscast roll can be ignored, although the priest will instead have to pass a RES test, or suffer 1d4 DMG with no armour and NA.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Strengths of Ohlnir",
                  Level = 3,
                  PrayerEffect = "The party feels invigorated, and their weapons feel like feathers in their hands. All members of the party gain +10 Strength.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Warriors of Ramos",
                  Level = 3,
                  PrayerEffect = "As if the gods guide the weapons of the heroes, all seem to fight with renewed power. All members of the party fight with +5 CS.",
                  Duration = "Until end of next battle, or 4 turns if used between battles."
                },
                new Prayer(){
                  Name = "Stay Thy Hand!",
                  Level = 3,
                  PrayerEffect = "The enemies seem to slow down, as if questioning whether to fight or not. All enemies within 4 squares of the priest must pass a Resolve Test and will lose 1 Action Point during that turn. Test at the start of every turn. This effect is not cumulative with any other effect causing an enemy to lose an action. For instance, a wounded enemy will not be affected by this prayer.",
                  Duration = "Until enemy test succeeds (tested at start of every turn)."
                },/* this prayer is printed like this on the card, which is the same as "Stay Thy Hand!"
                new Prayer(){
                  Name = "Be Gone!",
                  Level = 3,
                  PrayerEffect = "The enemies seem to slow down, as if questioning whether to fight or not. All enemies within 4 squares of the priest must pass a Resolve Test and will lose 1 Action Point during that turn. Test at the start of every turn. This effect is not cumulative with any other effect causing an enemy to lose an action. For instance, a wounded enemy will not be affected by this prayer.",
                  Duration = "Until enemy test succeeds (tested at start of every turn)."
                },*/
                new Prayer(){
                  Name = "Providence of Metheia",
                  Level = 3,
                  PrayerEffect = "Metheia will shield its children and protect them from harm. While this prayer is active, all heroes get +10 CON when rolling to resist disease or poison.",
                  Duration = "While prayer is active (until end of next battle, or 4 turns if used between battles)."
                },
                new Prayer(){
                  Name = "We Shall Not Falter",
                  Level = 4,
                  PrayerEffect = "The power of the gods strengthens the party, making them more resilient than ever. All members of the party gain +5 HP that can temporarily give a hero more HP than its current max. After the battle, this goes back to the normal max HP.",
                  Duration = "Until end of battle."
                },
                new Prayer(){
                  Name = "God's Champion",
                  Level = 4,
                  PrayerEffect = "The priest fights like a dervish, imbued by the power of their or them god. Combat Skill is increased by +15 but after the battle, the priest loses an additional Point of Energy. If there are not enough points, the Constitution of the priest is halved (RDD) until the next short rest or until the heroes leave the dungeon.",
                  Duration = "Until end of battle (effect on energy/constitution is after battle)."
                }
            };
        }

        internal List<Prayer> GetPrayersByLevel(int level)
        {
            List<Prayer> list = new List<Prayer>();
            foreach (Prayer prayer in Prayers)
            {
                if (prayer.Level == level)
                {
                    list.Add(prayer);
                }
            }
            return list;
        }

        public List<Perk> GetPerks()
        {
            return new List<Perk>()
            {
                new Perk(){
                    Category = PerkCategory.Leader,
                    Name = PerkName.CallToAction,
                    Effect = "Your hero lets out a battle shout that gives another hero a chance to spring into immediate action. You may use this Perk when activating the hero. Once the 2 AP has been spent, you may take a hero token from the bag and activate another hero within LOS of the hero using this Perk.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Leader,
                    Name = PerkName.Encouragement,
                    Effect = "Your hero's encouragement strengthens the hearts of their comrades, giving them +10 on an upcoming Fear or Terror Test.",
                    Comment = "May be used outside of the ordinary acting order whenever a fear or test is necessary."
                },
                new Perk(){
                    Category = PerkCategory.Leader,
                    Name = PerkName.KeepCalmAndCarryOn,
                    Effect = "Your hero's Resolve keeps the party together. You may increase Party Morale with +2.",
                    Comment = "This may not increase morale above starting morale."
                },
                new Perk(){
                    Category = PerkCategory.Leader,
                    Name = PerkName.Rally,
                    Effect = "The hero tries to encourage a comrade to action! A hero that has failed a Fear or Terror Test may immediately retake that test.",
                    Comment = "May be used outside of the ordinary acting order whenever a fear or test is necessary."
                },
                new Perk(){
                    Category = PerkCategory.Common,
                    Name = PerkName.IgnoreWounds,
                    Effect = "Hero gains Natural Armour 2",
                    Comment = "Lasts for one battle."
                },
                new Perk(){
                    Category = PerkCategory.Common,
                    Name = PerkName.SixthSense,
                    Effect = "May add +20 to a dodge result when attacked or +20 to a Perception Test when avoiding a triggered trap.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Common,
                    Name = PerkName.Sprint,
                    Effect = "Your hero may use one Point of Energy to move up to 6 squares with the first movement. A second movement is still allowed but with the standard half movement.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Common,
                    Name = PerkName.Taunt,
                    Effect = "Your hero knows exactly how to trigger the enemy. Your hero can force an enemy, that is not locked in close combat, to attack them ignoring normal targeting procedure.",
                    Comment = "Chose which enemy to taunt before rolling."
                },
                new Perk(){
                    Category = PerkCategory.Common,
                    Name = PerkName.TasteForBlood,
                    Effect = "Character evokes blood lust on a To Hit' roll of 01-10 instead of 01-05 for an entire battle.",
                    Comment = "Must be used before Damage Roll but lasts the entire battle."
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.BattleFury,
                    Effect = "Using their inner energy, your hero may perform 2 Power Attacks in one turn as if they only cost 1 AP.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.DeadlyStrike,
                    Effect = "Adds +25 CS to your next attack.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.Frenzy,
                    Effect = "Working herself into a frenzy, your hero flails wildly at them enemies. For every attack that damages the enemy, she may attack again. This attack does not have to be at the same target. While frenzied, the hero may only move or attack and may do nothing else, including parrying or dodging.",
                    Comment = "Barbarians only. Takes 1 AP to activate. Lasts for one battle."
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.HuntersEye,
                    Effect = "Your hero may shoot two arrows when performing a Ranged Attack with a bow. Both arrows must target the same enemy. Roll for each attack separately.",
                    Comment = "Can only be activated when using a bow or a sling."
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.PerfectAim,
                    Effect = "Your hero's aim is spot on. Add +25 to RS to your next Ranged Attack.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.PowerfulBlow,
                    Effect = "Your hero attacks with all them strength, causing 1d6 extra damage.",
                    Comment = "Must be decided before the attack is made."
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.ShieldBash,
                    Effect = "Pushes one enemy of the same size or smaller one square. Target must make a Dexterity Test or fall over, spending its next action to stand again. Hero may occupy target's square afterwards.",
                    Comment = "Flyers are immune. Requires a Heater or Tower shield."
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.ShieldWall,
                    Effect = "Years of training lets your hero handle that shield like a pro. You may parry twice during one turn while in Parry Stance.",
                    Comment = "May be used when attacked."
                },
                new Perk(){
                    Category = PerkCategory.Combat,
                    Name = PerkName.StunningStrike,
                    Effect = "Your hero may choose to stun the enemy instead of inflicting wounds. Your hero performs a Standard Attack with a -10 CS penalty and if the attack is successful, the enemy must pass a RES test or it may perform NO actions during its next turn.",
                    Comment = "Only melee weapons. Does not work on X-Large creatures and Large creatures only lose 1 AP."
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.CleverFingers,
                    Effect = "Relying on them experience, them fingers dance across the mechanism. Add +25 bonus to a single pick lock or disarming trap attempt.",
                    Comment = "Use before rolling."
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.HideInTheShadows,
                    Effect = "Your hero finds that perfect spot to avoid drawing attention. No enemy will target them if they are more than 2 squares away when they start their turn. If your model is adjacent to an enemy, that enemy will always attack another adjacent model if there is one. If not, the Perk will not work.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.LivingOnNothing,
                    Effect = "Accustomed to hardship, your hero can sustain themselves on almost nothing. Spending an Energy Point to activate this Perk is considered the same as consuming a ration.",
                    Comment = "The Energy Point cannot be regained in the same rest in which it was spent."
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.LootGoblin,
                    Effect = "Whenever told to roll for an amount of gold, your hero may decide to re-roll the result once.",
                    Comment = "The decision to use this Perk is done after the first dice roll. Second result will stand."
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.LuckyGit,
                    Effect = "As always, lady fortune is smiling at your hero. Reduce the Threat Level by 2.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.QuickDodge,
                    Effect = "Quick reflexes may make all the difference. Hero may dodge, even if the normal dodge is expended.",
                    Comment = "May be used when attacked."
                },
                new Perk(){
                    Category = PerkCategory.Sneaky,
                    Name = PerkName.StrikeToInjure,
                    Effect = "Your hero targets the enemy with extreme precision, striking the most vulnerable area. Ignore the enemy's armour for all your attacks this turn.",
                    Comment = "Must be declared before attacking"
                },
                new Perk(){
                    Category = PerkCategory.Faith,
                    Name = PerkName.GodsFavorite,
                    Effect = "Your hero is well attuned to the gods, and they always seem to listen to them. Once again, their prayer is heard, and all problems seem smaller. Decrease the Threat Level by 1d6.",
                    Comment = ""
                },
                new Perk(){
                    Category = PerkCategory.Faith,
                    Name = PerkName.FateForger,
                    Effect = "By spending an Energy Point, the priest can force a reroll of the Scenario die.",
                    Comment = "Used as soon as the Scenario die has been rolled."
                },
                new Perk(){
                    Category = PerkCategory.Faith,
                    Name = PerkName.Healer,
                    Effect = "Putting that extra effort into tending a wound can make such a difference. When applying a bandage, this Perk adds +3 HP to the result.",
                    Comment = "Used at the same time as the Healing Skill."
                },
                new Perk(){
                    Category = PerkCategory.Faith,
                    Name = PerkName.MyWillBeDone,
                    Effect = "Using their inner strength, the priest manifests tremendous Resolve. Add +10 RES.",
                    Comment = "Lasts until end of next battle"
                },
                new Perk(){
                    Category = PerkCategory.Arcane,
                    Name = PerkName.DispelMaster,
                    Effect = "The wizard is very skilled in the art of countering enemy magic.",
                    Comment = "The wizard gets +10 when rolling to dispel when this Perk is used."
                },
                new Perk(){
                    Category = PerkCategory.Arcane,
                    Name = PerkName.EnergyToMana,
                    Effect = "The wizard has the ability to turn energy into Mana. For each Energy Point spent, the wizard gains 5 Mana.",
                    Comment = "The wizard may spend any number of Energy Points in one go."
                },
                new Perk(){
                    Category = PerkCategory.Arcane,
                    Name = PerkName.InnerPower,
                    Effect = "The wizard increases the power of their magic missiles, causing an extra 1d6 Damage.",
                    Comment = "Must be declared before the spell is cast."
                },
                new Perk(){
                    Category = PerkCategory.Arcane,
                    Name = PerkName.InTuneWithTheMagic,
                    Effect = "Caster may use Focus before trying to identify a Magic Item. However, when attuning herself to the magic that way, she opens the mind enough to risk them Sanity.",
                    Comment = "Works just as if casting a spell but introduces miscast to the roll as well. 1 Action of Focus will give a miscast on 95-00. Increase the risk with 5 for each action."
                },
                new Perk(){
                    Category = PerkCategory.Arcane,
                    Name = PerkName.QuickFocus,
                    Effect = "The wizard has the ability of extreme Focus, increasing the chance to succeed with a spell. Add +10 Arcane Arts Skill without spending an action on focus. Risk for miscast is still increased.",
                    Comment = "Used at the same time as casting a spell. Only lasts for that spell."
                },
                new Perk(){
                    Category = PerkCategory.Alchemist,
                    Name = PerkName.CarefulTouch,
                    Effect = "By taking a little extra time, that specific specimen can be perfect. Chance of getting an Exquisite Ingredient or Part is increased to 20.",
                    Comment = "Declare before harvesting or gathering."
                },
                new Perk(){
                    Category = PerkCategory.Alchemist,
                    Name = PerkName.Connoisseur,
                    Effect = "The alchemist has a knack for identifying potions brewed by others. Grants a +10 bonus to the Alchemy roll on your next attempt to identify a potion (only one per Energy Point).",
                    Comment = "Energy Point is spent at the same time as you try to identify the potion."
                },
                new Perk(){
                    Category = PerkCategory.Alchemist,
                    Name = PerkName.PerfectHealer,
                    Effect = "Your hero's perfect mixing increases the potency of them potions. The Healing Potion heals +3 HP.",
                    Comment = "Used at the same time as the potion is mixed."
                },
                new Perk(){
                    Category = PerkCategory.Alchemist,
                    Name = PerkName.Pitcher,
                    Effect = "With that extra second to aim, your alchemist can throw a bottle with a perfect arc. Grants a +10 RS bonus to your next attempt to throw a potion.",
                    Comment = "Only lasts for one potion and must be declared before throwing."
                },
                new Perk(){
                    Category = PerkCategory.Alchemist,
                    Name = PerkName.PreciseMixing,
                    Effect = "Good mixing skills make all the difference. The alchemist may choose to reroll the result when rolling to see what potions have been created.",
                    Comment = "Energy may be spent after the first dice roll. The second result stands."
                },
                new Perk(){
                    Category = PerkCategory.Alchemist,
                    Name = PerkName.Surgeon,
                    Effect = "Taking a deep breath to calm the nerves, the alchemist can remove most parts with precision. The alchemist may choose what part to harvest.",
                    Comment = "Only works on one enemy per Energy Point spent."
                }
            };
        }

        public List<Perk> GetPerksByCategory(PerkCategory category)
        {
            return Perks.Where(p => p.Category == category).ToList();
        }

        public Perk GetPerkByName(PerkName name)
        {
            return Perks.FirstOrDefault(t => t.Name == name) ?? new Perk();
        }

        public List<Species> GetSpecies()
        {
            return new List<Species>()
            {
                new Species() {
                    Name = "Dwarf",
                    Description = "Dwarves are short, but broad and often muscular after their mandatory service in the mines under the mountains of the world. Their beards grow thick and long, and they serve as a symbol of their status. The longer the beard, the more respect they earn amongst their kindred. Female dwarves are every bit as sturdy as their male counterparts, and are seen just as often on the battlefield wielding an axe or warhammer. There have been numerous conflicts between Dwarf and Goblin clans, and with the Dwarves inability to forget a misdeed against them, this has led to a full-blown hatred.",
                    BaseStrength = 40,
                    BaseConstitution =  30,
                    BaseDexterity =  25,
                    BaseWisdom =  25,
                    BaseResolve =  30,
                    BaseHitPoints =  8,
                    MaxSTR = 80,
                    MaxDEX = 60,
                    MaxWIS = 80,
                    MaxRES = 80,
                    MaxCON = 70
                },
                new Species() {
                    Name = "Elf",
                    Description = "Elves are fair skinned, graceful, and often beautiful beings. They move gracefully, but can be exceptionally fast. Whilst Dwarves like to live far underground, Elves prefer to live in the forests of the world. Elven fighters are renowned for their skill with bows, but they often make good Wizards as well.",
                    BaseStrength = 25,
                    BaseConstitution =  20,
                    BaseDexterity =  40,
                    BaseWisdom =  35,
                    BaseResolve =  30,
                    BaseHitPoints =  6,
                    MaxSTR = 60,
                    MaxDEX = 80,
                    MaxWIS = 80,
                    MaxRES = 80,
                    MaxCON = 65
                },
                new Species() {
                    Name = "Human",
                    Description = "Humans are the most versatile of all species, with a wide range of physical characteristics and abilities. They are known for their adaptability and resourcefulness, as well as their ability to form complex societies. With their versatility, they could be anything from Wizards to lowly Thieves",
                    BaseStrength = 30,
                    BaseConstitution =  30,
                    BaseDexterity =  30,
                    BaseWisdom =  30,
                    BaseResolve =  30,
                    BaseHitPoints =  7,
                    MaxSTR = 70,
                    MaxDEX = 70,
                    MaxWIS = 80,
                    MaxRES = 80,
                    MaxCON = 65
                },
                new Species() {
                    Name = "Halfling",
                    Description = "Halfling's are small and nimble, with a love for adventure and exploration. They are known for their quick reflexes and stealthy nature, as well as their ability to blend in with their surroundings. They often gravity towards stealthier professions, such as thieves or rougues. There are not many known Halfling Wizards or alchemists, but this is not unheard of. Their preferred weapons are daggers, shorswords, or ranged weapons, such as shortbows and slings.",
                    BaseStrength = 20,
                    BaseConstitution =  20,
                    BaseDexterity =  40,
                    BaseWisdom =  30,
                    BaseResolve =  40,
                    BaseHitPoints =  5,
                    MaxSTR = 40,
                    MaxDEX = 80,
                    MaxWIS = 80,
                    MaxRES = 80,
                    MaxCON = 60
                }
            };
        }

        public List<Profession> GetProfessions()
        {
            return new List<Profession>()
            {
                new Profession {
                    Name = "Wizard",
                    Description = "Wizards have studied the magical arts at one of the sorcery colleges. These colleges admit only a select few, and the power that they wield can be quite remarkable in the eyes of commoners. Consequently, most wizards are regarded with suspicion. Their spells range from petty Hedge Magic, which is little more than conjuring tricks, to powerful Battle Magic or Spells which allow the wizard to bend the will of demons. However, dealing with magic is dangerous and any mistake can ravage your mind.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = -10,
                    DodgeSkillModifier = -10,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = 5,
                    HealSkillModifier = -5,
                    AlchemySkillModifier = -20,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = 10,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "RangedSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 2,
                    MaxMeleeWeaponType = 5,
                    TalentChoices = [GetTalentByName(TalentName.Wise), GetTalentByName(TalentName.Charming)],
                    StartingBackpackList = [EquipmentService.GetMeleeWeaponByName("Staff") as MeleeWeapon],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 5 },
                      {"DEX", 4 },
                      {"CON", 4 },
                      {"WIS", 2 },
                      {"RES", 3 },
                      {"CS", 5 },
                      {"RS", 4 },
                      {"Dodge", 3 },
                      {"PickLocks", 4 },
                      {"Perception", 2 },
                      {"Heal", 2 },
                      {"ArcaneArts", 1 },
                      {"Foraging", 5 },
                      {"Barter", 1 },
                      {"Alchemy", 3 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Rogue",
                    Description = "The Rogue walks a thin line between what is legal and what is illegal. Some overstep the line completely to become highwaymen, robbing unfortunate passers-by. Others take day-to-day work drifting from one town to another. Accustomed to a harsh life and surviving on their own, they are Jacks of all trades, ready to do whatever it takes to make a living.",
                    CombatSkillModifier = 0,
                    RangedSkillModifier = 0,
                    DodgeSkillModifier = 0,
                    PickLocksSkillModifier = -5,
                    BarterSkillModifier = 5,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -25,
                    PerceptionSkillModifier = 0,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = 0,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "PickLocksSkill",
                      "HealSkill",
                      "AlchemySkill"
                    ],
                    HPModifier = 1,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Shortsword/Rapier"],
                    StartingBackpackList = [
                        EquipmentService.GetArmourByName("Padded Jacket") as Armour,
                        EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", 10),
                        EquipmentService.GetEquipmentByName("Backpack - Medium") ],
                    StartingTalentList = [ GetTalentByName(TalentName.Backstabber),
                        new Talent() {
                            Category = TalentCategory.Sneaky,
                            Name = TalentName.Streetwise,
                            Description = "Your hero knows who to turn to in order to acquire the gear he is searching for. Every roll this hero makes for availability may be modified with -1.",
                        } ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 3},
                      {"DEX", 2},
                      {"CON", 3},
                      {"WIS", 4},
                      {"RES", 3 },
                      {"CS", 3},
                      {"RS", 3 },
                      {"Dodge", 3 },
                      {"PickLocks", 3 },
                      {"Perception", 3 },
                      {"Heal", 3 },
                      {"Foraging", 3 },
                      {"Barter", 3 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Ranger",
                    Description = "The Ranger spends their or them days in the wild. They make their living by tracking animals and selling their meat and pelts. Rangers earn a meagre income, but with time they will acquire unrivalled knowledge in how to survive in the wild, and they will seldom go hungry. Constant exposure to the weather and wandering the forests day after day also makes them quite tough and resilient. Their favourite weapon is, of course, the bow. However, some prefer the heavier crossbow for its sheer stopping power.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = 15,
                    DodgeSkillModifier = -5,
                    PickLocksSkillModifier = -25,
                    BarterSkillModifier = -20,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -20,
                    PerceptionSkillModifier = 0,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = 15,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "BarterSkill",
                      "HealSkill",
                      "AlchemySkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    TalentChoices = [GetTalentByName(TalentName.Marksman), GetTalentByName(TalentName.Hunter)],
                    StartingBackpackList = [
                        EquipmentService.GetRangedWeaponByName("Longbow") as RangedWeapon,
                        EquipmentService.GetAmmoByNameSetQuantity("Arrow", 10) ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 3 },
                      {"DEX", 2},
                      {"CON", 1},
                      {"WIS", 4},
                      {"RES", 3},
                      {"CS", 3 },
                      {"RS", 1 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 2 },
                      {"Heal", 2 },
                      {"Foraging", 1 },
                      {"Barter", 3 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Barbarian",
                    Description = "Barbarians live for the thrill of battle. Unlike most sane people, Barbarians eagerly await the next possibility for a good fight, and they often work themselves up into a frenzy once the battle starts. This frenzy causes them to wield their weapons like dervishes, striking out left and right, which makes them formidable. On the other hand, being in the grips of such a frenzy makes it easy to abandon caution and to forget to properly protect yourself.",
                    CombatSkillModifier = 15,
                    RangedSkillModifier = -10,
                    DodgeSkillModifier = 5,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = -15,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -25,
                    PerceptionSkillModifier = -5,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -15,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "RangedSkill",
                      "PickLocksSkill",
                      "BarterSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 2,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = [ "Weapon of Choice" ],
                    StartingPerkList = [GetPerkByName(PerkName.Frenzy)],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 2 },
                      {"DEX", 2},
                      {"CON", 2},
                      {"WIS", 5},
                      {"RES", 3},
                      {"CS", 1 },
                      {"RS", 3 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 4 },
                      {"Heal", 4 },
                      {"Foraging", 4 },
                      {"Barter", 5 },
                      {"Alchemy", 5 },
                      {"HitPoints", 5 }
                    }
                },
                new Profession {
                    Name = "Warrior Priest",
                    Description = "Warrior Priests have taken it upon themselves to act as soldiers of their god, preaching to those who will listen, and smiting those that they regard as heretics. Luckily, their codex will normally limit those deemed as heretics to the monsters of the world, or to those who choose to dabble with dark magic or evil gods. Their faith makes them unwavering in front of the most fearsome foes. The more experienced priests have learned to perfect the art of battle prayers, bestowing some blessings from their god to their comrades in arms.",
                    CombatSkillModifier = 5,
                    RangedSkillModifier = -5,
                    DodgeSkillModifier = -5,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = -10,
                    HealSkillModifier = 5,
                    AlchemySkillModifier = -15,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = 15,
                    FreeSkills = [
                      "RangedSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "BarterSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 1,
                    MaxArmourType = 4,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Weapon of choice", "Religious Relic of choice" ],
                    TalentChoices = [ GetTalentByName(TalentName.Braveheart), GetTalentByName(TalentName.Confident) ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 3 },
                      {"DEX", 3},
                      {"CON", 3},
                      {"WIS", 3},
                      {"RES", 2},
                      {"CS", 2 },
                      {"RS", 2 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 4 },
                      {"Heal", 2 },
                      {"BattlePrayers", 1 },
                      {"Foraging", 4 },
                      {"Barter", 3 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Warrior",
                    Description = "The Warrior is a veteran of the King's army and has more often than not seen countless battles. In contrast with the Berserker, the Warrior does not relish the fight and is more than happy to skip the risk it brings with it. However, when battle is unavoidable, the Warrior is effective in combat with a good all-round set of skills.",
                    CombatSkillModifier = 10,
                    RangedSkillModifier = 5,
                    DodgeSkillModifier = 0,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = -15,
                    HealSkillModifier = -10,
                    AlchemySkillModifier = -25,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -15,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "PickLocksSkill",
                      "BarterSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 3,
                    MaxArmourType = 4,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Weapon of Choice"],
                    TalentChoices = [ GetTalentByName(TalentName.MightyBlow), GetTalentByName(TalentName.Braveheart)],
                    StartingBackpackList = [EquipmentService.GetArmourByName("Leather Jacket") as Armour ],
                    StartingTalentList = [GetTalentByName(TalentName.Disciplined) ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 2 },
                      {"DEX", 2},
                      {"CON", 2},
                      {"WIS", 5},
                      {"RES", 3},
                      {"CS", 1 },
                      {"RS", 2 },
                      {"Dodge", 3 },
                      {"PickLocks", 5 },
                      {"Perception", 4 },
                      {"Heal", 4 },
                      {"Foraging", 4 },
                      {"Barter", 4 },
                      {"Alchemy", 5 },
                      {"HitPoints", 5 }
                    }
                },
                new Profession {
                    Name = "Alchemist",
                    Description = "The Alchemist has spent years studying the properties of materials and solutions, learning the effect they have on each other and on the human body. Through both study and experience, they have learned how to mix ingredients to obtain beneficial effects that have become highly sought after. Their ability to create powerful concoctions have made them popular amongst adventurers. Many alchemists can hold their ground pretty well, spreading fire and destruction all around them.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = -5,
                    DodgeSkillModifier = -10,
                    PickLocksSkillModifier = -20,
                    BarterSkillModifier = 0,
                    HealSkillModifier = 5,
                    AlchemySkillModifier = 10,
                    PerceptionSkillModifier = -10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "RangedSkill",
                      "DodgeSkill",
                      "PickLocksSkill",
                      "PerceptionSkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 5,
                    EquipmentChoices = ["Potions x 3 of choice", "Parts x 3 of choice", "Recipe of choice"],
                    StartingBackpackList = [
                        EquipmentService.GetEquipmentByName("Alchemist Tool"),
                        EquipmentService.GetEquipmentByName("Alchemist Belt"),
                        EquipmentService.GetMeleeWeaponByName("Shortsword") as MeleeWeapon,
                        AlchemyService.GetIngredients(3)[0],
                        AlchemyService.GetIngredients(3)[0],
                        AlchemyService.GetIngredients(3)[0]
                        ],
                    StartingTalentList = [ GetTalentByName(TalentName.ResistPoison) ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 5 },
                      {"DEX", 4},
                      {"CON", 4},
                      {"WIS", 2},
                      {"RES", 3},
                      {"CS", 3 },
                      {"RS", 3 },
                      {"Dodge", 4 },
                      {"PickLocks", 4 },
                      {"Perception", 2 },
                      {"Heal", 3 },
                      {"Foraging", 4 },
                      {"Barter", 3 },
                      {"Alchemy", 1 },
                      {"HitPoints", 10 }
                    }
                },
                new Profession {
                    Name = "Thief",
                    Description = "The Thief prefers to work in the shadows, avoiding all attention if possible. The use of arms is not alien to them, but it is seen as a last resort. Better to take what you want undetected, and thereby minimise the risk of getting injured whilst doing it. As a consequence, daggers are their preferred weapons. Lock picks and crowbars are the tools of the trade. Special: Whenever it is time to get treasure, a thief may always get two choices and choose which one to keep. This ability may be combined with the sense of gold talent.",
                    CombatSkillModifier = -5,
                    RangedSkillModifier = 5,
                    DodgeSkillModifier = 5,
                    PickLocksSkillModifier = 10,
                    BarterSkillModifier = 0,
                    HealSkillModifier = -20,
                    AlchemySkillModifier = -30,
                    PerceptionSkillModifier = 10,
                    ArcaneArtsSkillModifier = null,
                    ForagingSkillModifier = -20,
                    BattlePrayersSkillModifier = null,
                    FreeSkills = [
                      "CombatSkill",
                      "HealSkill",
                      "AlchemySkill",
                      "ForagingSkill"
                    ],
                    HPModifier = 0,
                    MaxArmourType = 3,
                    MaxMeleeWeaponType = 2,
                    StartingBackpackList = [
                        EquipmentService.GetMeleeWeaponByName("Dagger") as MeleeWeapon,
                        EquipmentService.GetEquipmentByName("Rope"),
                        EquipmentService.GetEquipmentByNameSetQuantity("Lock Picks", 10) ],
                    StartingTalentList = [ GetTalentByName(TalentName.Evaluate) ],
                    LevelUpCost = new Dictionary<string, int>(){
                      {"STR", 5},
                      {"DEX", 2},
                      {"CON", 4},
                      {"WIS", 3},
                      {"RES", 3 },
                      {"CS", 5},
                      {"RS", 2 },
                      {"Dodge", 1 },
                      {"PickLocks", 1 },
                      {"Perception", 1 },
                      {"Heal", 4 },
                      {"Foraging", 4 },
                      {"Barter", 2 },
                      {"Alchemy", 4 },
                      {"HitPoints", 10 }
                    }
                }
            };
        }

        public Dictionary<string, int> GetLevelUpCostByProfession(Profession profession)
        {
            return profession.LevelUpCost;
        }

        public int GetProfessionMaxArmourType(string professionName)
        {
            Profession? profession = Professions.FirstOrDefault(p => p.Name == professionName);
            return profession != null ? profession.MaxArmourType : 0;
        }

        internal Species GetSpeciesByName(string speciesName)
        {
            throw new NotImplementedException();
        }
    }

    public enum DamageType
        {
            Mundane,
            Silver,
            Fire,
            Frost,
            Water,
            Lightning,
            Acid,
            Poison,
            Magic
        }

    public class Prayer
        {
            public string Name { get; set; } = string.Empty;
            public int Level { get; set; }
            public int EnergyCost { get; set; } = 1;
            public bool IsActive { get; set; }
            public string Duration { get; set; } = string.Empty;
            public string PrayerEffect { get; set; } = string.Empty;// This could be an enum or a more complex object if effects become varied.

            // Constructor
            public Prayer()
            {

            }

            public override string ToString()
            {
                return $"[{Name} (Lvl {Level})] Cost: {EnergyCost} Energy | Duration: {Duration} | Effect: {PrayerEffect}";
            }

            // Method to get the prayer effect description
            // This method can be expanded to apply the effect in a game logic service.
            public string GetPrayerEffectDescription()
            {
                return PrayerEffect;
            }

            // Example method for applying the prayer effect (logic would typically be in a service)
            public void ApplyEffect(Hero hero)
            {
                // This is a placeholder. Real logic would depend on the 'PrayerEffect' string
                // or if 'PrayerEffect' was an enum/interface.
                Console.WriteLine($"{hero.Name} is affected by {Name}: {PrayerEffect}");
                // Example: if PrayerName == "Heal" -> hero.HP += amount;
            }
        }

    public enum PerkCategory
        {
            Leader,
            Common,
            Combat,
            Sneaky,
            Faith,
            Arcane,
            Alchemist
        }

    public enum PerkName
    {
        CallToAction,
        Encouragement,
        KeepCalmAndCarryOn,
        Rally,
        IgnoreWounds,
        SixthSense,
        Sprint,
        Taunt,
        TasteForBlood,
        BattleFury,
        DeadlyStrike,
        Frenzy,
        HuntersEye,
        PerfectAim,
        PowerfulBlow,
        ShieldBash,
        ShieldWall,
        StunningStrike,
        CleverFingers,
        HideInTheShadows,
        LivingOnNothing,
        LootGoblin,
        LuckyGit,
        QuickDodge,
        StrikeToInjure,
        GodsFavorite,
        FateForger,
        Healer,
        MyWillBeDone,
        DispelMaster,
        EnergyToMana,
        InnerPower,
        InTuneWithTheMagic,
        QuickFocus,
        CarefulTouch,
        Connoisseur,
        PerfectHealer,
        Pitcher,
        PreciseMixing,
        Surgeon
    }

    public class Perk
    {
        public PerkName Name { get; set; }
        public PerkCategory Category { get; set; }
        public string Effect { get; set; } = string.Empty;
        public string? Comment { get; set; }

        public Perk() { }

        public override string ToString()
        {
            var sb = new StringBuilder();
            // Add spaces before capital letters for readability
            sb.Append($"{Regex.Replace(Name.ToString(), "(\\B[A-Z])", " $1")}: {Effect}");
            if (!string.IsNullOrEmpty(Comment))
            {
                sb.Append($" ({Comment})");
            }
            return sb.ToString();
        }

    }
}
