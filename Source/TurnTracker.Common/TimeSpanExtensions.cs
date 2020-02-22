using System;

namespace TurnTracker.Common
{

    public static class TimeSpanExtensions
    {
        public const double DaysPerYear = 365.25;
        public const double DaysPerMonth = DaysPerYear / 12;

        public static string Plural(this int number) => number == 1 ? "" : "s";

        public static string ToDisplayString(this TimeSpan time)
        {
            if (time.TotalDays >= DaysPerYear)
            {
                var years = ExtractYears(ref time);
                var months = ExtractMonths(ref time);
                return $"{years} year{years.Plural()} {months} month{months.Plural()} {time.Days} day{time.Days.Plural()}";
            }

            if (time.TotalDays >= DaysPerMonth)
            {
                var months = ExtractMonths(ref time);
                return $"{months} month{months.Plural()} {time.Days} day{time.Days.Plural()}";
            }

            if (time.TotalDays >= 1)
            {
                return $"{time.Days} day{time.Days.Plural()} {time.Hours} hour{time.Hours.Plural()}";
            }

            if (time.TotalHours >= 1)
            {
                return $"{time.Hours} hour{time.Hours.Plural()} {time.Minutes} minute{time.Minutes.Plural()}";
            }

            if (time.TotalMinutes >= 1)
            {
                return $"{time.Minutes} minute{time.Minutes.Plural()} {time.Seconds} second{time.Seconds.Plural()}";
            }

            return $"{time.Seconds} second{time.Seconds.Plural()}";
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