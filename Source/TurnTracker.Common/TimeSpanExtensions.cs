using System;

namespace TurnTracker.Common
{
    public static class TimeSpanExtensions
    {
        public const double DaysPerYear = 365.25;
        public const double DaysPerMonth = DaysPerYear / 12;

        public static string ToDisplayString(this TimeSpan time)
        {
            if (time.TotalDays >= DaysPerYear)
            {
                var years = ExtractYears(ref time);
                var months = ExtractMonths(ref time);
                return $"{years}y {months}m {time.TotalDays:0}";
            }

            if (time.TotalDays >= DaysPerMonth)
            {
                var months = ExtractMonths(ref time);
                return $"{months}m {time.Days}d";
            }

            if (time.TotalDays >= 1)
            {
                return time.ToString(@"d\d\ h\h");
            }

            if (time.TotalHours >= 1)
            {
                return time.ToString(@"h\h\ m\m");
            }

            if (time.TotalMinutes >= 1)
            {
                return time.ToString(@"m\m\ s\s");
            }

            return time.ToString(@"s\s");
        }

        private static int ExtractYears(ref TimeSpan time)
        {
            return ExtractUnit(ref time, DaysPerYear);
        }

        private static int ExtractMonths(ref TimeSpan time)
        {
            return ExtractUnit(ref time, DaysPerMonth);
        }

        private static int ExtractUnit(ref TimeSpan time, double numberOfDaysInUnit)
        {
            var count = (int)(time.TotalDays / numberOfDaysInUnit);
            time = time.Subtract(TimeSpan.FromDays(count * numberOfDaysInUnit));
            return count;
        }
    }
}