using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Services.Settlement
{
    public class SettlementManagerService
    {
        private readonly GameDataService _gameData;
        public List<Hero> Heros {  get; set; } = new List<Hero>();

        SettlementManagerService(GameDataService gameData, List<Hero> heros) 
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
