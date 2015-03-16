using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            messagesConfig.AddMessage(HMK.About, HuntAbout());
            messagesConfig.AddMessage(HMK.InvalidCommand, "You ran the \"{0}\" command incorrectly. Type \"/hunt help\" to get help");
            messagesConfig.AddMessage(HMK.NotEnoughtPoints, "You don't have enought points to set!");
            messagesConfig.AddMessage(HMK.InvalidSkillName, "There is no such skill! Type \"/hunt skilllist\" to see the available skills");
            messagesConfig.AddMessage(HMK.ItemNotFound, "Item {0} not found.");
            messagesConfig.AddMessage(HMK.SkillNotLearned, "You havent learned this skill yet.");
            return messagesConfig;
        }

        private static List<string> HuntAbout()
        {
            var aboutMessages = new List<string>();
            aboutMessages.Add("=================================================");
            aboutMessages.Add("The Hunt RPG system in development.");
            aboutMessages.Add("It is consisted of levels, stats atributes, skills and later on specializations.");
            aboutMessages.Add("Currently there are 3 attributes, each of then give you and specific enhancement.");
            aboutMessages.Add("Strenght gives you more health, it will not be displayed in the Health Bar, but it is considered for healing and getting hurt.");
            aboutMessages.Add("Agillity gives you dodge change");
            aboutMessages.Add("Intelligence decreases your items crafting time");
            aboutMessages.Add("Right now you can level up by gathering resources.");
            aboutMessages.Add("Each level gives you 1 point in each attribute. And 3 more to distribute.");
            aboutMessages.Add("Each level gives you 1 skill point to distribute");
            aboutMessages.Add("Each skill have its required level, and later on it will require specific stats.");
            aboutMessages.Add("To see the all the available skills and its description type \"/hunt skilllist\"");
            aboutMessages.Add("To learn more about Hunt RPG go to the plugin page at <link>");
            aboutMessages.Add("=================================================");
            return aboutMessages;
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
            var description = "This skill allows you to research items you have. Each level enables a type of type to be researched. Table: Level 1 - Tools; Level 2 - Clothes; Level 3 - Construction and Resources; Level 4 - Ammunition and Medic; Level 5 - Weapons";
            var researcher = new Skill(HRK.Researcher, description,30, 5);
            researcher.AddModifier(HRK.CooldownModifier, new Modifier(HRK.CooldownModifier, new List<object>() {10,2}));
            skillTable.Add(HRK.Researcher, researcher);
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
                {ItemCategory.Resources, 3},
                {ItemCategory.Medical, 4},
                {ItemCategory.Ammunition, 4},
                {ItemCategory.Weapon, 5}
            };
            return researchTable;
        }
    }
}