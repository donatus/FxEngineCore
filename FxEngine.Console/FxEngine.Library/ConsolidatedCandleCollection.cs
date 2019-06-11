using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public class ConsolidatedCandleCollection : CandleCollection
    {

        private readonly CandleCollection _collectiontoconsolidate;

        public ConsolidatedCandleCollection(CandleCollection candleCollection) : base(candleCollection.Instrument, candleCollection.Period, candleCollection.PeriodCount)
        {
            _collectiontoconsolidate = candleCollection;
            _collectiontoconsolidate.CandleAddedEvent += CandleAdded;
        }

        private void CandleAdded(object sender, Candle addedCandle)
        {
            DateTime beginPeriod = addedCandle.DateTime.GetBeginPeriod(Period, PeriodCount);
        }
    }
}
