using System.Collections.Generic;
using System.IO;
using Hunt.RPG;
using Hunt.RPG.Keys;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Hunt RPG", "PedraozauM / SW", "1.2.0", ResourceId = 841)]
    public class HuntPlugin : RustPlugin
    {
        private HuntRPG HuntRPGInstance;
        private bool ServerInitialized;
        private bool GenerateNewConfig;
        private DynamicConfigFile HuntDataFile;

        public HuntPlugin()
        {
            HasConfig = true;
        }

        protected override void LoadDefaultConfig()
        {
            GenerateNewConfig = true;
            DefaultConfig();
        }

        private void DefaultConfig()
        {
            if (!ServerInitialized)
            {
                Config[HK.XPTable] = HuntTablesGenerator.GenerateXPTable(HK.MaxLevel, HK.BaseXP, HK.LevelMultiplier, HK.LevelModule, HK.ModuleReducer);
                Config[HK.MessagesTable] = HuntTablesGenerator.GenerateMessageTable();
                Config[HK.SkillTable] = HuntTablesGenerator.GenerateSkillTable();
            }
            else
            {
                if (!GenerateNewConfig) return;
                Puts("Item Table generated");
                Config[HK.ItemTable] = HuntTablesGenerator.GenerateItemTable();
                HuntDataFile = Interface.GetMod().DataFileSystem.GetDatafile(HK.DataFileName);
                HuntDataFile[HK.Profile] = new Dictionary<string, RPGInfo>();
                HuntDataFile[HK.Furnaces] = new Dictionary<string, string>();
                Interface.GetMod().DataFileSystem.SaveDatafile(HK.DataFileName);
                SaveConfig();
            }
                
        }

        private void LoadRPG()
        {
            LoadConfig();
            HuntDataFile = Interface.GetMod().DataFileSystem.GetDatafile(HK.DataFileName);
            var rpgConfig = ReadFromData<Dictionary<string, RPGInfo>>(HK.Profile);
            Puts("{0} profiles loaded", rpgConfig.Count.ToString());
            var playerFurnaces = ReadFromData<Dictionary<string, string>>(HK.Furnaces);
            Puts("{0} furnaces loaded", rpgConfig.Count.ToString());
            var xpTable = ReadFromConfig<Dictionary<int, long>>(HK.XPTable);
            var messagesTable = ReadFromConfig<PluginMessagesConfig>(HK.MessagesTable);
            var skillTable = ReadFromConfig<Dictionary<string, Skill>>(HK.SkillTable);
            var itemTable = ReadFromConfig<Dictionary<string, string>>(HK.ItemTable);
            HuntRPGInstance.ConfigRPG(messagesTable, xpTable, skillTable, itemTable, rpgConfig, playerFurnaces);
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

        public void SaveRPG(Dictionary<string, RPGInfo> rpgConfig, Dictionary<string, string> playersFurnaces)
        {
            HuntDataFile[HK.Profile] = rpgConfig;
            HuntDataFile[HK.Furnaces] = playersFurnaces;
            Puts("{0} profiles saved", rpgConfig.Count.ToString());
            Puts("{0} furnaces saved", playersFurnaces.Count.ToString());
            Interface.GetMod().DataFileSystem.SaveDatafile(HK.DataFileName);

        }

        [HookMethod("Init")]
        void Init()
        {
            Puts("Hunt initialized!");
            HuntRPGInstance = new HuntRPG(this);
            if (HuntRPGInstance == null)
                Puts("Problem initializating RPG Instance!");
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
        }

        [HookMethod("OnPlayerInit")]
        void OnPlayerInit(BasePlayer player)
        {
            HuntRPGInstance.PlayerInit(player);
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

        [HookMethod("OnBuildingBlockUpgrade")]
        object OnBuildingBlockUpgrade(BuildingBlock buildingBlock, BuildingGrade.Enum grade, BaseEntity.RPCMessage message)
        {
            HuntRPGInstance.OnBuildingBlockUpgrade(message.player, buildingBlock, grade);
            return grade;
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
            Puts(message);
        }

    }
}