using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Utilities;
using Microsoft.AspNetCore.Rewrite;
using System.Diagnostics.Eventing.Reader;
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
        public List<Equipment?>? FoundItems { get; set; }
        public Dictionary<string, string>? SpawnMonster { get; internal set; }
        public Dictionary<string, string>? SpawnPlacement { get; internal set; }
    }

    /// <summary>
    /// Handles all logic related to searching rooms, furniture, and corpses.
    /// </summary>
    public class SearchService
    {
        private readonly UserRequestService _diceRoll;
        private readonly TreasureService _treasure;
        private readonly TrapService _trap;
        private readonly DungeonManagerService _dungeonManager;
        public static List<Furniture> Furniture => GetFurniture();

        public SearchService(UserRequestService diceRollService, TreasureService treasure, TrapService trapService, DungeonManagerService dungeonManagerService)
        {
            _diceRoll = diceRollService;
            _treasure = treasure;
            _trap = trapService;
            _dungeonManager = dungeonManagerService;
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

            var searchResults = new SearchResult() { FoundItems = new List<Equipment?>() };
            int searchTarget = hero.GetSkill(Skill.Perception);
            var heroSearching = hero;
            if (hero.Party != null)
            {
                var heroWithHighestPerception = hero.Party.Heroes
                    .Where(h => h != null)
                    .OrderByDescending(h => h.GetSkill(Skill.Perception))
                    .FirstOrDefault();
                if (heroWithHighestPerception != null) 
                { 
                    searchTarget = heroWithHighestPerception.GetSkill(Skill.Perception); 
                    heroSearching = heroWithHighestPerception;
                }

                if (isPartySearch)
                {
                    for (int i = 0; i < hero.Party.Heroes.Count(); i++)
                    {
                        if (i == 0) continue; // this is the initial perception skill check
                        else if (i == 1) searchTarget += 10;
                        else searchTarget += 5;
                    }

                    var partyMemebrWithLantern = hero.Party.Heroes
                        .FirstOrDefault(h => h.Inventory.OffHand != null && h.Inventory.OffHand.HasProperty(EquipmentProperty.Lantern));

                    var partyMemebrWithTorch = hero.Party.Heroes
                        .FirstOrDefault(h => h.Inventory.OffHand != null && h.Inventory.OffHand.HasProperty(EquipmentProperty.Torch));

                    if (partyMemebrWithLantern != null)
                    {
                        searchTarget += 10;
                    }
                    else if (partyMemebrWithTorch != null)
                    {
                        searchTarget += 5;
                    }
                }
            }
            var resultRoll = await _diceRoll.RequestRollAsync("Attempt to search the room", "1d100", skill: (hero, Skill.Perception)); await Task.Yield();
            int searchRoll = resultRoll.Roll;


            if (room.Category == RoomCategory.Corridor)
            {
                searchRoll += 10;
            }

            if (searchRoll <= searchTarget)
            {
                resultRoll = await _diceRoll.RequestRollAsync("Search successful, now roll for treasure", "1d100"); await Task.Yield();
                int treasureRoll = resultRoll.Roll;
                // Original logic from SearchRoom(string type, bool isThief, int roll)
                var partyHasThief = hero.Party != null && hero.Party.Heroes.Any(h => h.IsThief);
                int count = partyHasThief ? 2 : 1;

                switch (treasureRoll)
                {
                    case <= 15:
                        Console.WriteLine("You found a secret door leading to a small _treasure chamber. Place tile R10 adjacent to the current tile and add a door as usual. Re-roll if tile is in use. Once the heroes leave the treasure chamber, the door closes up and the tile can be removed.");
                        // Note: The logic for creating a new room/door (GetRoom, Instantiate)
                        // must be handled by DungeonManagerService. This just adds the text result.
                        // You'd have to signal back to the DungeonManagerService to create this room.
                        room.AddTreasureRoom();
                        break;
                    case <= 25:
                        searchResults.FoundItems.AddRange(await _treasure.FoundTreasureAsync(TreasureType.Fine, count));
                        break;
                    case <= 40:
                        searchResults.FoundItems.AddRange(await _treasure.FoundTreasureAsync(TreasureType.Mundane, count));
                        break;
                    case <= 45:
                        searchResults.Message += "You found a set of levers. (Interaction handled by a LeverService)";
                        room.HasLevers = true; // Update room state
                        break;
                    case <= 50:
                        searchResults.FoundItems.Add(await _treasure.GetTreasureAsync("Coin", 0, 1, RandomHelper.GetRandomNumber(4, 40)));
                        break;
                    case <= 90:
                        searchResults.Message += "You found Nothing";
                        break;
                    case <= 100:
                        searchResults.Message += "You've sprung a trap!";
                        room.CurrentTrap = new Trap();
                        await _trap.TriggerTrapAsync(heroSearching, room.CurrentTrap);
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
        public async Task<SearchResult> SearchFurnitureAsync(Hero hero, Furniture furniture, int searchRoll)
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

            result = await _treasure.SearchFurnitureAsync(hero, furniture, searchRoll); 

            furniture.HasBeenSearched = true;
            return result;
        }

        internal async Task<SearchResult> DrinkFromFurniture(Hero hero, Furniture furniture, int roll)
        {
            var result = new SearchResult();

            if (!furniture.IsDrinkable)
            {
                result.Message = $"The {furniture.Name} cannot be drank from.";
                return result;
            }

            result = await _treasure.DrinkFurnitureAsync(hero, furniture, roll);

            furniture.HasBeenSearched = true;
            return result;
        }

        /// <summary>
        /// Performs a search of a defeated monster's corpse.
        /// </summary>
        /// <param name="hero">The hero searching the corpse.</param>
        /// <param name="corpse">The corpse to be searched.</param>
        /// <returns>A SearchResult object detailing the outcome.</returns>
        public async Task<SearchResult> SearchCorpseAsync(Hero hero, Corpse corpse, int searchRoll)
        {
            if (corpse.HasBeenSearched)
            {                
                return new SearchResult() { Message = "This corpse has already been looted." };
            }
            var result = await _treasure.SearchCorpseAsync(corpse.TreasureType, hero, searchRoll);

            corpse.HasBeenSearched = true;
            return result;
        }

        public static List<Furniture> GetFurniture()
        {
            return new List<Furniture>()
        {
            new Furniture()
            {
                Name = "Altar",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Altar
            },
            new Furniture()
            {
                Name = "Archery Target",
                IsObstacle = true,
                IsSearchable = true,
                TreasureType = TreasureType.ArcheryTarget
            },
            new Furniture()
            {
                Name = "Armour Rack",
                IsObstacle = true,
                IsSearchable = true,
                TreasureType = TreasureType.ArmourRack
            },
            new Furniture()
            {
                Name = "Backpack",
                IsSearchable = true,
                TreasureType = TreasureType.Backpack
            },
            new Furniture()
            {
                Name = "Barrels",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Barrels
            },
            new Furniture()
            {
                Name = "Bed",
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Bed
            },
            new Furniture()
            {
                Name = "Bedroll",
                IsSearchable = true,
                TreasureType = TreasureType.Bedroll
            },
            new Furniture()
            {
                Name = "Bookshelf",
                IsObstacle = true,
                IsSearchable = true,
                NoEntry = true,
                TreasureType = TreasureType.Bookshelf
            },
            new Furniture()
            {
                Name = "Boulder",
                CanBeClimbed = true,
                BlocksLoS = true
            },
            new Furniture()
            {
                Name = "Boxes",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Boxes
            },
            new Furniture()
            {
                Name = "Brazier",
                IsObstacle = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Bridge"
            },
            new Furniture()
            {
                Name = "Chasm",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Chair"
            },
            new Furniture()
            {
                Name = "Cage",
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Special
            },
            new Furniture()
            {
                Name = "Chest",
                IsSearchable= true,
                TreasureType = TreasureType.Chest
            },
            new Furniture()
            {
                Name = "Objective Chest",
                IsSearchable= true,
                TreasureType = TreasureType.ObjectiveChest
            },
            new Furniture()
            {
                Name = "Coffin",
                IsSearchable= true,
                TreasureType = TreasureType.Coffin,
            },
            new Furniture()
            {
                Name = "Dead Adventurer",
                IsSearchable = true,
                TreasureType = TreasureType.DeadAdventurer
            },
            new Furniture()
            {
                Name = "Debris",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Drawer",
                IsObstacle = true,
                IsSearchable = true,
                NoEntry = true,
                TreasureType = TreasureType.Drawer
            },
            new Furniture()
            {
                Name = "Floor"
            },
            new Furniture()
            {
                Name = "Fountain",
                IsObstacle = true,
                IsSearchable = true,
                TreasureType = TreasureType.Fountain,
                IsDrinkable = true,
                DrinkTreasureType = TreasureType.DrinkFountain,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Grate (over a hole)",
                IsSearchable= true,
                TreasureType = TreasureType.GrateOverHole,
            },
            new Furniture()
            {
                Name = "Hearth",
                IsObstacle = true,
                IsSearchable = true,
                NoEntry= true,
                TreasureType = TreasureType.Hearth
            },
            new Furniture()
            {
                Name = "Lava",
                SpecialRules = "Instant death for any character"
            },
            new Furniture()
            {
                Name = "Levers",
                IsLevers = true
            },
            new Furniture()
            {
                Name = "Pillar",
                IsObstacle= true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Pit",
                SpecialRules = "See exploration card for rules"
            },
            new Furniture()
            {
                Name = "Pottery",
                IsSearchable = true,
                TreasureType = TreasureType.Pottery
            },
            new Furniture()
            {
                Name = "Rubble",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Sarcophagus",
                IsSearchable= true,
                IsObstacle = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Sarcophagus
            },
            new Furniture()
            {
                Name = "Stairs"
            },
            new Furniture()
            {
                Name = "Statue",
                IsObstacle = true,
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Tree",
                IsObstacle = true
            },
            new Furniture()
            {
                Name = "Alchemist Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.AlchemistTable
            },
            new Furniture()
            {
                Name = "Dining Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.DiningTable
            },
            new Furniture()
            {
                Name = "Study Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.StudyTable
            },
            new Furniture()
            {
                Name = "Table",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.DiningTable
            },
            new Furniture()
            {
                Name = "Throne",
                IsObstacle = true,
                IsSearchable = true,
                CanBeClimbed = true,
                TreasureType = TreasureType.Throne
            },
            new Furniture()
            {
                Name = "Torture Tools",
                NoEntry = true,
                IsObstacle = true,
                IsSearchable = true,
                TreasureType = TreasureType.TortureTools
            },
            new Furniture()
            {
                Name = "Trap"
            },
            new Furniture()
            {
                Name = "Treasure Pile",
                IsSearchable = true,
                TreasureType = TreasureType.TreasurePile
            },
            new Furniture()
            {
                Name = "Water Basin",
                IsObstacle = true,
                IsDrinkable = true,
                NoEntry = true,
                DrinkTreasureType = TreasureType.DrinkWaterBasin
            },
            new Furniture()
            {
                Name = "Water",
                NoEntry = true
            },
            new Furniture()
            {
                Name = "Weapon Rack",
                IsSearchable = true,
                TreasureType = TreasureType.WeaponRack
            },
            new Furniture()
            {
                Name = "Wall",
                NoEntry = true,
                BlocksLoS = true,
            },
            new Furniture()
            {
                Name = "Well",
                IsSearchable = true,
                IsObstacle = true,
                NoEntry = true,
                TreasureType = TreasureType.Well
            }
        };
        }

        public static Furniture? GetFurnitureByName(string name)
        {
            return Furniture.FirstOrDefault(x => x.Name == name);
        }

        public Furniture? GetFurnitureByNameSetPosition(string name, List<GridPosition> gridPosition)
        {
            var furniture = GetFurnitureByName(name);
            if (furniture != null)
            {
                furniture.OccupiedSquares = gridPosition; 
            }
            return furniture;
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
        public TreasureType? DrinkTreasureType { get; set; }
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

    public class Chest : Furniture
    {
        public Trap Trap { get; set; } = new Trap();
        public Lock Lock { get; set; } = new Lock();

        public Chest()
        {

        }
    }    
}
