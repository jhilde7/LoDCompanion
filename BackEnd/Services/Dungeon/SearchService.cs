using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using Microsoft.AspNetCore.Rewrite;
using System.Text;

namespace LoDCompanion.BackEnd.Services.Dungeon
{
    /// <summary>
    /// Represents the result of a search action.
    /// </summary>
    public class SearchResult
    {
        public bool WasSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Equipment> FoundItems { get; set; } = new List<Equipment>();
    }

    /// <summary>
    /// Handles all logic related to searching rooms, furniture, and corpses.
    /// </summary>
    public class SearchService
    {
        private readonly UserRequestService _diceRoll;
        private readonly TreasureService _treasure;

        public SearchService(UserRequestService diceRollService, TreasureService treasure)
        {
            _diceRoll = diceRollService;
            _treasure = treasure;
        }


        /// <summary>
        /// This method encapsulates the original `SearchRoom` logic.
        /// The actual search roll and treasure generation would be performed by a service,
        /// which would then update the `SearchResults` of this room.
        /// The _treasure and RandomHelper (formerly Utilities) are injected/passed.
        /// </summary>
        /// <param name="hero">The hero performing the search.</param>
        /// <param name="treasureService">The service for generating treasure.</param>
        /// <param name="randomHelper">A utility for random number generation.</param>
        public async Task SearchRoomAsync(Room room, Hero hero, bool isPartySearch = false)
        {
            if (room.HasBeenSearched)
            {
                return;
            }

            int searchTarget = hero.GetSkill(Skill.Perception);
            if (isPartySearch)
            {
                searchTarget += 20;
            }
            if (hero.Inventory.OffHand is Equipment offHand)
            {
                if (offHand.HasProperty(EquipmentProperty.Torch))
                {
                    searchTarget += 5;
                }
                else if (offHand.HasProperty(EquipmentProperty.Lantern))
                {
                    searchTarget += 10;
                } 
            }
            var resultRoll = await _diceRoll.RequestRollAsync("Attempt to search the room", "1d100", hero: hero, skill: Skill.Perception); await Task.Yield();
            int searchRoll = resultRoll.Roll;


            if (room.Category == RoomCategory.Corridor)
            {
                searchRoll += 10;
            }

            room.SearchResults.Clear();

            if (searchRoll <= searchTarget)
            {
                resultRoll = await _diceRoll.RequestRollAsync("Search successful, roll for treasure", "1d100"); await Task.Yield();
                int treasureRoll = resultRoll.Roll;
                // Original logic from SearchRoom(string type, bool isThief, int roll)
                int count = hero.IsThief ? 2 : 1;

                switch (treasureRoll)
                {
                    case int r when r >= 1 && r <= 15:
                        Console.WriteLine("You found a secret door leading to a small _treasure chamber. Place tile R10 adjacent to the current tile and add a door as usual. Re-roll if tile is in use. Once the heroes leave the treasure chamber, the door closes up and the tile can be removed.");
                        // Note: The logic for creating a new room/door (GetRoom, Instantiate)
                        // must be handled by DungeonManagerService. This just adds the text result.
                        // You'd have to signal back to the DungeonManagerService to create this room.
                        break;
                    case int r when r >= 16 && r <= 25:
                        room.SearchResults.AddRange(await _treasure.FoundTreasureAsync(TreasureType.Fine, count));
                        break;
                    case int r when r >= 26 && r <= 40:
                        room.SearchResults.AddRange(await _treasure.FoundTreasureAsync(TreasureType.Mundane, count));
                        break;
                    case int r when r >= 41 && r <= 45:
                        Console.WriteLine("You found a set of levers. (Interaction handled by a LeverService)");
                        room.HasLevers = true; // Update room state
                        break;
                    case int r when r >= 46 && r <= 50:
                        room.SearchResults.Add(await _treasure.GetTreasureAsync("Coin", 0, 1, RandomHelper.GetRandomNumber(4, 40)));
                        break;
                    case int r when r >= 91 && r <= 100:
                        Console.WriteLine("You've sprung a trap!");
                        // A TrapService or DungeonManagerService would handle the trap instantiation/effect.
                        // You might set a flag here or return a Trap object.
                        // CurrentTrap = newTrap; // Example: if Trap is a simple data class.
                        break;
                    default:
                        Console.WriteLine("You found Nothing");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Search Failed");
            }
            room.SearchRoomTrigger = false; // Reset trigger
            room.HasBeenSearched = true;
        }

        /// <summary>
        /// Performs a search of a single piece of furniture.
        /// </summary>
        /// <param name="furniture">The furniture being searched.</param>
        /// <returns>A SearchResult object detailing the outcome.</returns>
        public async Task<SearchResult> SearchFurnitureAsync(Furniture furniture)
        {
            var result = new SearchResult();
            if (furniture.HasBeenSearched)
            {
                result.Message = $"The {furniture.Name} has already been searched.";
                return result;
            }

            if (!furniture.IsSearchable)
            {
                result.Message = $"The {furniture.Name} cannot be searched.";
                return result;
            }

            result.WasSuccessful = true;
            result.Message = $"You search the {furniture.Name}...";
            result.FoundItems = await _treasure.FoundTreasureAsync(TreasureType.Mundane, 1); // TODO: need to get specific treasure table for furniture

            furniture.HasBeenSearched = true;
            return result;
        }

        /// <summary>
        /// Performs a search of a defeated monster's corpse.
        /// </summary>
        /// <param name="hero">The hero searching the corpse.</param>
        /// <param name="corpse">The corpse to be searched.</param>
        /// <returns>A SearchResult object detailing the outcome.</returns>
        public async Task<SearchResult> SearchCorpseAsync(Hero hero, Corpse corpse)
        {
            var result = new SearchResult();
            if (corpse.HasBeenSearched)
            {
                result.Message = "This corpse has already been looted.";
                return result;
            }

            result.WasSuccessful = true;
            result.Message = $"{hero.Name} searches the remains...";
            result.FoundItems = await _treasure.SearchCorpseAsync(corpse.TreasureType, hero, 0);

            corpse.HasBeenSearched = true;
            return result;
        }
    }

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
        public GridPosition? Position { get; set; } = new GridPosition(0, 0, 0);
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
            if (Position == null) return;
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
        public Monster OriginMonster { get; set; }
        public bool HasBeenHarvested { get; set; }

        public Corpse(Monster monster)
        {
            Name = $"{monster.Name} Corpse";
            OriginMonster = monster;
            TreasureType = monster.TreasureType;
            HasBeenSearched = false;
            Treasures = new List<string>();
        }

        /// <summary>
        /// Attempts to search the corpse for treasures.
        /// </summary>
        /// <param name="hero">The hero attempting to search.</param>
        /// <param name="searchRoll">The result of the hero's search roll.</param>
        /// <returns>A list of treasures found.</returns>
        public async Task<List<string>> SearchCorpseAsync(Hero hero, int searchRoll, TreasureService treasure)
        {
            if (HasBeenSearched)
            {
                // Corpse already searched
                return new List<string>();
            }

            await treasure.SearchCorpseAsync(TreasureType, hero, searchRoll);

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
        public bool IsLevers { get; set; }
        public string SpecialRules { get; set; } = string.Empty;
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
            sb.Append($"'{Name}'");

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

    public enum ChestProperty
    {
        None,
        Locked,
        Trapped,
        LockModifier,
        LockHP,
    }

    public class Chest : Furniture
    {
        public Dictionary<ChestProperty, int>? Properties { get; set; }

        public Chest()
        {

        }

        //TODO: Implement logic to handle searching a chest
        public void SetLockState(int lockModifier, int lockHP)
        {
            Properties ??= new Dictionary<ChestProperty, int>();
            Properties.TryAdd(ChestProperty.Locked, 0);
            Properties.TryAdd(ChestProperty.LockModifier, lockModifier);
            Properties.TryAdd(ChestProperty.LockHP, lockHP);
        }

        public void SetTrapState()
        {
            Properties ??= new Dictionary<ChestProperty, int>();
            Properties.TryAdd(ChestProperty.Trapped, 0);
        }
    }    
}
