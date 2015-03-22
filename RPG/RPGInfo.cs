using System;
using System.Collections.Generic;
using Hunt.RPG.Keys;

namespace Hunt.RPG
{
    public class RPGInfo
    {
        public RPGInfo(string steamName)
        {
            SteamName = steamName;
            Level = 0;
            Skills = new Dictionary<string, int>();
            Preferences = new ProfilePreferences();
        }

        public bool AddExperience(long xp,long requiredXp)
        {
            Experience += xp;
            if (Experience < requiredXp) return false;
            if (Level == HK.MaxLevel) return false;
            Experience = Experience-requiredXp;
            LevelUp();
            return true;
        }

        public void Died()
        {
            var removedXP = (long)(Experience*HK.DeathReducer);
            Experience -= removedXP;
            if (Experience < 0)
                Experience = 0;
        }

        private void LevelUp()
        {
            Level++;
            Agility++;
            Strength++;
            Intelligence++;
            StatsPoints += 3;
            SkillPoints += 1;
        }

        public bool AddAgi(int points)
        {
            int absPoints = Math.Abs(points);
            if (StatsPoints < absPoints) return false;
            StatsPoints -= absPoints;
            Agility += absPoints;
            return true;
        }

        public bool AddStr(int points)
        {
            int absPoints = Math.Abs(points);
            if (StatsPoints < absPoints) return false;
            StatsPoints -= absPoints;
            Strength += absPoints;
            return true;
        }

        public bool AddInt(int points)
        {
            int absPoints = Math.Abs(points);
            if (StatsPoints < absPoints) return false;
            StatsPoints -= absPoints;
            Intelligence += absPoints;
            return true;
        }

        public int AddSkill(Skill skill, int points, out string reason)
        {
            int pointsToAdd = Math.Abs(points);
            var requiredPoints = pointsToAdd * skill.SkillpointsPerLevel;
            if (SkillPoints < requiredPoints)
            {
                reason = HMK.NotEnoughtPoints;
                return 0;
            }
            if (Level < skill.RequiredLevel)
            {
                reason = HMK.NotEnoughLevels;
                return 0;
            }
            foreach (var requiredStat in skill.RequiredStats)
            {
                switch (requiredStat.Key.ToLower())
                {
                    case "str":
                        if (Strength < requiredStat.Value)
                        {
                            reason = HMK.NotEnoughStrength;
                            return 0;
                        }
                    break;
                    case "agi":
                        if (Agility < requiredStat.Value)
                        {
                            reason = HMK.NotEnoughAgility;
                            return 0;
                        }
                    break;
                    case "int":
                        if (Intelligence < requiredStat.Value)
                        {
                            reason = HMK.NotEnoughIntelligence;
                            return 0;
                        }
                        break;
                }
            }
            if (Skills.ContainsKey(skill.Name))
            {
                int existingPoints = Skills[skill.Name];
                if (existingPoints + points > skill.MaxPoints)
                    pointsToAdd = skill.MaxPoints - existingPoints;
                if(pointsToAdd >  0)
                    Skills[skill.Name] += pointsToAdd;
            }
            else
            {
                if (points > skill.MaxPoints)
                    pointsToAdd = skill.MaxPoints;
                Skills.Add(skill.Name, pointsToAdd);
            }
            
            if (pointsToAdd <= 0)
            {
                reason = HMK.AlreadyAtMaxLevel;
                return 0;
            }
            reason = "";
            SkillPoints -= pointsToAdd * skill.SkillpointsPerLevel;
            return pointsToAdd;
        }

        public string SteamName { get; set; }
        public int Level { get; set; }
        public long Experience { get; set; }
        public int Agility { get; set; }
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int StatsPoints { get; set; }
        public int SkillPoints { get; set; }
        public Dictionary<string,int> Skills { get; set; }

        public ProfilePreferences Preferences { get; set; }

    }
}