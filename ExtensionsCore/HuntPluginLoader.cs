using System.Collections.Generic;
using Oxide.Core.Plugins;

namespace Oxide.Ext.Hunt.ExtensionsCore
{
    public class HuntPluginLoader : PluginLoader
    {
        public override IEnumerable<string> ScanDirectory(string directory)
        {
            return new[] {ExtensionInfo.Name};
        }

        public override Plugin Load(string directory, string name)
        {
            switch (name)
            {
                case ExtensionInfo.Name:
                    return new HuntPlugin();
                default:
                    return null;
            }
        }
    }
}
