using FxEngine.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                case Period.H:
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
                case Period.M:
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
                case Period.S:
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

        internal static DateTimeOffset Max(DateTimeOffset dateA, DateTimeOffset dateB)
        {
            return dateA > dateB ? dateA : dateB;
        }

        internal static DateTime Max(DateTime dateA, DateTime dateB)
        {
            return dateA > dateB ? dateA : dateB;
        }

        public static DateTime AddPeriod(this DateTime from,Period period, int periodCount, int count = 1)
        {
            DateTime result = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second);

            switch (period)
            {
                case Period.H:
                    result = result.AddHours(periodCount * count);
                    break;
                case Period.M:
                    result = result.AddMinutes(periodCount * count);
                    break;
                case Period.S:
                    result = result.AddSeconds(periodCount * count);
                    break;
            }

            return result;
        }
        public static string ToRfc3339(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }

        public static string ToRfc3339(this DateTimeOffset dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz");
        }


        public static IEnumerable<DateTime> GetPeriods(this DateTime from, DateTime to, Period period, int periodCount)
        {
            DateTime iterator = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second);

            while(iterator <= to)
            {
                yield return iterator;
                iterator = iterator.AddPeriod(period, periodCount);
            }
        }
    }
}
