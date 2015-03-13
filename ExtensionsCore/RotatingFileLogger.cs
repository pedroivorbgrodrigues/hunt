using System;
using System.IO;
using Oxide.Core;
using Oxide.Core.Logging;

namespace Oxide.Ext.Hunt.ExtensionsCore
{
    public class RotatingFileLogger : ThreadedLogger
    {
        private StreamWriter Writer;
        private string GetLogFilename(DateTime date)
        {
            string fileName = string.Format("{0}_{1:dd-MM-yyyy}.txt", ExtensionInfo.Name ,date);
            return Path.Combine(Interface.GetMod().LogDirectory, fileName);
        }

        protected override void BeginBatchProcess()
        {
            Writer = new StreamWriter(new FileStream(GetLogFilename(DateTime.Now), FileMode.Append, FileAccess.Write));
        }
        protected override void ProcessMessage(LogMessage message)
        {
            Writer.WriteLine(message.Message);
        }
        protected override void FinishBatchProcess()
        {
            Writer.Close();
            Writer.Dispose();
            Writer = null;
        }
    }
}
