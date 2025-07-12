using LoDCompanion.Services.Dungeon;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Game;
using System.Text;

namespace LoDCompanion.Models.Dungeon
{
    public class Searchable : IGameEntity
    {
        public string Id { get; }
        public string Name { get; set; } = string.Empty;
        public Room Room { get; set; } = new Room();
        public GridPosition Position { get; set; } = new GridPosition(0, 0, 0);
        public List<GridPosition> OccupiedSquares { get; set; } = new List<GridPosition>();
        public bool HasBeenSearched { get; set; }
        public Hero? HeroPerformingSearch { get; set; }
        public TreasureType TreasureType { get; set; } = TreasureType.None; // Default to empty string for safety
        public List<string> Treasures { get; set; }

        public Searchable()
        {
            Id = Guid.NewGuid().ToString();
            HasBeenSearched = false;
            Treasures = new List<string>();
        }
    }

    public class Corpse : Searchable
    {
        public Corpse(TreasureType treasureType)
        {            
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

            TreasureService.SearchCorpse(TreasureType, hero, searchRoll);

            HasBeenSearched = true;
            Console.WriteLine($"{hero.Name} found: {string.Join(", ", Treasures)}");
            return Treasures;
        }
    }

    public class Furniture : Searchable
    {
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

        // Constructor for easy initialization
        public Furniture()
        {

        }

        public override string ToString()
        {
            // Use StringBuilder for efficient string building
            var sb = new StringBuilder();

            // Start with the name from the base class
            sb.Append($"'{this.Name}'");

            // Add the description if it exists
            if (!string.IsNullOrEmpty(Description))
            {
                sb.Append($": {Description}");
            }

            // Create a list of features based on the boolean properties
            var features = new List<string>();
            if (IsSearchable) features.Add("Searchable");
            if (IsObstacle) features.Add("Obstacle");
            if (NoEntry) features.Add("Blocks Movement");
            if (BlocksLoS) features.Add("Blocks Line of Sight");
            if (CanBeClimbed) features.Add("Climbable");
            if (HeightAdvantage) features.Add("Grants Height Advantage");
            if (IsLevers) features.Add("Has Levers");
            if (IsDrinkable) features.Add("Drinkable");


            // Append the list of features if any exist
            if (features.Any())
            {
                sb.Append($" [{string.Join(", ", features)}]");
            }

            // Append any special rules
            if (!string.IsNullOrEmpty(SpecialRules))
            {
                sb.Append($" (Rules: {SpecialRules})");
            }

            return sb.ToString();
        }
    }
}
