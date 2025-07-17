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
        private Room? _room;
        public Room Room
        {
            get => _room ??= new Room();
            set
            {
                _room = value;

                if (_room != null)
                {
                    if (this is Furniture newFurniture)
                    {
                        _room.FurnitureList ??= new List<Furniture>();
                        _room.FurnitureList.Add(newFurniture);
                    }
                    else if (this is Corpse newCorpse)
                    {
                        _room.CorpsesInRoom ??= new List<Corpse>();
                        _room.CorpsesInRoom.Add(newCorpse);
                    }
                }
            }
        }
        public GridPosition Position { get; set; } = new GridPosition(0, 0, 0);
        public List<GridPosition> OccupiedSquares { get; set; } = new List<GridPosition>();
        public bool HasBeenSearched { get; set; }
        public Hero? HeroPerformingSearch { get; set; }
        public TreasureType TreasureType { get; set; } = TreasureType.None; // Default to empty string for safety
        public List<string> Treasures { get; set; }
        public bool IsLarge { get; set; }

        public Searchable()
        {
            Id = Guid.NewGuid().ToString();
            HasBeenSearched = false;
            Treasures = new List<string>();
        }

        internal void UpdateOccupiedSquares()
        {

            OccupiedSquares.Clear();
            int SizeX = 1;
            int SizeY = 1;
            int SizeZ = 1;

            if (IsLarge)
            {
                SizeX = 2;
                SizeY = 2;
                SizeZ = 1;
            }

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        OccupiedSquares.Add(new GridPosition(Position.X + x, Position.Y + y, Position.Z + z));
                    }
                }
            }
        }
    }

    public class Corpse : Searchable
    {

        public Corpse(string name, TreasureType treasureType)
        {
            Name = name;    
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

            TreasureService.SearchCorpseAsync(TreasureType, hero, searchRoll);

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
