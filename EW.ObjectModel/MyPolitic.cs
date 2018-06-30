using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [DataContract]
    public class MyPolitic
    {
        [DataMember]
        private bool _mPact;
        [DataMember]
        protected (string, string) MFactions;
        [DataMember]
        protected bool MUnion;

        [IgnoreDataMember]
        public string Id => Factions.Item1 + "-" + Factions.Item2;

        [IgnoreDataMember]
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

        [DataMember]
        public int PactTurns { get; set; }

        public bool Union
        {
            get => Status == MyPoliticStatus.Ally && MUnion;
            set => MUnion = value;
        }

        [DataMember]
        public MyPoliticStatus Status { get; set; }

        public MyPolitic((string, string) mFactions)
        {
            MFactions = mFactions;
            Status = MyPoliticStatus.Neutral;
        }

        /*public void NextTurn()
        {
            if (!Pact) return;
            --PactTurns;
            if (!((PactTurns <= 0) & (Status != MyPoliticStatus.Ally))) return;
            Pact = false;
            PactTurns = 0;
        }*/
    }

    public enum MyPoliticStatus
    {
        War,
        Neutral,
        Ally
    }
}
