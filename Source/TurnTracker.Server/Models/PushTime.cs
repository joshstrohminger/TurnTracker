using System;
using System.Globalization;

namespace TurnTracker.Server.Models
{
    public class PushTime
    {
        public string When { get; set; }

        public DateTimeOffset Parse() => DateTimeOffset.ParseExact(When.Substring(0, When.IndexOf('(') - 1), @"ddd MMM dd yyyy HH:mm:ss \G\M\TK", CultureInfo.InvariantCulture.DateTimeFormat);
    }
}