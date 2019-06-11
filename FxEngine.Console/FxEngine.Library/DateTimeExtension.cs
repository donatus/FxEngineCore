using FxEngine.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class DateTimeExtension
    {
        public static DateTime GetBeginPeriod(this DateTime date, Period period, int periodCount, bool fixedInterval = false)
        {
            DateTime result = DateTime.MinValue;
            int residual = 0;

            switch (period)
            {
                case Period.Hour:
                    result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                    if (fixedInterval)
                    {
                        residual = result.Hour % periodCount;
                        result = result.AddHours(-residual);
                    }
                    else
                    {
                        result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                        result = result.AddHours(-periodCount);
                    }

                    break;
                case Period.Minute:
                    result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    if (fixedInterval)
                    {
                        residual = result.Minute % periodCount;
                        result = result.AddMinutes(-residual);
                    }
                    else
                    {
                        result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                        result = result.AddMinutes(-(periodCount));
                    }
                    break;
                case Period.Second:
                    result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                    if (fixedInterval)
                    {
                        residual = result.Second % periodCount;
                        result = result.AddSeconds(-residual);
                    }
                    else
                    {
                        result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                        result = result.AddSeconds(-(periodCount));
                    }
                    break;
            }

            return result;
        }
    }
}
