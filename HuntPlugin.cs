using System.Collections.Generic;
using Hunt.RPG;
using Hunt.RPG.Keys;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Hunt RPG", "PedraozauM / SW", "1.0.0", ResourceId = 0)]
    public class HuntPlugin : RustPlugin
    {
        private HuntRPG HuntRPGInstance;
        private bool ServerInitialized;
        private bool GenerateNewConfig;

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
                SaveConfig();
            }
                
        }

        private void LoadRPG()
        {
            LoadConfig();
            Interface.GetMod().DataFileSystem.GetDatafile(HK.DataFileName);
            var rpgConfig = Interface.GetMod().DataFileSystem.ReadObject<Dictionary<string, RPGInfo>>(HK.DataFileName);
            Puts("{0} profiles loaded", new[] { rpgConfig.Count });
            var xpTable = ReadFromConfig<Dictionary<int, long>>(HK.XPTable);
            var messagesTable = ReadFromConfig<PluginMessagesConfig>(HK.MessagesTable);
            var skillTable = ReadFromConfig<Dictionary<string, Skill>>(HK.SkillTable);
            var itemTable = ReadFromConfig<Dictionary<string, string>>(HK.ItemTable);
            HuntRPGInstance.ConfigRPG(messagesTable, xpTable, skillTable, itemTable, rpgConfig);
        }

        public T ReadFromConfig<T>(string configKey)
        {
            string serializeObject = JsonConvert.SerializeObject(Config[configKey]);
            return JsonConvert.DeserializeObject<T>(serializeObject);
        }

        public void SaveRPG(Dictionary<string, RPGInfo> rpgConfig)
        {
            Interface.GetMod().DataFileSystem.WriteObject(HK.DataFileName, rpgConfig);
            Puts("{0} profiles saved", new []{rpgConfig.Count});
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

        [ChatCommand("cmdHuntShortcut")]
        void cmdHuntShortcut(BasePlayer player, string command, string[] args)
        {
            cmdHunt(player, command, args);
        }

        [ChatCommand("cmdHunt")]
        void cmdHunt(BasePlayer player, string command, string[] args)
        {
            HuntRPGInstance.HandleChatCommand(player, args);
        }

        [ConsoleCommand("cmdResetRPG")]
        private void cmdResetRPG(ConsoleSystem.Arg arg)
        {
            if (!arg.CheckPermissions()) return;
            HuntRPGInstance.ResetRPG();
        }

        [ConsoleCommand("cmdGenerateXPTable")]
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