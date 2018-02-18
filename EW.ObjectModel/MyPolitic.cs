namespace EW.ObjectModel
{
    public class MyPolitic
    {
        private bool _mPact;
        protected (string, string) MFactions;
        protected bool MUnion;

        public MyPolitic((string, string) mFactions)
        {
            MFactions = mFactions;
            Status = MyPoliticStatus.Neutral;
        }

        public string Id => Factions.Item1 + "-" + Factions.Item2;
        public string Id2 => Factions.Item2 + "-" + Factions.Item1;

        public (string, string) Factions
        {
            get => MFactions;
            set => MFactions = value;
        }

        public bool Pact
        {
            get => Status == MyPoliticStatus.Ally || Status != MyPoliticStatus.War && _mPact;
            set => _mPact = value;
        }

        public int PactTurns { get; set; }

        public bool Union
        {
            get => Status == MyPoliticStatus.Ally && MUnion;
            set => MUnion = value;
        }

        public MyPoliticStatus Status { get; set; }

        public void NextTurn()
        {
            if (!Pact) return;
            --PactTurns;
            if (!((PactTurns <= 0) & (Status != MyPoliticStatus.Ally))) return;
            Pact = false;
            PactTurns = 0;
        }
    }


    public enum MyPoliticStatus
    {
        War,
        Neutral,
        Ally
    }
}