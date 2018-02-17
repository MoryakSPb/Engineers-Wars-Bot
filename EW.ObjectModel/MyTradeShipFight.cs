using System;

namespace EW.ObjectModel
{
    public class MyTradeShipFight : AMyFight
    {
        public MyTradeShipFight(string attackersTag, string defendersTag, DateTime startTime) : base(attackersTag, defendersTag, startTime)
        {
        }
    }
}