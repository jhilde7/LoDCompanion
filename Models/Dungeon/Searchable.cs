using LoDCompanion.Services.Dungeon;
using LoDCompanion.Models.Character;
using LoDCompanion.Services.GameData;

namespace LoDCompanion.Models.Dungeon
{
    public class Searchable
    {
        public bool HasBeenSearched { get; set; }
        public Hero? HeroPerformingSearch { get; set; }
        public string TreasureType { get; set; } = "-"; // Default to empty string for safety
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
        public Corpse(GameDataService gameData, string treasureType)
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
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsObstacle { get; set; }
        public bool IsSearchable { get; set; }
        public bool ContainsTreasure { get; set; }

        // Constructor for easy initialization
        public Furniture(string name, string description, bool isObstacle = false, bool isSearchable = false, bool containsTreasure = false)
        {
            Name = name;
            Description = description;
            IsObstacle = isObstacle;
            IsSearchable = isSearchable;
            ContainsTreasure = containsTreasure;
        }

        // You might add methods here for interactions, e.g., Search() if it contains treasure.
        public string Search()
        {
            if (IsSearchable && ContainsTreasure)
            {
                return $"You search the {Name} and find something!";
            }
            else if (IsSearchable && !ContainsTreasure)
            {
                return $"You search the {Name}, but find nothing.";
            }
            else
            {
                return $"The {Name} cannot be searched.";
            }
        }
    }
}
