using LoDCompanion.Models.Characters;
using LoDCompanion.Utilities;
using System.Linq;

namespace LoDCompanion.Services.GameData
{
    public class PrayerLookupService
    {
        private readonly GameDataRegistryService _gameData;
        
        public PrayerLookupService(GameDataRegistryService gamedata)
        {
            _gameData = gamedata;
        }

        public Prayer? GetPrayer(string prayerName)
        {
            return _gameData.GetPrayerByName(prayerName);
        }

        public List<Prayer>? GetPrayersByLevel(int level)
        {
            return _gameData.GetPrayersByLevel(level);
        }

        internal List<Prayer> GetStartingPrayers()
        {
            var prayers = new List<Prayer>();
            var possiblePrayers = _gameData.GetPrayersByLevel(1);
            if (possiblePrayers == null)
            {
                throw new ArgumentException("No spells found for level 1.");
            }

            for (int i = 0; i < 2; i++)
            {
                Prayer prayer;
                do
                {
                    prayer = possiblePrayers[RandomHelper.GetRandomNumber(0, possiblePrayers.Count - 1)];
                } while (prayers.Contains(prayer));
                prayers.Add(prayer);
            }
            return prayers;
        }
    }
}