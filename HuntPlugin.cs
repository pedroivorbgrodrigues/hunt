using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hunt.RPG;
using Hunt.RPG.Keys;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Hunt RPG", "PedraozauM / SW", "1.2.3", ResourceId = 841)]
    public class HuntPlugin : RustPlugin
    {
        private readonly HuntRPG HuntRPGInstance;
        private bool ServerInitialized;
        private bool UpdateConfig;
        private bool UpdatePlayerData;
        private DynamicConfigFile HuntDataFile;

        public HuntPlugin()
        {
            HasConfig = true;
            HuntRPGInstance = new HuntRPG(this);
        }

        protected override void LoadDefaultConfig()
        {
            UpdateConfig = true;
            DefaultConfig();
        }

        private void DefaultConfig()
        {
            if (!ServerInitialized && UpdateConfig)
            {
                //this will only be called if there is not a config file, or it needs updating
                Config[HK.ConfigVersion] = Version;
                Config[HK.XPTable] = HuntTablesGenerator.GenerateXPTable(HK.MaxLevel, HK.BaseXP, HK.LevelMultiplier, HK.LevelModule, HK.ModuleReducer);
                Config[HK.MessagesTable] = HuntTablesGenerator.GenerateMessageTable();
                Config[HK.SkillTable] = HuntTablesGenerator.GenerateSkillTable();
                SaveConfig();
            }
            else
            {
                //this will be called only on serverinit if the config needs updating
                Config[HK.ItemTable] = HuntTablesGenerator.GenerateItemTable();
                SaveConfig();
            }

            if (!UpdatePlayerData || !UpdateConfig) return;
            // this will only be called if this version requires a data wipe and the config is outdated.
            LogToConsole("This version needs a wipe to data file.");
            LogToConsole("Dont worry levels will be kept! =]");
            LogToConsole("Doing that now...");
            LoadRPG(false);
            var profiles = new Dictionary<string, RPGInfo>(ReadFromData<Dictionary<string, RPGInfo>>(HK.Profile));
            var rpgInfos = new Dictionary<string, RPGInfo>();
            foreach (var profile in profiles)
            {
                var steamId = profile.Key;
                var player = BasePlayer.FindByID(Convert.ToUInt64(steamId)) ??
                             BasePlayer.FindSleeping(Convert.ToUInt64(steamId));
                var rpgInfo = new RPGInfo(player.displayName);
                rpgInfos.Add(steamId, rpgInfo);
                HuntRPGInstance.LevelUpPlayer(rpgInfo, profile.Value.Level);
            }
            LogToConsole("Data file updated!");
            SaveRPG(rpgInfos, new Dictionary<string, string>());
            UpdatePlayerData = false;
        }

        private void LoadRPG(bool showMsgs = true)
        {
            LoadConfig();
            if (showMsgs)
                LogToConsole("Loading plugin data and config...");
            HuntDataFile = Interface.GetMod().DataFileSystem.GetDatafile(HK.DataFileName);
            var rpgConfig = ReadFromData<Dictionary<string, RPGInfo>>(HK.Profile);
            if (showMsgs)
                LogToConsole(String.Format("{0} profiles loaded", rpgConfig.Count));
            var playerFurnaces = ReadFromData<Dictionary<string, string>>(HK.Furnaces);
            if (showMsgs)
                LogToConsole(String.Format("{0} furnaces loaded", playerFurnaces.Count));
            var xpTable = ReadFromConfig<Dictionary<int, long>>(HK.XPTable);
            var messagesTable = ReadFromConfig<PluginMessagesConfig>(HK.MessagesTable);
            var skillTable = ReadFromConfig<Dictionary<string, Skill>>(HK.SkillTable);
            var itemTable = ReadFromConfig<Dictionary<string, ItemInfo>>(HK.ItemTable);
            HuntRPGInstance.ConfigRPG(messagesTable, xpTable, skillTable, itemTable, rpgConfig, playerFurnaces);
            if (showMsgs)
                LogToConsole("Data and config loaded!");
        }

        public T ReadFromConfig<T>(string configKey)
        {
            string serializeObject = JsonConvert.SerializeObject(Config[configKey]);
            return JsonConvert.DeserializeObject<T>(serializeObject);
        }

        public T ReadFromData<T>(string dataKey)
        {
            string serializeObject = JsonConvert.SerializeObject(HuntDataFile[dataKey]);
            return JsonConvert.DeserializeObject<T>(serializeObject);
        }

        public void SaveRPG(Dictionary<string, RPGInfo> rpgConfig, Dictionary<string, string> playersFurnaces, bool showMsgs = true)
        {
            if (showMsgs)
                LogToConsole("Data being saved...");
            HuntDataFile[HK.Profile] = rpgConfig;
            HuntDataFile[HK.Furnaces] = playersFurnaces;
            Interface.GetMod().DataFileSystem.SaveDatafile(HK.DataFileName);
            if (!showMsgs) return;
            LogToConsole(String.Format("{0} profiles saved", rpgConfig.Count));
            LogToConsole(String.Format("{0} furnaces saved", playersFurnaces.Count));
            LogToConsole("Data was saved successfully!");
        }

        [HookMethod("Init")]
        void Init()
        {
            LogToConsole(HuntRPGInstance == null ? "Problem initializating RPG Instance!" : "Hunt RPG initialized!");
        }

        [HookMethod("OnServerInitialized")]
        void OnServerInitialized()
        {
            ServerInitialized = true;
            DefaultConfig();
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
            var configVersion = new VersionNumber();
            if (Config[HK.ConfigVersion] != null)
                configVersion = ReadFromConfig<VersionNumber>(HK.ConfigVersion);
            if (Version.Equals(configVersion))
            {
                PrintToChat("<color=lightblue>Hunt</color>: RPG Loaded!");
                PrintToChat("<color=lightblue>Hunt</color>: To see the Hunt RPG help type \"/hunt\" or \"/h\"");
                return;
            }
            LogToConsole("Your config needs updating...Doing it now.");
            Config.Clear();
            UpdateConfig = true;
            UpdatePlayerData = true;
            var wasUpdated = UpdatePlayerData;
            DefaultConfig();
            LogToConsole("Config updated!");
            foreach (var player in BasePlayer.activePlayerList)
                HuntRPGInstance.PlayerInit(player, wasUpdated);
        }

        [HookMethod("OnPlayerInit")]
        void OnPlayerInit(BasePlayer player)
        {
            HuntRPGInstance.PlayerInit(player, UpdatePlayerData);
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

        [HookMethod("OnItemDeployed")]
        void OnItemDeployed(Deployer deployer, BaseEntity baseEntity)
        {
            HuntRPGInstance.OnDeployItem(deployer, baseEntity);
        }

        [HookMethod("OnConsumeFuel")]
        void OnConsumeFuel(BaseOven oven,Item fuel, ItemModBurnable burnable)
        {
            HuntRPGInstance.OnConsumeFuel(oven, fuel, burnable);
        }

        [HookMethod("OnBuildingBlockDoUpgradeToGrade")]
        object OnBuildingBlockUpgrade(BuildingBlock buildingBlock, BaseEntity.RPCMessage message, BuildingGrade.Enum grade)
        {
            HuntRPGInstance.OnBuildingBlockUpgrade(message.player, buildingBlock, grade);
            return null;
        }


        [ChatCommand("h")]
        void cmdHuntShortcut(BasePlayer player, string command, string[] args)
        {
            cmdHunt(player, command, args);
        }

        [ChatCommand("hunt")]
        void cmdHunt(BasePlayer player, string command, string[] args)
        {
            HuntRPGInstance.HandleChatCommand(player, args);
        }

        [ConsoleCommand("hunt.saverpg")]
        private void cmdSaveRPG(ConsoleSystem.Arg arg)
        {
            if (!arg.CheckPermissions()) return;
            HuntRPGInstance.SaveRPG();
        }

        [ConsoleCommand("hunt.resetrpg")]
        private void cmdResetRPG(ConsoleSystem.Arg arg)
        {
            if (!arg.CheckPermissions()) return;
            HuntRPGInstance.ResetRPG();
        }

        [ConsoleCommand("hunt.genxptable")]
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

        public void LogToConsole(string message)
        {
            Puts(String.Format("Hunt: {0}",message));
        }

    }
}