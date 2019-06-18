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

            Assert.Equal(new DateTime(2019, 01, 01, 10, 30, 30), dateTime.GetBeginPeriod(Period.H, 2));
            Assert.Equal(new DateTime(2019, 01, 01, 9, 30, 30), dateTime.GetBeginPeriod(Period.H, 3));
            Assert.Equal(new DateTime(2019, 01, 01, 11, 30, 30), dateTime.GetBeginPeriod(Period.H, 1));

            Assert.Equal(new DateTime(2019, 01, 01, 12, 29, 30), dateTime.GetBeginPeriod(Period.M, 1));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 25, 30), dateTime.GetBeginPeriod(Period.M, 5));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 15, 30), dateTime.GetBeginPeriod(Period.M, 15));

            Assert.Equal(new DateTime(2019, 01, 01, 12, 30, 25), dateTime.GetBeginPeriod(Period.S, 5));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 30, 15), dateTime.GetBeginPeriod(Period.S, 15));
            Assert.Equal(new DateTime(2019, 01, 01, 12, 30, 29), dateTime.GetBeginPeriod(Period.S, 1));

        }

        [Fact]
        public void BrowseDateTimePeriods()
        {
            DateTime from = new DateTime(2019, 01, 01, 12, 30, 30);

            foreach(DateTime date in from.GetPeriods(new DateTime(2019, 01, 01, 12, 45, 30), Period.M, 1))
            {

            }
        }
    }
}
