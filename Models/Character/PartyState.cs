namespace LoDCompanion.Models.Character
{
    public class PartyState
    {
        public Party? CurrentParty { get; private set; }

        public void SetParty(Party party)
        {
            CurrentParty = party;
        }

        public void Reset()
        {
            CurrentParty = null;
        }

        public void CreateParty()
        {
            CurrentParty = new Party();
        }
    }

    public class Party
    {
        public string Id { get; private set; }
        public List<Hero> Heroes { get; set; } = new List<Hero>();
        public int Coins { get; set; }

        public Party() 
        { 
            Id = Guid.NewGuid().ToString();
        }

    }
}