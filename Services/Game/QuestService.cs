using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.Dungeon;

namespace LoDCompanion.Services.Game
{
    public enum QuestType
    {
        Dungeon,
        WildernessQuest,
        WildernessEvent,
        DesertEvent,
        RoadsEvent
    }
    public class QuestService
    {
        private readonly DungeonManagerService _dungeonManager;
        private readonly RoomService _room;
        private readonly QuestSetupService _questSetup;
        private readonly CombatManagerService _combatManager;

        public Quest? ActiveQuest { get; private set; }
        public Room? ActiveEncounterRoom { get; private set; }
        public bool IsObjectiveComplete { get; private set; }
        public event Action? OnQuestStateChanged;

        public List<Quest> Quests => GetQuests();
        public bool IsQuestActive => ActiveQuest != null;

        public QuestService(
            DungeonManagerService dungeonManagerService,
            RoomService roomService,
            WorldStateService worldState,
            PlacementService placement,
            EncounterService encounter,
            QuestSetupService questSetup,
            CombatManagerService combatManagerService)
        {
            _room = roomService;
            _dungeonManager = dungeonManagerService;
            _questSetup = questSetup;
            _combatManager = combatManagerService;
        }

        /// <summary>
        /// Starts a new quest, setting it as the active one.
        /// </summary>
        /// <param name="quest">The quest to begin.</param>
        public void StartQuest(Party party, Quest quest)
        {
            ActiveQuest = quest;

            switch (quest.QuestType)
            {
                case QuestType.Dungeon:
                    // For a dungeon, it tells the DungeonManager to build it.
                    _dungeonManager.InitializeDungeon(party, ActiveQuest);
                    break;

                case QuestType.WildernessQuest:
                    // For a single encounter, create a room and have the QuestSetupService populate it.
                    ActiveEncounterRoom = new Room();
                    _questSetup.ExecuteRoomSetup(quest, ActiveEncounterRoom);

                    if (ActiveEncounterRoom != null && ActiveEncounterRoom.HeroesInRoom != null && ActiveEncounterRoom.MonstersInRoom != null)
                    {
                        // Check for any special quest rules that affect the start of combat.
                        bool hasSurpriseAttack = quest.SetupActions.First(q => q.ActionType == QuestSetupActionType.ModifyInitiative) != null; // Simple check for "First Blood"

                        // Tell the CombatManager to begin the fight with the characters in the room.
                        _combatManager.SetupCombat(
                        ActiveEncounterRoom.HeroesInRoom,
                        ActiveEncounterRoom.MonstersInRoom,
                        hasSurpriseAttack);
                    }
                    else
                    {
                        Console.WriteLine("Error: Cannot start combat. Room or characters not initialized.");
                    }
                    break;
            }

            OnQuestStateChanged?.Invoke();
        }

        /// <summary>
        /// Checks if the quest's objective has been met.
        /// This would be called after key game events (e.g., defeating a boss, finding an item).
        /// </summary>
        /// <param name="dungeonState">The current state of the dungeon.</param>
        public void CheckObjectiveCompletion(DungeonState dungeonState)
        {
            if (ActiveQuest == null || IsObjectiveComplete) return;

            // Example objective: Check if the party is in the objective room.
            if (dungeonState.CurrentRoom?.Name == ActiveQuest.ObjectiveRoom?.Name)
            {
                // A more complex quest might require a specific monster to be defeated
                // or an item to be in the party's inventory.
                Console.WriteLine("Quest objective completed!");
                IsObjectiveComplete = true;
            }
        }

        /// <summary>
        /// Grants the quest rewards to the party.
        /// </summary>
        /// <param name="party">The party to receive the rewards.</param>
        /// <returns>A string describing the rewards given.</returns>
        public string GrantRewards(Party party)
        {
            if (ActiveQuest == null || !IsObjectiveComplete)
            {
                return "Quest objective not yet complete. No rewards given.";
            }

            party.Coins += ActiveQuest.RewardCoin;
            // TODO: Add logic to grant special item rewards (ActiveQuest.RewardSpecial)

            var rewardMessage = $"Quest Complete! The party receives {ActiveQuest.RewardCoin} coins. {ActiveQuest.NarrativeAftermath}";

            // Reset the quest service for the next adventure.
            ActiveQuest = null;
            ActiveEncounterRoom = null;
            IsObjectiveComplete = false;

            return rewardMessage;
        }

        public List<Quest> GetQuests()
        {
            return new List<Quest>
            {
                new Quest()
                {
                    IsSideQuest = true,
                    Name = "The Missing Brother",
                    Location = QuestLocation.MainQuest,
                    SpecialRules = "Once the card pile is done, place one extra Room Card somewhere within reach, without looking at it. Next, add Side Quest 1 to the pile. When you draw this, replace it with the extra Room Card. This is the Side Quest Objective Room. If the main objective room is found before this one, the heroes must decide whether to go home or continue searching for the Side Quest Room.",
                    RewardCoin = 100,
                    EncounterType = EncounterType.TheMissingBrother,
                    ObjectiveRoom = _room.GetRandomRoom(),
                    NarrativeQuest = "Spending an evening at The Screaming Hog tavern, the heroes are discussing their upcoming quest and their preparations, when they are suddenly interrupted by a young lady. She introduces herself and begs for forgiveness for eavesdropping, and then goes on to explain them business. Apparently, them brother went to the same dungeon as the one that the heroes are heading for, but he did not return. She now wonders if the party is willing to investigate their disappearance, since they are heading there anyway.",
                    NarrativeObjectiveRoom = "When the heroes open the door, they realise there is already a fight going on. Some poor soul is desperately fighting several enemies.",
                    NarrativeSetup = "Place the lone fighter in one of the far corners. Then roll twice on the Encounter Table and place these as close to the survivor as possible. Any Archers are placed as far away from the fighter as possible, but not in the 2 squares next to the door where the heroes are. The heroes may continue their turn. The enemies will target only the heroes from now on.",
                    NarrativeAftermath = "Found them! Once the last enemy is dead, the heroes check on the fighter. He is slumped against the wall, breathing heavily and bleeding, but he is not heavily wounded. He introduces himself and it turns out he is the missing brother of the girl they met at the inn. He thanks the heroes for saving them and bids farewell as he limps towards the exit and the way back to the city. Once back in the city, the heroes receive their promised coins from a very grateful sister.\nDidn't find them! As soon as the heroes set foot in the tavern, they can see the sister looking at them nervously. As one of the heroes shakes their head, she covers them face with them hands and rushes out through the door."
                },
                new Quest()
                {
                    IsSideQuest = true,
                    Name = "Slay the beast!",
                    Location = QuestLocation.MainQuest,
                    SpecialRules = "Add Side Quest 1 card to the pile. When you draw this, put it aside and pull the next card. This card will now lead you to the Objective Room. There is one more door than normal in that room. That door will lead to the Side Quest Objective Room.",
                    RewardSpecial = "Bet dependent on monster: Giant spider (300c), Cave Monster (400c), Common Troll (500c), Minotaur (600c), Gigantic Snake (700c), Gigantic Spider (800c).",
                    EncounterType = EncounterType.SlayTheBeast,
                    ObjectiveRoom = _room.GetRoomByName("R17"),
                    NarrativeQuest = "Feeling ready for a change of pace, the heroes decide to go to the Flying Wheel Tavern instead of their usual waterhole...they are comparing their deeds with another set of adventurers. One thing leads to another and suddenly the party is facing a challenge: 'I'd say these fellas are nothing more than loudmouths! Prove me wrong and I'll give you coins. If you fail, you either pay with your lives to the beast, or by coins to me!' The beast in question happens to be in the same dungeon the party is heading to.",
                    NarrativeObjectiveRoom = "The heroes enter a dimly lit room. Judging from the smell, this room is home to something way more sinister than most of the other rooms in this dungeon.",
                    NarrativeSetup = "Place the R17 tile. The monster is in the far end of the room.",
                    NarrativeAftermath = "Slayed the Beast! The heroes walk triumphantly into the tavern, quickly locate the other party, and slam down the decapitated head of the beast in the middle of the table...the man who challenged you reaches inside their jacket and takes out a bag of coins. Failed to Slay the Beast! The heroes try to enter the tavern without drawing too much attention...The man behind the challenge makes a big scene...There is nothing to do but to pay up."
                },
                new Quest()
                {
                    IsSideQuest = true,
                    Name = "The Mapmaker",
                    Location = QuestLocation.MainQuest,
                    SpecialRules = "The party must keep the map maker alive in the journey from the city, through the dungeon, and back again. He should be represented with a model just like the heroes, and the players may move them in the way they see fit. He will always strive to be within 2 squares of a hero, as that makes them feel safe. He may not fight or interact with the enemy, although he may try to dodge as usual.",
                    RewardCoin = 500,
                    EncounterType = EncounterType.MainQuest,
                    NarrativeQuest = "One of the heroes happens upon a note on the bulletin board at the market. Apparently, there is a mapmaker seeking escort to the very dungeon to which you are heading. Why on earth he plans to map the dungeon is beyond your understanding, but the large bag of coins he offers is quite easy to understand.",
                },
                new Quest()
                {
                    IsSideQuest = true,
                    Name = "Go Fetch!",
                    Location = QuestLocation.MainQuest,
                    SpecialRules = "Make sure that R9 makes it into the pile of Exploration Cards. If you manage to find R9, you have also found the Objective Room for this side quest.",
                    RewardCoin = 350,
                    RewardSpecial = "If found, the heroes may use the standard Heater Shield (DEF 6, ENC 4).",
                    EncounterType = EncounterType.MainQuest,
                    ObjectiveRoom = _room.GetRoomByName("R9"),
                    NarrativeQuest = "The party has been contacted by another adventurer who is in desperate need of their help. He was quite recently on a quest in the very dungeon to which you are heading, but had to make a hasty retreat. Wounded and outnumbered he managed to leave the dungeon. However, to make it out he had to leave their beloved shield behind: 'If the Gods are good, it's still resting up against that fountain!'",
                    NarrativeObjectiveRoom = "You clearly recognise the fountain from the adventurer's description. Before you see if the shield is there, you notice that you are not alone. There is an automatic Encounter in this room. Roll on the quest-specific Encounter Table. Once the fight is over, you can try to locate the shield.",
                    NarrativeAftermath = "Back with the Shield: The adventurer is overjoyed to see you, and especially the old shield. He quickly hands the heroes their agreed reward and limps away. Shield is Broken or was Not Found: He sighs, lowers their head and walks away. No shield - No money."
                },
                new Quest()
                {
                    IsSideQuest = true,
                    Name = "Manhunt",
                    Location = QuestLocation.MainQuest,
                    SpecialRules = "Once the dungeon deck is done, take the same number of cards from the Dungeoneers Deck in a separate pile. Choose one specific card that represents the bandit and then mix that deck and place it next to the ordinary deck. Once you open a door, draw one card from this new deck. If you draw the card representing the bandit, you have found them. Roll for Encounters as usual, then place the bandit in the far end of the room.",
                    RewardCoin = 250,
                    EncounterType = EncounterType.MainQuest,
                    NarrativeQuest = "Apparently, one of the city guards was killed last night, and the streets are swarming with armed soldiers. Posters with sketches of the perpetrator have been posted everywhere. Apparently the man is missing both an eye and an ear, so he should be easy to identify. It seems anyone who kills the bandit, and can prove it, will be handsomely rewarded. Unbeknownst to the heroes, the bandit has taken refuge in the very same dungeon to which they are heading.",
                    NarrativeAftermath = "If the heroes manage to kill the bandit, they get the reward once they are back in the city."
                },
                new Quest()
                {
                    IsSideQuest = true,
                    Name = "Mushrooms",
                    Location = QuestLocation.MainQuest,
                    SpecialRules = "Once the Dungeon Deck is done, take the same number of cards from the Dungeoneers Deck in a separate deck. Choose one specific card that represents the Side Quest Objective and then mix that deck and place it next to the ordinary deck. Once you search a room or corridor, draw one card from this new deck. If the card you draw is the Side Quest Objective Card, you have found the mushrooms.",
                    RewardCoin = 250,
                    RewardSpecial = "A potent healing potion that heals 2d6 HP.",
                    EncounterType = EncounterType.MainQuest,
                    NarrativeQuest = "According to an alchemist you have happened upon, there is a mushroom that has very special properties. This mushroom could apparently revolutionise the brewing of healing potions. The mushroom only grows indoors in dark, damp areas and the dungeon you are heading to fits the bill perfectly. If you could bring a specimen back, he would reward you generously.",
                    NarrativeAftermath = "Found it! The Alchemist is very satisfied, and happily pays the money he promised...A day later a messenger boy catches up with you and hands you a package with a carefully wrapped potion. There is a note from the Alchemist as well, saying that this is their new healing potion. The potion is actually very potent and heals 2d6 HP once consumed."
                },
                new Quest()
                {
                    Name = "First Blood",
                    QuestType = QuestType.WildernessQuest,
                    StartingRoom = _room.GetRoomByName("Barren Land"),
                    SpecialRules = "The bandits gain +2 initiative tokens on the first turn as they surprise the sleeping heroes. Due to the darkness, no one can see or shoot further than 10 squares. The Scenario die is not in use.",
                    EncounterType = EncounterType.Bandits_Brigands,
                    NarrativeQuest = "The heroes have travelled for days and are finally nearing their destination in the village...the hero on watch is suddenly snapped into high alert by the faint sound of stealthy footsteps. Realising the potential peril, he quickly tries to wake the rest of the party. As they fumble to rise, a gang of bandits charge out of the darkness.",
                    NarrativeSetup = "The party has been attacked by three bandits and one bandit leader. The bandit leader is equipped with a longsword, a shield and has armour 1. Two bandits are armed with shortswords and have armour 0. One bandit is armed with a shortbow, dagger and has armour 0. Use the wilderness outdoor tiles (open ground tiles) and place the heroes in the centre, adjacent to each other. Then randomise which board edge each bandit approaches from, placing them 1d10 squares from the edge.",
                    NarrativeAftermath = "With the bandits dead, the party decides to gather their gear and continue their travel. They arrive at the settlement without further issues. Head to the settlement chapter and start by rolling for a settlement event.",
                    SetupActions = new List<QuestSetupAction>()
                    {
                        new QuestSetupAction
                        {
                            // A new ActionType for initiative changes.
                            ActionType = QuestSetupActionType.ModifyInitiative,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "Target", "Monster" }, // Or "Enemy"
                                { "Amount", "+2" },
                                { "Turn", "1" } // Specifies it only applies to the first turn
                            }
                        },
                        new QuestSetupAction
                        {
                            // This handles the darkness rule.
                            ActionType = QuestSetupActionType.SetCombatRule,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "Rule", "MaxVision" },
                                { "Value", "10" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SetRoom,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "RoomName", "Barren Land" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SetTurnOrder,
                            Parameters = new Dictionary<string, string>()
                            {
                                // The narrative implies the bandits act first due to the surprise.
                                { "First", "Enemies" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.PlaceHeroes,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "PlacementRule", "Center" },
                                { "Arrangement", "Adjacent" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SpawnMonster,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "Name", "Bandit Leader" },
                                { "Count", "1" },
                                { "Weapons", "Longsword" },
                                { "Shield", "true" },
                                { "Armour", "1" },
                                { "PlacementRule", "RandomEdge" },
                                { "PlacementArgs", "1d10" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SpawnMonster,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "Name", "Bandit" },
                                { "Count", "2" },
                                { "Weapons", "Shortsword" },
                                { "Armour", "0" },
                                { "PlacementRule", "RandomEdge" },
                                { "PlacementArgs", "1d10" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SpawnMonster,
                            Parameters = new Dictionary<string, string>()
                            {
                                { "Name", "Bandit Archer" },
                                { "Count", "1" },
                                { "Weapons", "Shortbow,Dagger" },
                                { "Armour", "0" },
                                { "PlacementRule", "RandomEdge" },
                                { "PlacementArgs", "1d10" }
                            }
                        }
                    }
                },
                new Quest()
                {
                    Name = "Spring Cleaning",
                    Location = QuestLocation.CurrentTown,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 12, a Wandering Monster will appear regardless of the Scenario dice result. For this quest only, the party may gain the benefits of the 'Rest' rule (p. 86) without consuming any food rations.",
                    CorridorCount = 4,
                    RoomCount = 4,
                    RewardCoin = 100,
                    RewardSpecial = "50 c extra per hero as gratitude for finding Johann. 10 rations to use on the road.",
                    EncounterType = EncounterType.SpringCleaning,
                    ObjectiveRoom = _room.GetRoomByName("R5B"),
                    StartThreatLevel = 2,
                    MinThreatLevel = 2,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "You have been tasked by the local Town Elder to clear out the basement of the Town Hall. This basement has been closed off for several years, but a recent invasion of large rats in the village has led the local rat catcher to think that their nest is in this building.",
                    NarrativeObjectiveRoom = "The heroes enter what seems to be a large storage room. The room is filled with huge piles of crates and barrels and everywhere they look, the heroes can see the beady red eyes of large rodents staring at them malevolently. In the far back, a huge creature can be seen. Much larger than the rest, and heavily deformed, it is questionable whether this rat is living or Undead.",
                    NarrativeSetup = "The party enters a large dusty room filled with old crates and barrels. There are 1d10 Rats in the room (placed randomly), and as far away as possible from the player is the Brood Mother. If the party has not encountered Johann yet, he will also be in the far end of the room.",
                    NarrativeAftermath = "The Town Elder is pleased with the hero's efforts. The news of Johann came as a bit of a shock though, and he seems troubled. Before parting, the party is offered 50 c extra per hero as gratitude for finding Johann. The Town Elder recommends the party to head to Silver City. To further help the party on their journey, he offers them 10 rations to use on the road."
                },
                new Quest()
                {
                    Name = "The Dead Rising",
                    Location = QuestLocation.SilverCity,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 10, a Wandering Monster will appear regardless of the Scenario dice result.",
                    CorridorCount = 5,
                    RoomCount = 5,
                    RewardCoin = 150,
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Great Crypt"),
                    StartThreatLevel = 4,
                    MinThreatLevel = 4,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "The Jarl of Silver City reaches out to the party, requesting their help: 'My cousin told me you proved you could handle yourself...Would you consider heading down the Mausoleum to get in contact with Ulfric?'...Accepting the quest, the adventures are now tasked with making contact with Ulfric and their Brothers.",
                    NarrativeObjectiveRoom = "The Crypt that the heroes enter is a large room with six sarcophagi. In the middle stands Ulfric, unmistakably changed. Flanking them are two skeletons, both armed with bronze swords and shields. As he notices the party, he lunges forward, with their Hammer held high.",
                    NarrativeSetup = "Place Ulfric in the middle and the two skeletons on either side of them. They are considered ordinary skeletons with longswords and shields.",
                    NarrativeAftermath = "The Jarl listens carefully to the story told by the adventurers...he appraises their skills and begs them not to spread word of what happened...He also promises to put in a good word for the party, should they ever need it."
                },
                new Quest()
                {
                    Name = "Highwaymen",
                    Location = QuestLocation.White34,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 10, a Wandering Monster will appear regardless of the Scenario dice result.",
                    CorridorCount = 6,
                    RoomCount = 6,
                    RewardCoin = 200,
                    EncounterType = EncounterType.Bandits_Brigands,
                    ObjectiveRoom = _room.GetRoomByName("The Throne Room"),
                    StartThreatLevel = 3,
                    MinThreatLevel = 3,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "A deal is settled. The party will deal with the bandits, in exchange for coins. Digging around for more information...it becomes clear that all attacks have happened close to the old ruined fort...all wagons were completely smashed up...no trace has been found of the wagons' crew.",
                    NarrativeObjectiveRoom = "The party enters a room which seems to be the former great hall of the Commandant at the fort. Close to the throne stands the leader of the bandits together with their bodyguards. It suddenly dawns on the adventurers what caused the carts to be totally demolished. The bandit leader is no ordinary man, but a towering, muscular Ogre. Spotting the adventurers, he points a chubby finger at them and bellows for their guards to attack.",
                    NarrativeSetup = "Place Graup, the Ogre, by the throne. Roll twice on the Bandits and Brigands Table and randomly place them around the Ogre, as close as possible. The heroes enter along the short side of the room. Bandits move first. Graup is an ordinary Ogre and is armed with a longsword and has Armour 1.",
                    NarrativeAftermath = "Taking a look around the room the party notices two chests filled with loot from the bandits' plunder. There is also a thick ledger, detailing the plunder...Curiously, the last raids have listed the wagons crew as plunder as well. It appears that their bodies have been sold for coin...Taking what they can, the heroes start their journey back to the city.",
                    SetupActions = new List<QuestSetupAction>()
                    {
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SetRoom,
                            Parameters = new Dictionary<string, string>() { { "RoomName", "The Throne Room" } }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SetTurnOrder,
                            Parameters = new Dictionary<string, string>() { { "First", "Enemies" } }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.PlaceHeroes,
                            Parameters = new Dictionary<string, string>() { { "Rule", "ShortSide" } }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SpawnMonster,
                            Parameters = new Dictionary<string, string>() {
                                { "Name", "Graup" },
                                { "BaseMonster", "Ogre" },
                                { "Count", "1" },
                                { "Equipment", "Longsword" },
                                { "Armour", "1" },
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", "Throne" }
                            }
                        },
                        new QuestSetupAction
                        {
                            ActionType = QuestSetupActionType.SpawnFromChart,
                            Parameters = new Dictionary<string, string>() {
                                { "ChartName", "BanditsAndBrigands" },
                                { "Rolls", "2" },
                                { "PlacementRule", "RelativeToTarget" },
                                { "PlacementTarget", "Graup" },
                                { "PlacementArgs", "AsCloseAsPossible" }
                            }
                        }
                    }
                },
                new Quest()
                {
                    Name = "The Burning Village",
                    QuestType = QuestType.WildernessQuest,
                    Location = QuestLocation.OutsideRochdale,
                    StartingRoom = _room.GetRoomByName("Field With Trees"),
                    SpecialRules = "The heroes are allowed one rest before the battle, but no roll on the Travel Events Table is necessary. No Threat Level is used during this quest. The Scenario dice should still be rolled and a result of 9-0 triggers reinforcements for the Goblinoids. In that case, roll once more on the OaG Table and place them cantered along a random table edge.",
                    RewardSpecial = "If the heroes win, they receive 3 random potions and 1 Fine Treasure. If they flee, there is no reward.",
                    EncounterType = EncounterType.Orcs_Goblins,
                    NarrativeQuest = "After about half an hour walking through difficult terrain, the forest abruptly gives way to open fields, and just a short distance ahead lies Rochdale. One of the small houses on the outskirts is ablaze and violent screams are emanating from the village. The adventurers rush across the field as they realize that the village is beset by a band of Orcs and Goblins.",
                    NarrativeSetup = "Set up the outdoor tiles. Roll twice on the Orcs and Goblins Encounter Chart and randomise their placement along 1 board edge. Place 1 Orc Chieftain in the same way. Victory Condition: If at any time, there are no living enemies on the table, the heroes win. If at any time the heroes feel like running away, they can do so by simply leaving the map, in which case the quest is over.",
                    NarrativeAftermath = "If The Heroes Won: As the last Orc falls to the ground the villagers start to emerge from their houses. The Village Elder approaches the heroes, praising their efforts and offers the party a small token of their gratitude. Rummaging through the belongings of the chieftain, the party discovers yet another note on payment for bodies signed 'Imgrahil'.\nIf The Heroes Fled: Realising they were in for more than they could handle, the heroes decide it is better to live another day and head back to the forest. The orcs follow a short distance before they turn back to set the rest of the village ablaze."
                },
                new Quest()
                {
                    Name = "The Apprentice",
                    Location = QuestLocation.White22,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 10, a Wandering Monster will appear regardless of the Scenario dice result. Any Encounter Table roll of 15-20 results in an encounter with the caretaker, plus the ordinary encounter. The caretaker, Emil, wields a sharpened shovel (Greataxe) and drops a bronze key upon death.",
                    CorridorCount = 6,
                    RoomCount = 6,
                    RewardCoin = 200,
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Chamber of Reverence"),
                    StartThreatLevel = 4,
                    MinThreatLevel = 4,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "Once back in the city, the adventurers are summoned by the Jarl. He explains that there have been more incidents at the cemeteries, pointing towards a Necromancer. He asks the heroes to investigate ruins east of the city to find and kill whoever is collecting corpses.",
                    NarrativeObjectiveRoom = "The heroes enter a huge chamber. In the middle stands a man, Imgrahil, performing an incantation over the body of a peasant girl lying in a magical circle. The heroes watch as the girl starts to twitch and stand up. As they advance, more bodies appear out of the darkness to stop them.",
                    NarrativeSetup = "Place Imgrahil, the Apprentice, in the centre of the circle. Place one unarmed Zombie next to them. Place 2d6 unarmed Zombies along the long walls, as far as possible from the heroes. The heroes are placed close to the door. Imgrahil is an apprentice Necromancer armed with a poisoned dagger, Armour 1, and has mastered Raise Dead, Healing, Vampiric Touch, and Mirrored Self.",
                    NarrativeAftermath = "Once Imgrahil is dead, the heroes find a notebook in their pockets. It mentions a 'Master' and an 'Apostle', and a growing army of the Undead. It also mentions a sacrifice to a being named Melkhior. A map reveals the locations of the Apostle and the Master, forcing the heroes to choose their next target. If they go for the Apostle, play quest 6A. If they go for the Master, play 6B."
                },
                new Quest()
                {
                    Name = "Sacrifice",
                    Location = QuestLocation.White36,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    CorridorCount = 7,
                    RoomCount = 7,
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Lava River"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "Deciding to intervene with the sacrifice, the heroes head for the spot where the Apostle is said to be. They arrive at an old, ruined tower and find a trapdoor leading down into the darkness, which they decide to descend.",
                    NarrativeObjectiveRoom = "Upon entering the large hall, the heroes spot a group of civilians on the bank of a lava river, surrounded by skeletons. Further back, on a dais, stands the Apostle, them eyes fixed on the heroes.",
                    NarrativeSetup = "The heroes enter along the short side opposite the altar. Place 4 Wights on their side of the river. Place the Apostle on the dais. The heroes go first. There are 2 unlocked, non-trapped objective chests next to the altar. The Apostle is a Vampire Fledgling armed with a longsword and Armour 2.",
                    NarrativeAftermath = "After the heroes liberate the civilians, they send them on their way and head straight towards the Master themselves."
                },
                new Quest()
                {
                    Name = "The Master",
                    Location = QuestLocation.White39,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "The Scenario dice triggers an event on a roll of 8-0. If the heroes chose to stop the sacrifice in Quest 6A, roll twice on the Encounter Table for every encounter, choosing the higher result. The middle chest in room R10 is locked and can only be opened with the bronze key from Quest 5; it contains the Vanquisher.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 2000,
                    RewardSpecial = "The Vanquisher (magical longsword, +2 damage to Undead). A grimoire with one random spell. The Master's engraved dagger.",
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Great Crypt"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "After a couple of rainy days on the road, the heroes dive into the ruins of a forgotten castle where the map indicates they can find the Master.",
                    NarrativeObjectiveRoom = "The heroes enter the room where the Master resides. At the far end, a figure in tattered robes with a hidden face stands flanked by two lumbering Zombie Ogres. After a moment, the Master gestures for their thralls to attack and begins their incantations.",
                    NarrativeSetup = "The heroes enter along a short side. Place the two Ogres and the Master at the far end of the room. The heroes go first. There are 6 tombs in the room that can be looted. The Master is a Necromancer with a poisoned dagger, Armour 1, and knows Raise Dead, 3 Close-combat Spells, and 4 Ranged Spells.",
                    NarrativeAftermath = "As the Master falls, their body disappears, leaving behind their robes, a grimoire, and a dagger. After looting the crypt, the party heads back to Silver City. The Jarl greets them, commends their actions, and gives them their reward. He also informs them that High King Logan III has requested their enlistment into the League of Dungeoneers to head into the Ancient Lands."
                },
                new Quest()
                {
                    Name = "Lair of the Spider Queen",
                    Location = QuestLocation.White40,
                    RewardCoin = 1200,
                    NarrativeQuest = "The party has been contacted by an elderly wizard who tells them the story of Queen Araneae, who controlled spiders with a magical sceptre. He believes the sceptre is buried with them in a tomb north of the Ancient Lands and will reward the party handsomely for retrieving it. To begin the quest, travel to location White 40."
                },
                new Quest()
                {
                    Name = "Level 1: The Entrance",
                    Location = QuestLocation.White40,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "The Secondary Quest card 1 should be mixed in with the first half of the pile.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    EncounterType = EncounterType.Orcs_Goblins,
                    ObjectiveRoom = _room.GetRoomByName("The Chamber of Reverence"),
                    StartThreatLevel = 4,
                    MinThreatLevel = 4,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "Upon arrival at the temple ruins, the adventurers find it occupied. Sharpened poles with severed heads warn them to stay out. They must find a hidden passageway leading deeper underground.",
                    NarrativeObjectiveRoom = "The large chamber appears to have been the inner sanctum of the temple. Orcs are kneeling around a large, decayed statue of some forgotten deity.",
                    NarrativeSetup = "The heroes enter along the short side opposite the statue. Five Orcs and one Orc Chieftain are two squares away from the statue with their backs to the heroes. The heroes may continue their turn. Chieftain Grotto is armed with a Greataxe and Armour 3. The Orcs have Battlehammers, Shields, and Armour 1.",
                    NarrativeAftermath = "Once the chamber is clear, the party finds a small imprint at the base of the statue that matches the spider amulet found on Amburr the Ettin. Pressing the amulet into the imprint reveals a dusty stair leading down."
                },
                new Quest()
                {
                    Name = "Level 2: The Basement",
                    Location = QuestLocation.White40,
                    StartingRoom = _room.GetRoomByName("C18"),
                    SpecialRules = "The Secondary Quest card 1 should be mixed in with the stack before dividing it to add the normal objective card. A Wandering Monster will appear when the Threat Level reaches 10 and 16. The secondary objective involves fighting Kraghul the Mighty, a powerful Minotaur.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardSpecial = "If the secondary objective is completed, the party finds two Antidote potions.",
                    EncounterType = EncounterType.Beasts,
                    ObjectiveRoom = _room.GetRoomByName("C16"),
                    StartThreatLevel = 4,
                    MinThreatLevel = 4,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "The adventurers continue down the stairs into the temple's basement. There is a pungent smell in the air and the stairs are covered in dust. In the distance they suddenly hear a terrifying roar, alerting them to a dangerous presence.",
                    NarrativeObjectiveRoom = "Finally, the heroes locate the stairs. They carefully approach them, expecting traps or monsters, but they safely reach the top. The darkness prevents any possibility to spy what lies ahead. Instead, they carefully start their descent further into the bowels of the temple.",
                    NarrativeAftermath = "Upon reaching the stairs, the heroes proceed to the next level."
                },
                new Quest()
                {
                    Name = "Level 3: The Tomb of the Spider Queen",
                    Location = QuestLocation.White40,
                    StartingRoom = _room.GetRoomByName("C8"),
                    SpecialRules = "Whenever you have an encounter, roll a die. An odd number will result in 1d3 Giant Spiders; an even number will result in an encounter from the Undead Encounter List. All doors are considered cobweb covered openings. A Threat Level of 12 will trigger a Wandering Monster.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardSpecial = "4d100 c and 3 Wonderful Treasures found in the sarcophagus.",
                    EncounterType = EncounterType.TheTombOfTheSpiderQueen,
                    ObjectiveRoom = _room.GetRoomByName("The Great Crypt"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "Reaching the end of the stairs, the party finds the corridor ending in an opening covered in a tight weave of cobweb. The dust on the floor has been disturbed by a strange pattern, suggesting the Queen's power over spiders transgresses the borders between life and death.",
                    NarrativeObjectiveRoom = "The heroes reach the final resting place of the Queen. Sunbeams shoot from holes in the roof, creating a spotlight on a sarcophagus. Standing next to it is a huge, motionless black spider. Suddenly, its eight eyes gleam and it rises as if woken from slumber.",
                    NarrativeSetup = "The Gigantic Spider, Belua, is placed next to the sarcophagus. The heroes enter on the short side of the room. Belua has a special ability: on a behavior roll of 5-6, she attempts to summon a Giant Spider, which appears randomly in the room but cannot act on the turn it arrives.",
                    NarrativeAftermath = "With Belua and them children slain, the heroes examine the sarcophagus. Inside lies the dried husk of Queen Araneae, clutching a long black sceptre. Prying it free, they also find piles of gold and valuables. After looting, their trek back to the surface is eventless."
                },
                new Quest()
                {
                    Name = "Stop the Heretics",
                    Location = QuestLocation.Random,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 10, a Wandering Monster will appear. In the Objective Room, the heroes have 10 turns to kill the caster. If they fail, the ritual succeeds, the caster collapses, and demons are summoned.",
                    CorridorCount = 6,
                    RoomCount = 6,
                    RewardCoin = 250,
                    EncounterType = EncounterType.StopTheHeretics,
                    ObjectiveRoom = _room.GetRoomByName("The Lava River"),
                    StartThreatLevel = 4,
                    MinThreatLevel = 4,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "A High Wizard has felt disturbances from the Void and determined someone is performing a Ritual of Summoning. If the heroes can interrupt the ritual, a demonic invasion can be prevented. They must enter the dungeon and slay the heretic conjurer.",
                    NarrativeObjectiveRoom = "The heat from the lava river is almost unbearable. Several guards are in the room, protecting a caster who is busy performing their ritual at an altar on a dais in the far end of the room.",
                    NarrativeSetup = "The party enters on the short side opposite the altar. Roll twice on the Encounter Table to place guards on the heroes' side of the river. On the other side stands a Magic User of the encountered race, who is so occupied with the ritual that he doesn't notice the battle. The heroes act first.",
                    NarrativeAftermath = "Once all creatures are defeated, the heroes find an alternative exit. If they kill the Spellcaster before he opens the portal, they get the full reward. If they fail, they only get half, as the portal is now open."
                },
                new Quest()
                {
                    Name = "The Master Alchemist",
                    Location = QuestLocation.Random,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 12, a Wandering Monster will appear. To complete the objective, a hero must spend 2 actions next to the lava river and pass a DEX test to fill a special vial.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 300,
                    EncounterType = EncounterType.TheMasterAlchemist,
                    ObjectiveRoom = _room.GetRoomByName("The Lava River"),
                    StartThreatLevel = 5,
                    MinThreatLevel = 3,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "The settlement's leading alchemist needs liquid lava for a new formula. He has constructed a special vial that will keep the lava from cooling and needs the heroes to venture into a monster-infested dungeon to retrieve it.",
                    NarrativeObjectiveRoom = "With the lava river in view, the heroes prepare to advance, but they are not alone in the chamber.",
                    NarrativeSetup = "Roll twice on the Encounter Table. Place the first result on the same side of the river as the heroes and the second on the far side. The heroes enter along the short side and may act first.",
                    NarrativeAftermath = "Once the room is clear, the heroes can leave via an alternative exit. If they return with the vial of lava, they receive their reward. If the vial was dropped, they receive nothing but a scolding from the alchemist."
                },
                new Quest()
                {
                    Name = "Preventing a Disaster",
                    Location = QuestLocation.SilverCity,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "When the Threat Level reaches 12, a Wandering Monster will appear. Ignore any special rules for the dead members of the Brotherhood; treat them as searchable dead adventurers. The objective room has tremors: if the Scenario Dice is 1-2, each hero must pass a DEX test or be toppled.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 300,
                    RewardSpecial = "If both scrolls fail, the heroes receive no coin but are granted +1 luck on their next quest by the god Rhidnir.",
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Lava River"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 5,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "The lava river beneath the temple of Rhidnir is causing tremors, threatening to flood the temple. To stop it, a powerful incantation from a scroll must be read at an old shrine by the river. The priests have hired the heroes to brave the Undead-infested burial grounds below the temple and perform the ritual.",
                    NarrativeObjectiveRoom = "The heroes enter a room filled with mindlessly walking Undead. As they brace against the tremors, they slowly advance towards the altar on the far side.",
                    NarrativeSetup = "Roll 3 times on the Undead Encounter Table and place the enemies randomly in the room. The heroes may act first.",
                    NarrativeAftermath = "To stop the tremors, a hero must successfully cast the incantation from one of two scrolls while adjacent to the altar. If successful, the party can escape and claim their reward. If both scrolls fail, the river floods, and the party has 3 turns to escape the chamber before taking fire damage. The priests will refuse to pay, but the god Rhidnir grants the party a boon for the spectacle."
                },
                new Quest()
                {
                    Name = "Rescuing the Prisoners",
                    Location = QuestLocation.Random,
                    SpecialRules = "When the Threat Level reaches 14, a Wandering Monster appears. An encounter roll of 17-18 means you have met Briggo the ogre, and 19-20 means you have met Gorm the ogre bodyguard. Every time the Scenario dice is 1-3, one of the 10 prisoners is killed.",
                    CorridorCount = 8,
                    RoomCount = 8,
                    RewardCoin = 250,
                    RewardSpecial = "The final reward is variable. If all 10 prisoners survive, the Jarl adds 100c to each hero's reward. For each prisoner that dies, 50c is deducted from a base of 300c per hero.",
                    EncounterType = EncounterType.Bandits_Brigands,
                    ObjectiveRoom = _room.GetRoomByName("Bandits Hideout"),
                    StartThreatLevel = 5,
                    MinThreatLevel = 5,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "A caravan has been ambushed and its passengers kidnapped by a local bandit menace known as Golfrid the Short. The Jarl has turned to their trusted heroes to find the bandits' lair and rescue the prisoners.",
                    NarrativeObjectiveRoom = "The heroes locate the hideout and find the Halfling, Golfrid, standing by a bed at the far end of the room. The kidnapped prisoners are sitting along one of the walls. Golfrid orders their bandits to attack, and in the ensuing chaos, the panicked prisoners start running around the room.",
                    NarrativeSetup = "Place Golfrid by the bed, with their ogre bodyguards next to them if they haven't been defeated yet. Roll twice on the Encounter Table and place the bandits evenly along the walls. The heroes may act first.",
                    NarrativeAftermath = "Once the battle is over, the heroes escort any surviving prisoners back to the city. The Jarl adjusts the final reward based on the number of survivors."
                },
                new Quest()
                {
                    Name = "The Pleasure House",
                    Location = QuestLocation.SilverCity,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "The Scenario dice is triggered on a result of 8-0.",
                    CorridorCount = 8,
                    RoomCount = 6,
                    RewardCoin = 250,
                    RewardSpecial = "During the aftermath, the party can make a deal with the High Priest's daughter for an extra 400c. Success is determined by a dice roll, with outcomes ranging from getting the extra coin to losing the main reward and suffering a temporary HP penalty.",
                    EncounterType = EncounterType.Bandits_Brigands,
                    ObjectiveRoom = _room.GetRoomByName("Bandits Hideout"),
                    StartThreatLevel = 5,
                    MinThreatLevel = 5,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "The High Priests have learned of an illegal Pleasure House operating in the catacombs below the city, run by the beautiful but cruel brigand, Madame Isabelle. Unwilling to trust the city guard, the priests hire the adventurers to shut down the establishment.",
                    NarrativeObjectiveRoom = "When the heroes enter the Pleasure House, the room, filled with patrons and guards, falls silent. Madame Isabelle sits at the far end, enjoying a glass of wine. After a moment, them guards rush towards the intruders.",
                    NarrativeSetup = "Roll twice on the Bandits and Brigands Encounter Chart and place the guards along the walls, with any archers at the far end. Place Madame Isabelle by the bed. The enemy acts first. Madame Isabelle has the Seduction special rule.",
                    NarrativeAftermath = "After defeating the guards and Isabelle, the heroes tend to the wounded. They recognize the High Priest's daughter among the visitors, who begs them not to reveal them presence and offers a hefty bribe for their silence."
                },
                new Quest()
                {
                    Name = "Cleansing the Water",
                    Location = QuestLocation.CurrentTown,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "A Wandering Monster will appear when the Threat Level reaches 14.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 300,
                    RewardSpecial = "A Potion of Healing for each hero.",
                    EncounterType = EncounterType.Reptiles,
                    ObjectiveRoom = _room.GetRoomByName("The Fountain Room"),
                    StartThreatLevel = 2,
                    MinThreatLevel = 2,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "The sacred healing water of the Temple of Meredith has recently turned into a foul, muddy green liquid. The High Priest begs the adventurers to investigate the source of the problem in the temple's basement, where the fountain originates.",
                    NarrativeObjectiveRoom = "The room is filled with the hissing of reptiles. Next to the fountain, two unhealthy-looking lizards are pouring a substance from a large vase into the water. The foul smell in the room makes every breath a struggle.",
                    NarrativeSetup = "Roll three times on the Reptile Table and place them randomly in the chamber. Place two Gecko Assassins (armed with shortswords, Armour 0) close to the fountain. The heroes enter along a short side and act first.",
                    NarrativeAftermath = "Once all reptiles are dead, the heroes see the water slowly returning to its normal color. Back in the temple, the grateful High Priest gives them their reward and a healing potion for each hero."
                },
                new Quest()
                {
                    Name = "Baptising",
                    Location = QuestLocation.Random,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 300,
                    RewardSpecial = "Two unlocked, untrapped objective chests are found in the chamber. The heroes may also dip their weapons into the fountain, giving them +1 Damage until the end of the next quest.",
                    EncounterType = EncounterType.Orcs_Goblins,
                    ObjectiveRoom = _room.GetRoomByName("The Fountain Room"),
                    StartThreatLevel = 4,
                    MinThreatLevel = 4,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "The heroes have been tasked by the local garrison to retrieve sacred water from a fountain of Ramos to perform an ancient weapon-baptising ritual. The closest fountain, however, is located in an abandoned castle overrun by a tribe of Orcs and Goblins.",
                    NarrativeObjectiveRoom = "The light from the glittering fountain illuminates the chamber. The heroes are not the only ones aware of the fountain's power; a huge orc is just about to dip their sword into the water when he spots them.",
                    NarrativeSetup = "Roll twice on the Orcs and Goblins Table and place them randomly in the chamber. Place Gaul the Mauler, a huge Orc, next to the fountain on the far side. Before the fight, Gaul dips their weapon, giving it +1 damage. The heroes start on a short side and act first.",
                    NarrativeAftermath = "After the fight, the heroes investigate the chamber and find two large chests. They take turns dipping their own weapons in the fountain before filling a bottle for the garrison and heading back to Silver City."
                },
                new Quest()
                {
                    Name = "Returning the Relic",
                    Location = QuestLocation.Random,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "Due to the curse of the stone, all Luck Points are nullified until the stone is returned. The Scenario dice is triggered on a result of 8-0. After all enemies are defeated, a hero must stand before the statue and pass a DEX Test to replace the stone, which takes one turn. Failure increases the Threat Level by 1.",
                    CorridorCount = 8,
                    RoomCount = 8,
                    RewardCoin = 300,
                    EncounterType = EncounterType.Random,
                    ObjectiveRoom = _room.GetRoomByName("The Chamber of Reverence"),
                    StartThreatLevel = 5,
                    MinThreatLevel = 5,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "A local nobleman has contracted the heroes to return a polished black stone to a long-forgotten temple. Since retrieving the stone, he has suffered from bad luck and believes it to be cursed. He hopes returning it will lift the curse.",
                    NarrativeObjectiveRoom = "The heroes enter the chamber and see the great statue at the far end, with an obvious place for the stone. However, a horde of enemies stands between them and their objective.",
                    NarrativeSetup = "Roll twice on the Encounter Table and place the enemies randomly. The heroes may act first.",
                    NarrativeAftermath = "Once the stone is returned to its place, absolutely nothing happens. The heroes head home, where the nobleman pays them their reward, convinced their luck has turned now that the stone is gone."
                },
                new Quest()
                {
                    Name = "Slaying the Fiend",
                    Location = QuestLocation.Random,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "The fiend, Molgor, is a huge Minotaur with the Frenzy and Ferocious Charge rules. He has been wounded in a previous fight; roll on a table to determine how many starting wounds to subtract.",
                    CorridorCount = 8,
                    RoomCount = 8,
                    RewardSpecial = "A pile of gold and two objective chests, which are unlocked and not trapped.",
                    EncounterType = EncounterType.Beasts,
                    ObjectiveRoom = _room.GetRoomByName("The Chamber of Reverence"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "The heroes hear a tale from a wounded knight about a valiant battle against numerous beasts in the ruins of Fort Summerhall. The knight speaks of wounding a huge fiend and seeing a glimmer of gold in its chamber. Curious, the party sets out to investigate.",
                    NarrativeObjectiveRoom = "The heroes enter a large hall with a huge idol to a Dark God at the far end. The floor is littered with the bodies of beastmen. As they walk towards the idol, a deep roar echoes and the fiend, Molgor, charges from the shadows.",
                    NarrativeSetup = "Place the heroes 1d3 squares from the door. Place Molgor 1d6 squares from the idol, charging towards the heroes. Molgor acts first.",
                    NarrativeAftermath = "Once the fiend is defeated, the heroes search the chamber. The knight's story proves true, and they find a pile of gold and two objective chests."
                },
                new Quest()
                {
                    Name = "Closing the Portal",
                    Location = QuestLocation.SilverCity,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "Any encounter roll of an even 10 (10, 20, 30...) results in an encounter with random demons. To close the portal, a hero must spend 1d6+1 consecutive turns reading a scroll. If the reader is interrupted in any way, they must start over. Every other turn, a new demon emerges from the portal.",
                    CorridorCount = 8,
                    RoomCount = 8,
                    RewardCoin = 300,
                    RewardSpecial = "Two unlocked and untrapped objective chests are in the room.",
                    EncounterType = EncounterType.Random,
                    ObjectiveRoom = _room.GetRoomByName("The Chamber of Reverence"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "Demons have been spotted in the sewers beneath the city, and the High Wizard believes an open portal is the cause. The Jarl has tasked the heroes with entering the catacombs to find and seal the gate using a special scroll.",
                    NarrativeObjectiveRoom = "A humming sound leads the heroes to a large chamber. In the center, over a summoning circle, a shimmering bubble hangs in the air, periodically crackling and spewing out a demon. A large number of demons already in the room fix their eyes on the adventurers.",
                    NarrativeSetup = "Roll twice on the special demon encounter table and place them randomly in the room. The heroes enter centered along the long side of the room.",
                    NarrativeAftermath = "Once the portal is closed and the final demon is slain, the party loots two objective chests. Back on the surface, the Jarl and the wizard greet them thankfully and provide the promised reward."
                },
                new Quest()
                {
                    Name = "Retrieving the Family Heirloom",
                    Location = QuestLocation.Random,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "The Scenario dice is triggered on 8-0. The objective room contains six tombs. Take 6 playing cards (3 black, 1 red, 2 face cards) and shuffle them. Opening a tomb takes two heroes a full turn. Draw a card: Black means an empty tomb. Red means the sword is found. A face card means a mummy attacks.",
                    CorridorCount = 8,
                    RoomCount = 8,
                    RewardCoin = 300,
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Great Crypt"),
                    StartThreatLevel = 5,
                    MinThreatLevel = 5,
                    MaxThreatLevel = 18,
                    NarrativeQuest = "A young knight has hired the heroes to retrieve a powerful family heirloom—a sword of immense beauty—from their great-great-grandfather's tomb in one of the city's mausoleums. The knight himself is unable to go, as he has caught the flu.",
                    NarrativeObjectiveRoom = "The heroes enter the Great Crypt where the ancestor is entombed. The room is covered in cobwebs and dust, and there are six tombs, not one. The engravings are worn away, so the heroes must open them one by one to find the correct one.",
                    NarrativeSetup = "There are no enemies in the room when the heroes enter.",
                    NarrativeAftermath = "Once the sword is retrieved, the heroes return to the surface. The young knight yanks the sword from their hands, complaining about the time it took, but nonetheless pays the agreed-upon reward."
                },
                new Quest()
                {
                    Name = "Stopping the Necromancer",
                    Location = QuestLocation.CurrentTown,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "The floor of the objective room is covered in corpses, making combat difficult; heroes suffer a -10% penalty to CS and RS. A 'To Hit' roll of 90+ means the hero falls and must pass a DEX test to stand up. The Zombies will form a protective circle around Ragnalf and will not move. If Ragnalf dies, all zombies die as well.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 300,
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Great Crypt"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "The heroes have been tasked with finding and killing Ragnalf the Mad, a Necromancer who has been raiding cemeteries for corpses. After tracing their minions to a ruined fort near the city, the heroes prepare to locate the evil wizard.",
                    NarrativeObjectiveRoom = "The heroes enter a room filled with the stench of death and literally covered in corpses, some of which are twitching. At the far end stands Ragnalf the Mad, an old man in a tattered robe, surrounded by a host of zombies. The heroes carefully advance.",
                    NarrativeSetup = "Place Ragnalf the Necromancer at the far end of the room, with 6 Zombies armed with longswords placed close to them. The heroes enter along a short side.",
                    NarrativeAftermath = "When the Necromancer dies, all the zombies collapse and the eerie movement on the floor stops. The heroes, not wanting to linger, head back to town to collect their reward."
                },
                new Quest()
                {
                    Name = "Tomb Raiders",
                    Location = QuestLocation.White38,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "Opening a tomb takes two heroes a full turn. While in the objective room, the Scenario dice triggers an event on a roll of 1-4; any result that would increase the Threat Level instead summons a Wandering Monster.",
                    CorridorCount = 7,
                    RoomCount = 5,
                    RewardSpecial = "Any loot found.",
                    EncounterType = EncounterType.Undead,
                    ObjectiveRoom = _room.GetRoomByName("The Great Crypt"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "An ancient tomb has been discovered far to the south of Silver City. High King Logan III has requested it be opened and examined. At the suggestion of the Jarl, the heroes take the job with the deal that they may keep anything they find.",
                    NarrativeObjectiveRoom = "The heroes enter a vast, silent chamber filled with tombs. There is no movement, only the promise of ancient treasure within the graves.",
                    NarrativeSetup = "The heroes remain where they stood when they opened the door to the chamber.",
                    NarrativeAftermath = "Coming back into the sunlight, the heroes enjoy the fresh air. The quest was not as bad as feared, and they wonder if more tombs lie further to the south."
                },
                new Quest()
                {
                    Name = "The Pyramid of Xantha",
                    Location = QuestLocation.AncientLands,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "Due to the abundance of treasure, you may re-roll 4 rolls on the Furniture Chart during the quest (a re-roll cannot be re-rolled). If the scenario roll is triggered in the objective room, the mummy of Xánthu himself (a Mummy Prince) will rise from a sarcophagus to join the fight.",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardSpecial = "Any loot found. A hidden, beautiful chest can also be discovered and searched as an objective chest.",
                    EncounterType = EncounterType.AncientLands,
                    ObjectiveRoom = _room.GetRoomByName("The Large Tomb"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "The heroes have heard of the Pyramid of Xánthu, the final resting place of a cruel but prosperous ruler from the Ancient Lands. Believing he was buried with their treasures, they deem the ominous pyramid a worthy target for dungeoneering.",
                    NarrativeObjectiveRoom = "The heroes enter a vast, mostly empty chamber containing two intricately carved stone sarcophagi at its center, surrounded by painted glyphs on the floor. The hall is not empty, however, as undead guardians must be dispatched before the room can be examined.",
                    NarrativeSetup = "The heroes enter along a short edge of the room. Make two rolls on the Encounter Table and place the enemies randomly.",
                    NarrativeAftermath = "Once the chamber is quiet, the heroes search for treasure, carefully avoiding the glyphs. They eventually find a set of movable stones in a wall, revealing a beautiful chest behind them."
                },
                new Quest()
                {
                    Name = "Tomb of the Hierophant",
                    Location = QuestLocation.AncientLands,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "You may re-roll 4 rolls on the furniture chart. Mix a special deck of 14 playing cards (11 red, Ace/2/3 of Spades). Flip one card upon entering each new tile; if a spade is drawn, a special event occurs (finding a statue, triggering a fire trap, or finding a stone tablet).",
                    CorridorCount = 7,
                    RoomCount = 7,
                    RewardCoin = 300,
                    RewardSpecial = "An extra 100c per hero for bringing back the stone tablet, and another 100c per hero for the golden statue. A rune-covered tome is also found in the sarcophagus.",
                    EncounterType = EncounterType.AncientLands,
                    ObjectiveRoom = _room.GetRoomByName("R22"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "The Golden College of Magic has requested that the heroes investigate the tomb of Djet, an influential Hierophant who seized power through dark arts. They believe valuable artifacts may still be present that could shed light on the ancient civilization.",
                    NarrativeObjectiveRoom = "The heroes enter a surprisingly small chamber. On a stone sarcophagus in the center, the dried husk of the Hierophant Djet begins to move and rise. At the same time, the sound of approaching footsteps echoes from the tile they just left.",
                    NarrativeSetup = "Place a Mummy Priest next to the sarcophagus. Then roll twice on the Encounter Table and place those enemies in the tile the heroes just vacated.",
                    NarrativeAftermath = "After the battle, the party searches the chamber. Lying in the sarcophagus is an ancient, rune-covered tome which they collect automatically, along with any other treasure. Back at the outpost, they receive bonus payment for any special artifacts they found."
                },
                new Quest()
                {
                    Name = "Temple of Despair",
                    Location = QuestLocation.AncientLands,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "You may re-roll 4 rolls on the furniture chart during the quest. A side quest card is added to the exploration deck; if drawn, it triggers a special encounter with a Mummy Priest and wraiths.",
                    CorridorCount = 6,
                    RoomCount = 6,
                    RewardSpecial = "Any loot found. The party also finds several valuable old books for the High Priests. However, upon leaving, each hero is afflicted with a random curse that lasts until the end of their next dungeon.",
                    EncounterType = EncounterType.AncientLands,
                    ObjectiveRoom = _room.GetRoomByName("R33"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "Knowledge of the Ancient Gods is scarce, but scholars believe the Temple of Despair was a site for human mass offerings. The High Priest has requested the heroes examine the site and bring back any related artifacts.",
                    NarrativeObjectiveRoom = "The chamber ahead appears to be a chapel or place of worship, making it a likely place to find the artifacts the heroes are looking for.",
                    NarrativeSetup = "Roll once on the Encounter Table and place the enemies randomly. If the special side quest encounter was triggered, those creatures will be in this room instead.",
                    NarrativeAftermath = "The heroes find and pack several dusty, thick old books of value. As they leave the temple, the angered Ancient Gods place a curse on each hero."
                },
                new Quest()
                {
                    Name = "Halls of Amenhotep",
                    Location = QuestLocation.AncientLands,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "You may re-roll 4 rolls on the furniture chart during the quest. The fifth time the initiative bag is reloaded, two Tomb Guardians awaken from the statues in the room and join the fight.",
                    CorridorCount = 6,
                    RoomCount = 6,
                    RewardCoin = 500,
                    RewardSpecial = "Lots of loot to be found after the battle.",
                    EncounterType = EncounterType.AncientLands,
                    ObjectiveRoom = _room.GetRoomByName("The Ancient Throne Room"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "Little is known of the ruler Amenhotep, but the halls from where he reigned have been located. Untouched for millennia, they are bound to contain riches and are a perfect target for dungeoneering.",
                    NarrativeObjectiveRoom = "The vast chamber is inspiring, with intricately carved walls and detailed seated statues that speak to its former magnificence.",
                    NarrativeSetup = "The party enters the throne room along the short edge. Roll twice on the Encounter Table and place the enemies randomly in the room.",
                    NarrativeAftermath = "With all the evil defeated, the heroes can enjoy the best part of dungeoneering: going through all the loot. Once done, the party is ready to move on."
                },
                new Quest()
                {
                    Name = "Crypt of Khaba",
                    Location = QuestLocation.AncientLands,
                    StartingRoom = _room.GetRoomByName("Start Tile"),
                    SpecialRules = "You may re-roll 4 rolls on the furniture chart. A special playing card deck is used; each time you enter a new tile, you flip a card. If the Ace of Spades is drawn, it triggers a special encounter with the undead Queen Khaba and them four wight bodyguards.",
                    CorridorCount = 6,
                    RoomCount = 6,
                    RewardCoin = 500,
                    RewardSpecial = "The Sceptre of the Queen, a legendary magic staff that contains the Fireball spell (cast once per dungeon, never needs recharging).",
                    EncounterType = EncounterType.AncientLands,
                    ObjectiveRoom = _room.GetRoomByName("The Large Tomb"),
                    StartThreatLevel = 6,
                    MinThreatLevel = 6,
                    MaxThreatLevel = 20,
                    NarrativeQuest = "Queen Khaba was a fascinating exception to the male rulers of the Ancient Lands. Her crypt has finally been identified, and it is said to be untouched for millennia. She was always pictured with a sceptre, and the heroes hope to find it with them remains.",
                    NarrativeObjectiveRoom = "The heroes have finally located the burial site of Queen Khaba. Her legendary sceptre may be awaiting liberation within one of the two sarcophagi.",
                    NarrativeSetup = "The party enters along the long edge. Roll twice on the Encounter Table and place the enemies randomly. If the party has not yet met Queen Khaba through the special card event, she will be present in this room next to them sarcophagus.",
                    NarrativeAftermath = "With all undead returned to dust, the party rummages through the chamber. As expected, they find the Sceptre of the Queen in them final resting place."
                }
            };
        }

        public Quest GetQuestByName(string name)
        {
            return Quests.First(q => q.Name == name);
        }
    }
}
