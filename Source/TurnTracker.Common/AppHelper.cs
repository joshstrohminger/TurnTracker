using System.Reflection;

namespace TurnTracker.Common
{
    public class AppHelper
    {
        public static string Version { get; } = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}