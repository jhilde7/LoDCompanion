using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using System.Xml.Linq;

namespace LoDCompanion.Code.BackEnd.Services.GameData
{
    public enum PrayerName
    {
        BringerOfLight,
        ThePowerOfIphy,
        CharusWalkWithUs,
        MetheiasWard,
        PowerOfTheGods,
        LitanyOfMetheia,
        PowerOfFaith,
        SmiteTheHeretics,
        VerseOfTheSane,
        ShieldOfTheGods,
        StrengthsOfRamos,
        WarriorsOfOhlnir,
        StayThyHand,
        ProvidenceOfMetheia,
        WeShallNotFalter,
        GodsChampion,
    }

  public class PrayerService()
  {

    public static List<Prayer> Prayers => GetPrayers();


    public static List<Prayer> GetPrayers()
    {
            return new List<Prayer>()
      {
        new Prayer(){
          Name = PrayerName.BringerOfLight,
          Level = 1,
          PrayerEffect = "The light of the gods shines through the priest, causing the Undead to waver. Any Undead trying to attack the Warrior Priest suffers -10 CS.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.BringerOfLight, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.ThePowerOfIphy,
          Level = 1,
          PrayerEffect = "This empowering psalm strengthens your resolve. The party gets +10 RES on any Fear or Terror Test during the battle. If they already have failed these tests, they may retake them with this bonus.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.ThePowerOfIphy, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.CharusWalkWithUs,
          Level = 1,
          PrayerEffect = "This prayer goes to Charus and as long as he listens, all heroes regain an Energy Point on any skill roll of 01-10, instead of the normal 01-05. Note that this only affects energy, not the other options you have if you roll 01-05. However, the priest will be too busy with the prayer to benefit from this.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.CharusWalkWithUs, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.MetheiasWard,
          Level = 1,
          PrayerEffect = "Under the protection of Metheia, the priest regains 1 lost HP at the start of their activation, for the rest of the battle.",
          Duration = "For the rest of the battle.",
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.MetheiasWard, -1, removeAfterCombat: true)
        },
        new Prayer(){
          Name = PrayerName.PowerOfTheGods,
          Level = 1,
          PrayerEffect = "By channelling the power of the gods and diverting it to a wizard, the priest can help conjure a spell. As long as the prayer is active, any hero wizard gains +10 Arcane Arts.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.PowerOfTheGods, -1, skillBonus: (Skill.ArcaneArts, 10), removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.LitanyOfMetheia,
          Level = 2,
          PrayerEffect = "Metheia watches over the heroes and grants them the power of life. Every hero that passes a RES test at the start of their activation regains 1 HP.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.LitanyOfMetheia, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.PowerOfFaith,
          Level = 2,
          PrayerEffect = "The gods grant your party inner strength, making them immune to fear, and treating terror as fear. Heroes already scared will regain their courage if this prayer succeeds.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.PowerOfFaith, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.SmiteTheHeretics,
          Level = 2,
          PrayerEffect = "The wrath of the gods renders the flesh of your enemies. At the start of each turn, the enemies within 4 squares of the priest must pass a RES test or lose 1 HP.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.SmiteTheHeretics, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.VerseOfTheSane,
          Level = 2,
          PrayerEffect = "As long as this verse is read, the heroes are less prone to mental scars. Each event that would trigger a loss of a Sanity Point is negated by a RES test. If the test succeeds, the Sanity Point is not lost.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.VerseOfTheSane, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.ShieldOfTheGods,
          Level = 2,
          PrayerEffect = "The gods will protect the pious, and as long as this prayer is active, any wizard will be protected from miscast. Any Miscast roll can be ignored, although the priest will instead have to pass a RES test, or suffer 1d4 DMG with no armour and NA.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.ShieldOfTheGods, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.StrengthsOfRamos,
          Level = 3,
          PrayerEffect = "The party feels invigorated, and their weapons feel like feathers in their hands. All members of the party gain +10 Strength.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.StrengthOfRamos, -1, removeAfterNextBattle: true, statBonus: (BasicStat.Strength, 10))
        },
        new Prayer(){
          Name = PrayerName.WarriorsOfOhlnir,
          Level = 3,
          PrayerEffect = "As if the gods guide the weapons of the heroes, all seem to fight with renewed power. All members of the party fight with +5 CS.",
          Duration = "Until end of next battle, or 4 turns if used between battles.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.WarriorsOfOhlnir, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.StayThyHand,
          Level = 3,
          PrayerEffect = "The enemies seem to slow down, as if questioning whether to fight or not. All enemies within 4 squares of the priest must pass a Resolve Test and will lose 1 Action Point during that turn. Test at the start of every turn. This effect is not cumulative with any other effect causing an enemy to lose an action. For instance, a wounded enemy will not be affected by this prayer.",
          Duration = "Until enemy test succeeds (tested at start of every turn).",
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.StayThyHand, -1)
        },
        /* this prayer is printed like this on the card, which is the same as "Stay Thy Hand!"
        new Prayer(){
          Name = "Be Gone!",
          Level = 3,
          PrayerEffect = "The enemies seem to slow down, as if questioning whether to fight or not. All enemies within 4 squares of the priest must pass a Resolve Test and will lose 1 Action Point during that turn. Test at the start of every turn. This effect is not cumulative with any other effect causing an enemy to lose an action. For instance, a wounded enemy will not be affected by this prayer.",
          Duration = "Until enemy test succeeds (tested at start of every turn)."
        },*/
        new Prayer(){
          Name = PrayerName.ProvidenceOfMetheia,
          Level = 3,
          PrayerEffect = "Metheia will shield its children and protect them from harm. While this prayer is active, all heroes get +10 CON when rolling to resist disease or poison.",
          Duration = "While prayer is active (until end of next battle, or 4 turns if used between battles).",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.ProvidenceOfMetheia, -1, removeAfterNextBattle: true)
        },
        new Prayer(){
          Name = PrayerName.WeShallNotFalter,
          Level = 4,
          PrayerEffect = "The power of the gods strengthens the party, making them more resilient than ever. All members of the party gain +5 HP that can temporarily give a hero more HP than its current max. After the battle, this goes back to the normal max HP.",
          Duration = "Until end of battle.",
          EffectsParty = true,
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.WeShallNotFalter, -1, removeAfterCombat: true, statBonus: (BasicStat.HitPoints, 5))
        },
        new Prayer(){
          Name = PrayerName.GodsChampion,
          Level = 4,
          PrayerEffect = "The priest fights like a dervish, imbued by the power of their or them god. Combat Skill is increased by +15 but after the battle, the priest loses an additional Point of Energy. If there are not enough points, the Constitution of the priest is halved (RDD) until the next short rest or until the heroes leave the dungeon.",
          Duration = "Until end of battle (effect on energy/constitution is after battle).",
          ActiveStatusEffect = new ActiveStatusEffect(StatusEffectType.GodsChampion, -1, removeAfterCombat: true, skillBonus: (Skill.CombatSkill, 15))
        }
      };
    }

    internal static List<Prayer> GetPrayersByLevel(int level)
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

        public static Prayer GetPrayerByName(PrayerName name)
        {
            return Prayers.FirstOrDefault(p => p.Name == name) ?? new Prayer();
        }

    }

    public class Prayer
  {
        public PrayerName Name { get; set; }
        public int Level { get; set; }
        public int EnergyCost { get; set; } = 1;
        public string Duration { get; set; } = string.Empty;
        public string PrayerEffect { get; set; } = string.Empty;// This could be an enum or a more complex object if effects become varied.
        public ActiveStatusEffect ActiveStatusEffect { get; set; } = new ActiveStatusEffect(StatusEffectType.NeedRest, 0);
        public TargetType TargetType { get; set; } = TargetType.Self;
        public bool EffectsParty { get; set; }

        // Constructor
        public Prayer()
        {

        }

        public override string ToString()
        {
          return $"[{Name} (Lvl {Level})] Cost: {EnergyCost} Energy | Duration: {Duration} | Effect: {PrayerEffect}";
        }
  }

}