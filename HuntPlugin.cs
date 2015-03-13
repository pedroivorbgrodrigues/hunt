using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;
using Oxide.Ext.Hunt.ExtensionsCore;
using Oxide.Ext.Hunt.RPG;
using Oxide.Ext.Hunt.RPG.Keys;
using Oxide.Plugins;
using Oxide.Rust.Libraries;
using UnityEngine;
using LogType = Oxide.Core.Logging.LogType;

namespace Oxide.Ext.Hunt
{
    public class HuntPlugin : CSPlugin
    {
        public readonly Logger Logger;
        private HuntRPG HuntRPGInstance;

        public HuntPlugin()
        {
            Title = ExtensionInfo.Title;
            Name = ExtensionInfo.Name;
            Author = ExtensionInfo.Author;
            Version = ExtensionInfo.Version;
            HasConfig = true;
            ResourceId = ExtensionInfo.ResourceId;
            Logger = Interface.GetMod().RootLogger;
        }

        protected override void LoadDefaultConfig()
        {
            Config[HK.XPTable] = HuntTablesGenerator.GenerateXPTable(HK.MaxLevel, HK.BaseXP, HK.LevelMultiplier, HK.LevelModule, HK.ModuleReducer);
            Config[HK.MessagesTable] = HuntTablesGenerator.GenerateMessageTable();
            Config[HK.SkillTable] = HuntTablesGenerator.GenerateSkillTable();
        }

        private void LoadRPG()
        {
            LoadConfig();
            Interface.GetMod().DataFileSystem.GetDatafile(HK.DataFileName);
            var rpgConfig = Interface.GetMod().DataFileSystem.ReadObject<Dictionary<string, RPGInfo>>(HK.DataFileName);
            Logger.Write(LogType.Info, String.Format("{0} profiles loaded", rpgConfig.Count));
            var xpTable = ReadFromConfig<Dictionary<int, long>>(HK.XPTable);
            var messagesTable = ReadFromConfig<PluginMessagesConfig>(HK.MessagesTable);
            var skillTable = ReadFromConfig<Dictionary<string, Skill>>(HK.SkillTable);
            if(xpTable == null)
                Logger.Write(LogType.Error, "Failed to load XPTable");
            if (messagesTable == null)
                Logger.Write(LogType.Error, "Failed to load MessageTable");
            if (skillTable == null)
                Logger.Write(LogType.Error, "Failed to load SkillTable");
            HuntRPGInstance.ConfigRPG(messagesTable, xpTable, skillTable,rpgConfig);
        }

        public T ReadFromConfig<T>(string configKey)
        {
            string serializeObject = JsonConvert.SerializeObject(Config[configKey]);
            return JsonConvert.DeserializeObject<T>(serializeObject);
        }

        public void SaveRPG(Dictionary<string, RPGInfo> rpgConfig)
        {
            Interface.GetMod().DataFileSystem.WriteObject(HK.DataFileName, rpgConfig);
            Logger.Write(LogType.Info, String.Format("{0} profiles saved", rpgConfig.Count));
        }

        private bool HasAccess(BasePlayer player)
        {
            return player.net.connection.authLevel >= 2;
        }

        [HookMethod("BuildServerTags")]
        private void BuildServerTags(IList<string> taglist)
        {
            taglist.Add(ExtensionInfo.Name);
        }

        [HookMethod("Init")]
        void Init()
        {
            var library = Interface.GetMod().GetLibrary<Command>("Command");
            library.AddConsoleCommand("hunt.xptable", this, "cmdGenerateXPTable");
            library.AddConsoleCommand("hunt.reset", this, "cmdResetRPG");
            library.AddChatCommand("hunt", this, "cmdHunt");
            library.AddChatCommand("h", this, "cmdHuntShortcut");
            Logger.Write(LogType.Info, "Hunt initialized!");
            HuntRPGInstance = new HuntRPG(this);
            if (HuntRPGInstance == null)
                Logger.Write(LogType.Info, "Problem initializating RPG Instance!");
        }

        [HookMethod("OnServerInitialized")]
        void OnServerInitialized()
        {
            LoadRPG();
        }

        [HookMethod("OnUnload")]
        void OnUnload()
        {
            HuntRPGInstance.SaveRPG();
        }

        [HookMethod("Loaded")]
        private void Loaded()
        {
            Interface.GetMod().DataFileSystem.GetDatafile(HK.DataFileName);
            LoadRPG();
        }

        [HookMethod("OnPlayerInit")]
        void OnPlayerInit(BasePlayer player)
        {
            HuntRPGInstance.InitPlayer(player);
        }

        [HookMethod("OnEntityAttacked")]
        object OnEntityAttacked(MonoBehaviour entity, HitInfo hitInfo)
        {
            var player = entity as BasePlayer;
            if (player == null) return null;
            if (!HuntRPGInstance.OnAttacked(player, hitInfo)) return null;
            hitInfo = new HitInfo();
            return hitInfo;
        }


        [HookMethod("OnEntityDeath")]
        void OnEntityDeath(MonoBehaviour entity, HitInfo hitinfo)
        {
            var player = entity as BasePlayer;
            if (player == null) return;
            HuntRPGInstance.OnDeath(player);
        }

        [HookMethod("OnItemCraft")]
        ItemCraftTask OnItemCraft(ItemCraftTask item)
        {
            return HuntRPGInstance.OnItemCraft(item);
        }

        [HookMethod("OnGather")]
        void OnGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            HuntRPGInstance.OnGather(dispenser, entity, item);
        }

        [HookMethod("cmdHuntShortcut")]
        void cmdHuntShortcut(BasePlayer player, string command, string[] args)
        {
            cmdHunt(player, command, args);
        }

        [HookMethod("cmdHunt")]
        void cmdHunt(BasePlayer player, string command, string[] args)
        {
            Logger.Write(LogType.Info, "Chat command called");
            HuntRPGInstance.HandleChatCommand(player, args);
        }

        [ChatCommand("cmdResetRPG")]
        void cmdResetRPG(ConsoleSystem.Arg arg)
        {
            if (!arg.CheckPermissions()) return;
            HuntRPGInstance.ResetRPG();
        }

        [HookMethod("cmdGenerateXPTable")]
        private void cmdGenerateXPTable(ConsoleSystem.Arg arg)
        {
            if (!arg.CheckPermissions()) return;
            arg.ReplyWith("Gerando Tabela");
            var levelMultiplier = HK.LevelMultiplier;
            var baseXP = HK.BaseXP;
            var levelModule = HK.LevelModule;
            var moduleReducer = HK.ModuleReducer;
            if (arg.HasArgs())
                baseXP = arg.GetInt(0);
            if (arg.HasArgs(2))
                levelMultiplier = arg.GetFloat(1);
            if (arg.HasArgs(3))
                levelModule = arg.GetInt(2);
            if (arg.HasArgs(4))
                moduleReducer = arg.GetFloat(3);
                Config[HK.XPTable] = HuntTablesGenerator.GenerateXPTable(HK.MaxLevel, baseXP, levelMultiplier, levelModule, moduleReducer);
            SaveConfig();
            arg.ReplyWith("Tabela Gerada");
        }

        [HookMethod("OnServerSave")]
        void OnServerSave()
        {
            HuntRPGInstance.SaveRPG();
        }

        [HookMethod("OnServerQuit")]
        void OnServerQuit()
        {
            HuntRPGInstance.SaveRPG();
        }

    }
}
