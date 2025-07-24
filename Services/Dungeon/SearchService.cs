using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    /// <summary>
    /// Represents the result of a search action.
    /// </summary>
    public class SearchResult
    {
        public bool WasSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> FoundItems { get; set; } = new List<string>();
    }

    /// <summary>
    /// Handles all logic related to searching rooms, furniture, and corpses.
    /// </summary>
    public class SearchService
    {
        private readonly DiceRollService _diceRoll;
        private readonly TreasureService _treasure;

        public SearchService(DiceRollService diceRollService, TreasureService treasure)
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
                room.SearchResults.Add("This room has already been searched.");
                return;
            }

            int searchTarget = hero.GetSkill(Skill.Perception);
            if (isPartySearch)
            {
                searchTarget += 20;
            }
            if (hero.Inventory.OffHand != null && hero.Inventory.OffHand.HasProperty(Models.EquipmentProperty.Torch))
            {
                searchTarget += 5;
            }
            if (hero.Inventory.OffHand != null && hero.Inventory.OffHand.HasProperty(Models.EquipmentProperty.Lantern))
            {
                searchTarget += 10;
            }

            int searchRoll = await _diceRoll.RollDice("Attempt to search the room", "1d100");

            if (room.Category == RoomCategory.Corridor)
            {
                searchRoll += 10;
            }

            room.SearchResults.Clear();

            if (searchRoll <= searchTarget)
            {
                int treasureRoll = await _diceRoll.RollDice("Search successful, rool for treasure", "1d100");
                // Original logic from SearchRoom(string type, bool isThief, int roll)
                int count = hero.IsThief ? 2 : 1;

                switch (treasureRoll)
                {
                    case int r when r >= 1 && r <= 15:
                        room.SearchResults.Add("You found a secret door leading to a small _treasure chamber. Place tile R10 adjacent to the current tile and add a door as usual. Re-roll if tile is in use. Once the heroes leave the treasure chamber, the door closes up and the tile can be removed.");
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
                        room.SearchResults.Add("You found a set of levers. (Interaction handled by a LeverService)");
                        room.HasLevers = true; // Update room state
                        break;
                    case int r when r >= 46 && r <= 50:
                        room.SearchResults.Add(await _treasure.GetTreasureAsync("Coin", 0, 1, RandomHelper.GetRandomNumber(4, 40)));
                        break;
                    case int r when r >= 91 && r <= 100:
                        room.SearchResults.Add("You've sprung a trap!");
                        // A TrapService or DungeonManagerService would handle the trap instantiation/effect.
                        // You might set a flag here or return a Trap object.
                        // CurrentTrap = newTrap; // Example: if Trap is a simple data class.
                        break;
                    default:
                        room.SearchResults.Add("You found Nothing");
                        break;
                }
            }
            else
            {
                room.SearchResults.Add("Search Failed");
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
}
