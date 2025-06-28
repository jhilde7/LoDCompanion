using LoDCompanion.Models.Characters;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.Settlement
{
    public class SettlementManagerService
    {
        private readonly GameDataRegistryService _gameData;
        public List<Hero> Heros {  get; set; } = new List<Hero>();

        SettlementManagerService(GameDataRegistryService gameData, List<Hero> heros) 
        { 
            _gameData = gameData;
            Heros = heros;
        }

        public string Rest()
        {
            throw new NotImplementedException();
        }

    }
}
