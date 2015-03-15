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

        public int AddSkill(string skill, int points, int maxpoints)
        {
            int pointsToAdd = Math.Abs(points);
            if (SkillPoints < pointsToAdd) return 0;
            if (Skills.ContainsKey(skill))
            {
                int existingPoints = Skills[skill];
                if (existingPoints + points > maxpoints)
                    pointsToAdd = maxpoints - existingPoints;
                Skills[skill] += pointsToAdd;
            }
            else
            {
                if (points > maxpoints)
                    pointsToAdd = maxpoints;
                Skills.Add(skill, pointsToAdd);
            }
            SkillPoints -= pointsToAdd;
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

    }
}