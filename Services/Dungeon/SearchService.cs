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
        private readonly TreasureService _treasureService;

        public SearchService(TreasureService treasureService)
        {
            _treasureService = treasureService;
        }

        /// <summary>
        /// Performs a search of an entire room or corridor.
        /// </summary>
        /// <param name="primaryHero">The hero leading the search.</param>
        /// <param name="assistingHeroes">A list of other heroes helping with the search.</param>
        /// <param name="room">The room being searched.</param>
        /// <returns>A SearchResult object detailing the outcome.</returns>
        public SearchResult SearchRoom(Hero primaryHero, List<Hero> assistingHeroes, Room room)
        {
            var result = new SearchResult();
            if (room.HasBeenSearched)
            {
                result.Message = "This area has already been thoroughly searched.";
                return result;
            }

            int perceptionBonus = 0;
            if (assistingHeroes.Any())
            {
                perceptionBonus += 10; // +10 for the first helper
                perceptionBonus += (assistingHeroes.Count - 1) * 5; // +5 for each subsequent helper
            }

            int searchRoll = RandomHelper.RollDie("D100");
            if (searchRoll <= primaryHero.PerceptionSkill + perceptionBonus)
            {
                result.WasSuccessful = true;
                result.Message = "The party's thorough search paid off!";
                // The PDF on page 85 implies a general treasure roll. We'll use the TreasureService for this.
                result.FoundItems = _treasureService.FoundTreasure(TreasureType.Mundane, 1);
            }
            else
            {
                result.Message = "The party searches the area but finds nothing of interest.";
            }

            room.HasBeenSearched = true;
            return result;
        }

        /// <summary>
        /// Performs a search of a single piece of furniture.
        /// </summary>
        /// <param name="furniture">The furniture being searched.</param>
        /// <returns>A SearchResult object detailing the outcome.</returns>
        public SearchResult SearchFurniture(Furniture furniture)
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

            // As per PDF page 85, roll on the Furniture Treasure Table.
            // We'll use the TreasureService to handle this roll.
            result.WasSuccessful = true;
            result.Message = $"You search the {furniture.Name}...";
            result.FoundItems = _treasureService.FoundTreasure(TreasureType.Mundane, 1); // Assuming a mundane find

            furniture.HasBeenSearched = true;
            return result;
        }

        /// <summary>
        /// Performs a search of a defeated monster's corpse.
        /// </summary>
        /// <param name="hero">The hero searching the corpse.</param>
        /// <param name="corpse">The corpse to be searched.</param>
        /// <returns>A SearchResult object detailing the outcome.</returns>
        public SearchResult SearchCorpse(Hero hero, Corpse corpse)
        {
            var result = new SearchResult();
            if (corpse.HasBeenSearched)
            {
                result.Message = "This corpse has already been looted.";
                return result;
            }

            result.WasSuccessful = true;
            result.Message = $"{hero.Name} searches the remains...";
            result.FoundItems = _treasureService.SearchCorpse(corpse.TreasureType, hero, 0); // Roll is handled inside

            corpse.HasBeenSearched = true;
            return result;
        }
    }
}
