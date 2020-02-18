using System;
using System.Text;

namespace TurnTracker.Common
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendFormattable(this StringBuilder sb, FormattableString formattableString)
        {
            return sb.AppendFormat(formattableString.Format, formattableString.GetArguments());
        }
    }
}