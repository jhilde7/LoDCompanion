using LoDCompanion.Services.Dungeon;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Models.Dungeon
{
    public class Searchable
    {
        public bool HasBeenSearched { get; set; }
        public Hero? HeroPerformingSearch { get; set; }
        public TreasureType TreasureType { get; set; } = TreasureType.None; // Default to empty string for safety
        public List<string> Treasures { get; set; }

        public Searchable()
        {
            HasBeenSearched = false;
            Treasures = new List<string>();
        }
    }

    public class Corpse : Searchable
    {
        private readonly GameDataService _gameData;
        public Corpse(GameDataService gameData, TreasureType treasureType)
        {
            _gameData = gameData;
            TreasureType = treasureType;
            HasBeenSearched = false;
            Treasures = new List<string>();
        }

        /// <summary>
        /// Attempts to search the corpse for treasures.
        /// </summary>
        /// <param name="hero">The hero attempting to search.</param>
        /// <param name="searchRoll">The result of the hero's search roll.</param>
        /// <returns>A list of treasures found.</returns>
        public List<string> SearchCorpse(Hero hero, int searchRoll)
        {
            if (HasBeenSearched)
            {
                // Corpse already searched
                return new List<string>();
            }

            new TreasureService(_gameData).SearchCorpse(TreasureType, hero, searchRoll);

            HasBeenSearched = true;
            Console.WriteLine($"{hero.Name} found: {string.Join(", ", Treasures)}");
            return Treasures;
        }
    }

    public class Furniture : Searchable
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsObstacle { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsDrinkable { get; set; }
        public bool IsLevers {  get; set; }
        public string SpecialRules { get; set; } = string.Empty ;
        public bool CanBeClimbed { get; set; }
        public bool HeightAdvantage { get; set; }
        public bool NoEntry { get; set; }
        public bool BlocksLoS { get; set; }
        public List<GridPosition> OccupiedSquares { get; set; } = new List<GridPosition>();

        // Constructor for easy initialization
        public Furniture()
        {

        }
    }
}
