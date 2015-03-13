using System.Collections.Generic;

namespace Oxide.Ext.Hunt.RPG
{
    delegate TResult Handler<out TResult, in TParams>(TParams args);
    public class Skill
    {
        public Skill(string name, string description, int requiredLevel, int maxPoints)
        {
            Name = name;
            Description = description;
            RequiredLevel = requiredLevel;
            MaxPoints = maxPoints;
            RequiredSkills = new Dictionary<string, int>();
            Modifiers = new Dictionary<string, Modifier>();
        }

        public void AddRequiredSkill(string skillName, int pointsNeeded)
        {
            if (!RequiredSkills.ContainsKey(skillName))
                RequiredSkills.Add(skillName, pointsNeeded);
        }

        public void AddModifier(string modifier, Modifier handler)
        {
            if (!Modifiers.ContainsKey(modifier))
                Modifiers.Add(modifier, handler);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredLevel { get; set; }

        public int MaxPoints { get; set; }
        public Dictionary<string,int> RequiredSkills { get; set; }
        public Dictionary<string, Modifier> Modifiers { get; set; }
    }

    public class Modifier
    {
        public Modifier(string identifier, List<object> args)
        {
            Identifier = identifier;
            Args = args;
        }

        public string Identifier { get; set; }

        public List<object> Args { get; set; }
    }
}