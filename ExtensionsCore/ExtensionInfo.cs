using System.Reflection;
using Oxide.Core;

namespace Oxide.Ext.Hunt.ExtensionsCore
{
    static class ExtensionInfo
    {
        public const string Title = "Hunt Mechanics for Oxide";
        public const string Name = "Hunt";
        public const string Author = "SW";
        public const int ResourceId = 0;
        public static VersionNumber Version = new VersionNumber((ushort)Assembly.GetExecutingAssembly().GetName().Version.Major, (ushort)Assembly.GetExecutingAssembly().GetName().Version.Minor, (ushort)Assembly.GetExecutingAssembly().GetName().Version.Build);
    }
}
