using System.Collections.Generic;
using System.Linq;
using Hunt.RPG.Keys;

namespace Hunt.RPG
{
    public static class HuntTablesGenerator
    {
        public static Dictionary<int, long> GenerateXPTable(int maxLevel, int baseExp, float levelMultiplier, int levelModule, float moduleReducer)
        {
            var xpTable = new Dictionary<int, long>();
            long previousLevel = baseExp;
            xpTable.Add(0, baseExp);
            for (int i = 0; i < maxLevel; i++)
            {
                if (i%levelModule == 0)
                    levelMultiplier -= moduleReducer;
                long levelRequiredXp = (long)(previousLevel * levelMultiplier);
                xpTable.Add(i+1, levelRequiredXp);
                previousLevel = levelRequiredXp;
            }
            return xpTable;
        }

        public static PluginMessagesConfig GenerateMessageTable()
        {
            var messagesConfig = new PluginMessagesConfig("Hunt", "lightblue");
            messagesConfig.AddMessage("help", new List<string>
            {
                "To get an overview about the Hunt RPG, type \"/hunt about\"",
                "To see you available shortcuts commdands, type \"/hunt shortcuts\"",
                "To see you player profile, type \"/hunt profile\"",
                "To see the skill list type \"/hunt skilllist\"",
                "To spend your available stats points, type \"/hunt statset <stats> <points> \". Ex: /hunt statset agi 3",
                "To spend your available skill points, type \"/hunt skillset <skillname> <points> \". Ex: /hunt skillset lumberjack 1",
            });
            messagesConfig.AddMessage("shortcuts", new List<string>
            {
                "\"/hunt\" = \"/h\"",
                "\"/hunt profile\" = \"/h p\"",
                "\"/hunt statset\" = \"/h sts\"",
                "\"/hunt skillset\" = \"/h sks\"",
            });

            messagesConfig.AddMessage(HMK.InvalidCommand, "You ran the \"{0}\" command incorrectly. Type \"/hunt help\" to get help");
            messagesConfig.AddMessage(HMK.NotEnoughtPoints, "You don't have enought points to set!");
            messagesConfig.AddMessage(HMK.InvalidSkillName, "There is no such skill! Type \"/hunt skilllist\" to see the available skills");
            messagesConfig.AddMessage(HMK.ItemNotFound, "Item {0} not found.");
            messagesConfig.AddMessage(HMK.SkillNotLearned, "You havent learned this skill yet.");
            return messagesConfig;
        }



        public static Dictionary<string, Skill> GenerateSkillTable()
        {
            var skillTable = new Dictionary<string, Skill>();
            var lumberJack = new Skill(HRK.LumberJack, "This skill allows you to gather wood faster. Each point gives you 10% more wood per hit.", 0, 20);
            var woodAndFleshModifier = new Modifier(HRK.GatherModifier, new List<object>(){10});
            lumberJack.AddModifier(HRK.GatherModifier, woodAndFleshModifier);
            skillTable.Add(HRK.LumberJack, lumberJack);
            var miner = new Skill(HRK.Miner, "This skill allows you to gather stones faster. Each point gives you 5% more stones per hit.", 0, 20);
            miner.AddModifier(HRK.GatherModifier, new Modifier(HRK.GatherModifier, new List<object>(){5}));
            skillTable.Add(HRK.Miner, miner);
            var hunter = new Skill(HRK.Hunter, "This skill allows you to gather resources faster from animals. Each point gives you 10% more resources per hit.", 0, 20);
            hunter.AddModifier(HRK.GatherModifier, woodAndFleshModifier);
            skillTable.Add(HRK.Hunter, hunter);
            skillTable.Add(HRK.Researcher, new Skill(HRK.Researcher, "This skill allows you to research items you have. Each level enables a type of type to be researched. Ex: Level 1 - Tools",30, 5));
            return skillTable;
        }

        public static Dictionary<string, string> GenerateItemTable()
        {
            var itemsDefinition = ItemManager.GetItemDefinitions();
            return itemsDefinition.ToDictionary(itemdef => itemdef.displayName.translated.ToLower(), itemdef => itemdef.shortname);
        }

        public static Dictionary<ItemCategory, int> GenerateResearchTable()
        {
            var researchTable = new Dictionary<ItemCategory, int>
            {
                {ItemCategory.Tool, 1},
                {ItemCategory.Attire, 2},
                {ItemCategory.Construction, 3},
                {ItemCategory.Resources, 4},
                {ItemCategory.Medical, 4},
                {ItemCategory.Ammunition, 4},
                {ItemCategory.Weapon, 5}
            };
            return researchTable;
        }
    }
}