using System;
using System.Collections;
using System.Collections.Generic;

namespace Hunt.RPG
{
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
            RequiredStats = new Dictionary<string, int>();
            SkillpointsPerLevel = 1;
        }

        public void AddRequiredStat(string stat, int points)
        {
            if(!RequiredStats.ContainsKey(stat))
                RequiredStats.Add(stat, points);
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
        public Dictionary<string, Modifier> Modifiers { get; set;}
        public Dictionary<string,int> RequiredStats { get; set; }
        public int SkillpointsPerLevel { get; set; }
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