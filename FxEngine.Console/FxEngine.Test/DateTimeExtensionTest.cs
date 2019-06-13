using FxEngine.Library;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FxEngine.Test
{
    public class DateTimeExtensionTest
    {
        [Fact]
        public void GetBeginPeriodTest()
        {
            DateTime dateTime = new DateTime(2019, 01, 01, 12, 30, 30);

            Assert.Equal(new DateTime(2019, 01, 01, 10, 30, 30), dateTime.GetBeginPeriod(Period.Hour, 2));
            Assert.Equal(new DateTime(2019, 01, 01, 9, 30, 30), dateTime.GetBeginPeriod(Period.Hour, 3));
            Assert.Equal(new DateTime(2019, 01, 01, 11, 30, 30), dateTime.GetBeginPeriod(Period.Hour, 1));

            Assert.Equal(new DateTime(2019, 01, 01, 12, 29, 30), dateTime.GetBeginPeriod(Period.Minute, 1));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 25, 30), dateTime.GetBeginPeriod(Period.Minute, 5));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 15, 30), dateTime.GetBeginPeriod(Period.Minute, 15));

            Assert.Equal(new DateTime(2019, 01, 01, 12, 30, 25), dateTime.GetBeginPeriod(Period.Second, 5));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 30, 15), dateTime.GetBeginPeriod(Period.Second, 15));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 30, 29), dateTime.GetBeginPeriod(Period.Second, 1));

        }

        [Fact]
        public void BrowseDateTimePeriods()
        {
            DateTime from = new DateTime(2019, 01, 01, 12, 30, 30);

            foreach(DateTime date in from.GetPeriods(new DateTime(2019, 01, 01, 12, 45, 30), Period.Minute, 1))
            {

            }
        }
    }
}
