using System.Collections.Generic;
using System.Linq;

namespace Hunt.RPG
{
    public class PluginMessagesConfig
    {
        public Dictionary<string, List<string>> Messages { set; get; }
        public string ChatPrefix { set; get; }
        public string ChatPrefixColor { set; get; }

        public PluginMessagesConfig(string chatPrefix, string chatPrefixColor)
        {
            Messages = new Dictionary<string, List<string>>();
            ChatPrefix = chatPrefix;
            ChatPrefixColor = chatPrefixColor;
        }

        public void AddMessage(string key, string message)
        {
            Messages.Add(key, new List<string> {message});
        }

        public void AddMessage(string key, List<string> message)
        {
            Messages.Add(key, message);
        }

        public List<string> GetMessage(string key, string[] args = null)
        {
            var strings = new List<string>();
            if (!Messages.ContainsKey(key)) return strings;
            var messageList = Messages[key];
            strings.AddRange(messageList.Select(message => args == null ? message : string.Format(message, args)));
            return strings;
        }
    }
}