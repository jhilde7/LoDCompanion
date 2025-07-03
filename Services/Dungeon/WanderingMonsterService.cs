using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Utilities;

namespace LoDCompanion.Services.Dungeon
{
    public class WanderingMonsterService
    {
        private readonly EncounterService _encounter;
        private readonly DungeonState _dungeonState;

        public WanderingMonsterService(DungeonState dungeonState, EncounterService encounter)
        {
            _dungeonState = dungeonState;
            _encounter = encounter;
        }

        /// <summary>
        /// Spawns a new wandering monster token at the dungeon's entrance.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        public void SpawnWanderingMonster(DungeonState dungeonState)
        {
            if (dungeonState.StartingRoom == null) return;

            var newWanderingMonster = new WanderingMonsterState(dungeonState.StartingRoom.RoomName);
            dungeonState.WanderingMonsters.Add(newWanderingMonster);

            Console.WriteLine("A wandering monster token has been placed at the dungeon entrance.");
        }

        /// <summary>
        /// Moves all active wandering monster tokens according to the rules.
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        /// <returns>True if any monster spotted the party.</returns>
        public bool MoveWanderingMonsters(DungeonState dungeonState)
        {
            bool partySpotted = false;
            foreach (var monsterState in dungeonState.WanderingMonsters)
            {
                if (monsterState.IsRevealed) continue; // Revealed monsters act in normal combat.

                // Simplified movement logic. A full implementation would need a grid and pathfinding.
                // For now, we simulate the d6 roll for direction.
                int moveRoll = RandomHelper.RollDie("D6");
                if (moveRoll >= 2)
                {
                    // Monster moves towards the party.
                    // TODO: Implement logic to move the token 4 squares towards the party's current room.
                    // This involves finding the next room in the path.
                    Console.WriteLine($"Wandering monster token moves towards the party...");

                    // After moving, check if it spots the party.
                    if (CheckForReveal(monsterState, dungeonState))
                    {
                        partySpotted = true;
                    }
                }
                else
                {
                    // Monster moves away from the party.
                    Console.WriteLine("Wandering monster token moves away from the party.");
                }
            }
            return partySpotted;
        }

        /// <summary>
        /// Checks if a wandering monster token should be revealed.
        /// </summary>
        private bool CheckForReveal(WanderingMonsterState monsterState, DungeonState dungeonState)
        {
            // Simplified reveal logic. Rule: "If it enters a room from where it has line of sight to
            // the characters and they are within 10 squares, roll on the quest-specific Monster Table".
            if (monsterState.CurrentRoomId == dungeonState.CurrentRoom?.RoomName)
            {
                Console.WriteLine("The wandering monster has found the party!");

                // TODO: Replace with a roll on the actual quest's encounter table.
                // For now, we'll just grab a random monster.
                var monsterTemplates = new Dictionary<string, Monster>();
                var weaponTemplates = new Dictionary<string, MonsterWeapon>();

                List<Monster> monsters = new List<Monster>();

                if (_dungeonState.Quest != null)
                {
                    monsters = _encounter.GetEncounters(_dungeonState.Quest.EncounterType, monsterTemplates, weaponTemplates);
                }
                else
                {
                    monsters = _encounter.GetEncounters(EncounterType.Beasts, monsterTemplates, weaponTemplates);
                }

                if (monsters.Count > 0)
                {
                    monsterState.RevealedMonster = monsters[0];
                    // TODO: Add the revealed monster to the current room's encounter list.
                    // dungeonState.CurrentRoom.Monsters.Add(monsterState.RevealedMonster);
                }
                return true;
            }
            return false;
        }
    }
}