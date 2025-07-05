namespace LoDCompanion.Models.Character
{
    public class Party
    {
        public string Id { get; private set; }
        public List<Hero> Heroes { get; set; } = new List<Hero>();
        public int Coins { get; set; }
        public int PartyMorale { get; set; }

        public Party() 
        { 
            Id = Guid.NewGuid().ToString();
        }

    }
}