using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hunt.RPG.Keys;
using Oxide.Plugins;
using UnityEngine;

namespace Hunt.RPG
{
    class HuntRPG
    {
        private Dictionary<string, RPGInfo> RPGConfig;
        private PluginMessagesConfig MessagesTable;
        private Dictionary<string, Skill> SkillTable;
        private Dictionary<int, long> XPTable;
        private Dictionary<string, string> ItemTable;
        private Dictionary<ItemCategory, int> ResearchTable;
        private Dictionary<string, Dictionary<string,float>> SkillsCooldowns;
        private SkillMethods SkillMethods;
        private readonly HuntPlugin PluginInstance;
        readonly System.Random RandomGenerator = new System.Random();
        const float MaxCraftingTimeReducer = HK.MaxLevel * 7;
        const float MaxEvasion = HK.MaxLevel * 8;
        const float MaxHealth = HK.MaxLevel * 8;

        public HuntRPG(HuntPlugin pluginInstance)
        {
            PluginInstance = pluginInstance;
            SkillsCooldowns = new Dictionary<string, Dictionary<string, float>>();
            SkillMethods = new SkillMethods();
        }

        public void ConfigRPG(PluginMessagesConfig messagesTable, Dictionary<int, long> xpTable, Dictionary<string, Skill> skillTable, Dictionary<string, string> itemTable, Dictionary<string, RPGInfo> rpgConfig)
        {
            MessagesTable = messagesTable;
            XPTable = xpTable;
            SkillTable = skillTable;
            ItemTable = itemTable;
            RPGConfig = rpgConfig;
            ResearchTable = HuntTablesGenerator.GenerateResearchTable();
        }

        private RPGInfo RPGInfo(BasePlayer player)
        {
            string steamId = SteamId(player);
            if (RPGConfig.ContainsKey(steamId)) return RPGConfig[steamId];
            RPGConfig[steamId] = new RPGInfo(player.displayName);
            PluginInstance.SaveRPG(RPGConfig);
            return RPGConfig[steamId];
        }

        private string SteamId(BasePlayer player)
        {
            return player.userID.ToString();
        }

        public void HandleChatCommand(BasePlayer player, string[] args)
        {
            if (args.Length == 0)
            {
                ChatMessage(player, MessagesTable.GetMessage("help"));
                return;
            }
            var rpgInfo = RPGInfo(player);

            switch (args[0].ToLower())
            {
                case "about":
                    ChatMessage(player, MessagesTable.GetMessage(HMK.About));
                    break;
                case "h":
                case "health":
                    ChatMessage(player, CurrentHealth(rpgInfo, player));
                    break;
                case "p":
                case "profile":
                    DisplayProfile(player);
                    break;
                case "sts":
                case "statset":
                    SetStatsCommand(player, args, rpgInfo);
                    break;
                case "sks":
                case "skillset":
                    SetSkillsCommand(player, args, rpgInfo);
                    break;
                case "skilllist":
                    ListSkills(player);
                    break;
                case "lvlup":
                    LevelUp(player, args, rpgInfo);
                    break;
                case "research":
                    ReserachItem(player, args, rpgInfo);
                    break;
                default:
                    ChatMessage(player, MessagesTable.GetMessage("help"));
                    break;
            }
        }

        private void ListSkills(BasePlayer player)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================");
            sb.AppendLine("Availabel Skills:");
            foreach (var skill in SkillTable)
            {
                sb.AppendLine(String.Format("{0} - Required Level: {1}", skill.Key, skill.Value.RequiredLevel));
                sb.AppendLine(String.Format("{0}", skill.Value.Description));
                sb.AppendLine("-----------------");
            }
                
            sb.AppendLine("==================");
            ChatMessage(player, sb.ToString());
        }

        public bool OnAttacked(BasePlayer player, HitInfo hitInfo)
        {
            var baseNpc = hitInfo.Initiator as BaseNPC;
            var basePlayer = hitInfo.Initiator as BasePlayer;
            bool canEvade = baseNpc != null || basePlayer != null && player.userID != basePlayer.userID;
            if (canEvade)
            {
                var randomFloat = Random(0, 1);
                RPGInfo rpgInfo = RPGInfo(player);
                CurrentHealth(rpgInfo, player);
                var evasion = GetEvasion(rpgInfo);
                bool evaded = randomFloat <= evasion;
                if (evaded)
                    ChatMessage(player, "Dodged!");
                return evaded;
            }
            return false;
        }

        private float GetEvasion(RPGInfo rpgInfo)
        {
            var evasion = (float) (rpgInfo.Agility/MaxEvasion);
            return evasion;
        }

        private float GetMaxHealth(RPGInfo rpgInfo)
        {
            var healthMultiplier = (float) (rpgInfo.Strength/MaxHealth);
            var extraHealht = rpgInfo.Strength * healthMultiplier;
            return 100 + extraHealht;
        }

        private static float GetCraftingReducer(RPGInfo rpgInfo)
        {
            return rpgInfo.Intelligence / MaxCraftingTimeReducer;
        }

        double Random(double a, double b)
        {
            return a + RandomGenerator.NextDouble() * (b - a);
        }

        public ItemCraftTask OnItemCraft(ItemCraftTask item)
        {
            BasePlayer player = item.owner;
            var rpgInfo = RPGInfo(player);
            float craftingTime = item.blueprint.time;
            float craftingReducer = GetCraftingReducer(rpgInfo);
            var amountToReduce = (craftingTime*craftingReducer);    
            float reducedCraftingTime = craftingTime - amountToReduce;
            item.endTime = UnityEngine.Time.time + reducedCraftingTime;
            ChatMessage(player, String.Format("Crafting reduced by {0:F}", amountToReduce));
            return item;
        }

        public void OnGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            BasePlayer player = entity.ToPlayer();
            if (player != null)
            {
                var gatherType = dispenser.gatherType;
                RPGInfo rpgInfo = RPGInfo(player);
                int experience = item.amount;
                if (rpgInfo == null) return;
                if (gatherType == ResourceDispenser.GatherType.Tree)
                {
                    if (rpgInfo.Skills.ContainsKey(HRK.LumberJack))
                    {
                        var modifier = SkillTable[HRK.LumberJack].Modifiers[HRK.GatherModifier];
                        int newAmount = SkillMethods.GatherModifier(rpgInfo.Skills[HRK.LumberJack], Convert.ToInt32(modifier.Args[0]), item.amount);
                        item.amount = newAmount;
                    }
                    experience = item.amount;
                }
                if (gatherType == ResourceDispenser.GatherType.Ore)
                {
                    if (rpgInfo.Skills.ContainsKey(HRK.Miner))
                    {
                        var modifier = SkillTable[HRK.Miner].Modifiers[HRK.GatherModifier];
                        int newAmount = SkillMethods.GatherModifier(rpgInfo.Skills[HRK.Miner], Convert.ToInt32(modifier.Args[0]), item.amount);
                        item.amount = newAmount;
                    }
                    experience = (int) ((float)item.amount/3);
                }
                if (gatherType == ResourceDispenser.GatherType.Flesh)
                {
                    if (rpgInfo.Skills.ContainsKey(HRK.Hunter))
                    {
                        var modifier = SkillTable[HRK.Hunter].Modifiers[HRK.GatherModifier];
                        int newAmount = SkillMethods.GatherModifier(rpgInfo.Skills[HRK.Hunter], Convert.ToInt32(modifier.Args[0]), item.amount);
                        item.amount = newAmount;
                    }
                    experience = item.amount * 5;
                }
                if (rpgInfo.AddExperience(experience, RequiredExperience(rpgInfo.Level)))
                {
                    ChatMessage(player, String.Format("<color=yellow>Level Up! You are now level {0}</color>", rpgInfo.Level));
                    DisplayProfile(player);
                    PluginInstance.SaveRPG(RPGConfig);
                }
                else
                    ChatMessage(player, String.Format("<color=teal>+{0}XP</color> | {1}", experience, XPProgression(rpgInfo)));
            }
        }

        private long RequiredExperience(int level)
        {
            return XPTable[level];
        }

        public string XPProgression(RPGInfo rpgInfo)
        {
            float percent = (float)((float)(rpgInfo.Experience)/(float)(RequiredExperience(rpgInfo.Level)));
            return String.Format("XP: {0:P}", percent);
        }

        public string Profile(RPGInfo rpgInfo, BasePlayer player)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(String.Format("========{0}========", rpgInfo.SteamName));
            sb.AppendLine(String.Format("Level: {0}", rpgInfo.Level));
            sb.AppendLine(CurrentHealth(rpgInfo, player));
            sb.AppendLine(String.Format("Evasion Chance: {0:P}", GetEvasion(rpgInfo)));
            sb.AppendLine(String.Format("Crafting Reducer: {0:P}", GetCraftingReducer(rpgInfo)));
            sb.AppendLine(XPProgression(rpgInfo));
            sb.Append(String.Format("<color={0}>Agi: {1}</color> | ","green", rpgInfo.Agility));
            sb.Append(String.Format("<color={0}>Str: {1}</color> | ", "red", rpgInfo.Strength));
            sb.Append(String.Format("<color={0}>Int: {1}</color>", "blue", rpgInfo.Intelligence));
            sb.AppendLine();
            sb.AppendLine(String.Format("Stats Points: {0}", rpgInfo.StatsPoints));
            sb.AppendLine(String.Format("Skill Points: {0}", rpgInfo.SkillPoints));
            sb.AppendLine(String.Format("========<color={0}>Skills</color>========", "purple"));
            foreach (var skill in rpgInfo.Skills)
                sb.AppendLine(String.Format("{0}: {1}/{2}", skill.Key, skill.Value, SkillTable[skill.Key].MaxPoints));
            sb.AppendLine("====================");
            return sb.ToString();
        }

        private string CurrentHealth(RPGInfo rpgInfo, BasePlayer player)
        {
            return String.Format("Health: {0}/{1}", player.Health(), GetMaxHealth(rpgInfo));
        }

        private void ChatMessage(BasePlayer player, IEnumerable<string> messages)
        {
            foreach (string message in messages)
                ChatMessage(player, message);
        }

        private void ChatMessage(BasePlayer player, string message)
        {
            player.ChatMessage(string.Format("<color={0}>{1}</color>: {2}", MessagesTable.ChatPrefixColor, MessagesTable.ChatPrefix, message));
        }

        public bool ReserachItem(BasePlayer player, string[] args, RPGInfo rpgInfo)
        {
            int commandArgs = args.Length - 1;
            if (commandArgs != 1)
            {
                InvalidCommand(player, args);
                return false;
            }
            
            if (!rpgInfo.Skills.ContainsKey(HRK.Researcher))
            {
                ChatMessage(player, MessagesTable.GetMessage(HMK.SkillNotLearned));
                return false;
            }
            var playerResearchPoints = rpgInfo.Skills[HRK.Researcher];
            var itemname = args[1];
            itemname = itemname.ToLower();
            if (ItemTable.ContainsKey(itemname))
                itemname = ItemTable[itemname];
            var definition = ItemManager.FindItemDefinition(itemname);
            if (definition == null)
            {
                ChatMessage(player, MessagesTable.GetMessage(HMK.ItemNotFound, new[] { itemname }));
                return false;
            }
            var playerContainer = player.inventory.containerMain;
            var hasItem = player.inventory.AllItems().Any(item => item.info.shortname.Equals(itemname));
            if (!hasItem)
            {
                ChatMessage(player, String.Format("In order to research an item you must have it on your inventory"));
                return false;
            }
            if (!ResearchTable.ContainsKey(definition.category))
            {
                ChatMessage(player, "You can research itens of this type");
                return false;
            }
            var requiredSkillPoints = ResearchTable[definition.category];
            if (playerResearchPoints < requiredSkillPoints)
            {
                ChatMessage(player, String.Format("Your research skills are not hight enought. Required {0}", requiredSkillPoints));
                return false;
            }

            var steamId = SteamId(player);
            if (!SkillsCooldowns.ContainsKey(steamId))
                SkillsCooldowns.Add(steamId, new Dictionary<string, float>());
            Dictionary<string, float> playerCooldowns = SkillsCooldowns[steamId];
            float availableAt = 0;
            var time = UnityEngine.Time.time;
            bool isReady = false;
            if (playerCooldowns.ContainsKey(HRK.Researcher))
            {
                availableAt = playerCooldowns[HRK.Researcher];
                isReady = time >= availableAt;
            }
            else
            {
                isReady = true;
                playerCooldowns.Add(HRK.Researcher, availableAt);
            }
            if (isReady)
            {
                var random = Random(0, 1);
                if (random > 0.6)
                {
                    ChatMessage(player, String.Format("You managed to reverse enginier the {0}. The blueprint its on your inventory", definition.displayName.translated));
                    player.inventory.GiveItem(ItemManager.CreateByItemID(definition.itemid, 1, true), playerContainer);
                }
                else
                {
                    ChatMessage(player, String.Format("OPS! While you were trying to research the {0} you accidently broke it.", definition.displayName.translated));
                    var itemInstance = player.inventory.FindItemID(definition.itemid);
                    player.inventory.Take(new List<Item> { itemInstance }, definition.itemid, 1);
                }
                var modifier = SkillTable[HRK.Researcher].Modifiers[HRK.CooldownModifier];
                
                availableAt = SkillMethods.CooldownModifier(rpgInfo.Skills[HRK.Researcher], Convert.ToInt32(modifier.Args[0]), Convert.ToInt32(modifier.Args[1]), time);
                playerCooldowns[HRK.Researcher] = availableAt;
            }
            else
            {
                var timeLeft = availableAt - time;
                var formatableTime = new DateTime(TimeSpan.FromSeconds(timeLeft).Ticks);
                ChatMessage(player, String.Format("You have tried this moments ago, give it a rest. Time left to research again: {0:mm\\:ss}",formatableTime));
            }
            return true;
        }

        private void SetSkillsCommand(BasePlayer player, string[] args, RPGInfo rpgInfo)
        {
            int commandArgs = args.Length - 1;
            if (args.Length < 3 || (commandArgs % 2) != 0)
                InvalidCommand(player, args);
            else
            {
                var pointsSpent = new List<string>();
                int pairs = (commandArgs / 2) + 1;
                for (int i = 1; i < pairs; i++)
                {
                    int index = i * 2 - 1;
                    string skillKey = args[index];
                    int points;
                    if (!Int32.TryParse(args[index + 1], out points))
                    {
                        InvalidCommand(player, args);
                        continue;
                    }

                    if (SkillTable.ContainsKey(skillKey))
                    {
                        var pointsAdded = rpgInfo.AddSkill(skillKey, points, SkillTable[skillKey].MaxPoints);
                        if (pointsAdded!=0)
                            pointsSpent.Add(String.Format("<color={0}>{1}: +{2}</color>", "purple", skillKey, pointsAdded));
                        else
                            pointsSpent.AddRange(MessagesTable.GetMessage(HMK.NotEnoughtPoints));
                    }
                    else
                        pointsSpent.AddRange(MessagesTable.GetMessage(HMK.InvalidSkillName));
                }
                ChatMessage(player, pointsSpent);
            }
        }

        private void SetStatsCommand(BasePlayer player, string[] args, RPGInfo rpgInfo)
        {
            int commandArgs = args.Length - 1;
            if (args.Length < 3 || (commandArgs%2) != 0)
                InvalidCommand(player, args);
            else
            {
                var pointsSpent = new List<string>();
                int pairs = (commandArgs/2) + 1;
                for (int i = 1; i < pairs; i++)
                {
                    int index = i*2 - 1;
                    int points;
                    if (!Int32.TryParse(args[index + 1], out points))
                    {
                        InvalidCommand(player, args);
                        continue;
                    }

                    switch (args[index].ToLower())
                    {
                        case "agi":
                            if (rpgInfo.AddAgi(points))
                                pointsSpent.Add(String.Format("<color={0}>Agi: +{1}</color>", "green", points));
                            else
                                pointsSpent.AddRange(MessagesTable.GetMessage(HMK.NotEnoughtPoints));
                            break;
                        case "str":
                            if (rpgInfo.AddStr(points))
                            {
                                SetMaxHealth(player);
                                pointsSpent.Add(String.Format("<color={0}>Str: +{1}</color>", "red", points));
                            }
                            else
                                pointsSpent.AddRange(MessagesTable.GetMessage(HMK.NotEnoughtPoints));
                            break;
                        case "int":
                            if (rpgInfo.AddInt(points))
                                pointsSpent.Add(String.Format("<color={0}>Int: +{1}</color>", "blue", points));
                            else
                                pointsSpent.AddRange(MessagesTable.GetMessage(HMK.NotEnoughtPoints));
                            break;
                        default:
                            InvalidCommand(player, args);
                            break;
                    }
                }
                ChatMessage(player, pointsSpent);
            }
        }

        private void LevelUp(BasePlayer player, string[] args, RPGInfo rpgInfo)
        {
            if (!player.IsAdmin()) return;
            int commandArgs = args.Length - 1;
            if (commandArgs != 1)
                InvalidCommand(player, args);
            else
            {
                int desiredLevel;
                if (!Int32.TryParse(args[1], out desiredLevel)) return;
                if (desiredLevel <= rpgInfo.Level) return;
                var levelsToUp = desiredLevel - rpgInfo.Level;
                for (int i = 0; i < levelsToUp; i++)
                {
                    long requiredXP = RequiredExperience(rpgInfo.Level);
                    rpgInfo.AddExperience(requiredXP, requiredXP);
                }
            }
        }

        private void InvalidCommand(BasePlayer player, string[] args)
        {
            ChatMessage(player, MessagesTable.GetMessage(HMK.InvalidCommand, new[] {args[0]}));
        }

        public void ResetRPG()
        {
            RPGConfig.Clear();
            PluginInstance.SaveRPG(RPGConfig);
        }

        public void SaveRPG()
        {
            PluginInstance.SaveRPG(RPGConfig);
        }

        public void DisplayProfile(BasePlayer player)
        {
            ChatMessage(player, Profile(RPGInfo(player), player));
        }

        public void OnDeath(BasePlayer player)
        {
            RPGInfo(player).Died();
        }

        public void InitPlayer(BasePlayer player)
        {
            SetMaxHealth(player);
            DisplayProfile(player);
        }

        private void SetMaxHealth(BasePlayer player)
        {
            var typeOf = typeof (BaseCombatEntity);
            var myFieldInfo = typeOf.GetField("_maxHealth", BindingFlags.NonPublic | BindingFlags.Instance);
            if (myFieldInfo != null)
            {
                myFieldInfo.SetValue(player, GetMaxHealth(RPGInfo(player)));
            }
        }

        public void OnEntityBuilt(Planner planner, GameObject gameobject)
        {
        }
    }
}