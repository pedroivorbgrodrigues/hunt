using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Logging;
using Oxide.Unity.Logging;

namespace Oxide.Ext.Hunt.ExtensionsCore
{
    public class HuntExtension : Extension
    {
        private CompoundLogger logger;

        public HuntExtension(ExtensionManager manager) : base(manager)
        {
            logger = new CompoundLogger();
            logger.AddLogger(new UnityLogger());
            logger.AddLogger(new RotatingFileLogger());
        }

        public override void Load()
        {
            Manager.RegisterPluginLoader(new HuntPluginLoader());
        }

        public override void LoadPluginWatchers(string plugindir)
        {
        }

        public override void OnModLoad()
        {
        }

        public override string Name
        {
            get { return ExtensionInfo.Name; }
        }

        public override VersionNumber Version
        {
            get { return ExtensionInfo.Version; }
        }

        public override string Author
        {
            get { return ExtensionInfo.Author; }
        }
    }
}
