using System;
using System.Collections.Generic;
using System.Text;
using Hunt.RPG.Keys;

namespace Hunt.RPG
{
    public static class RPGHelper
    {
        public static string SteamId(BasePlayer player)
        {
            return player.userID.ToString();
        }

        public static void SkillInfo(StringBuilder sb, Skill skill)
        {
            sb.AppendLine(String.Format("{0} - Required Level: {1}", RPGHelper.WrapInColor(skill.Name, OC.LightBlue), skill.RequiredLevel));
            if (skill.SkillpointsPerLevel > 1)
                sb.AppendLine(String.Format("Each skill level costs {0} skillpoints",
                    skill.SkillpointsPerLevel));

            if (skill.RequiredStats.Count > 0)
            {
                StringBuilder sbs = new StringBuilder();
                foreach (var requiredStat in skill.RequiredStats)
                    sbs.Append(String.Format("{0}: {1} |", requiredStat.Key, requiredStat.Value));
                sb.AppendLine(String.Format("Required stats: {0}", sbs));
            }
            sb.AppendLine(String.Format("{0}", skill.Description));
            if (skill.Usage != null)
                sb.AppendLine(String.Format("{0}{1}",RPGHelper.WrapInColor("Usage: ", OC.Teal) ,skill.Usage));
            sb.AppendLine("-----------------");
        }

        public static string WrapInColor(string msg, string color=OC.Orange)
        {
            return String.Format("<color={1}>{0}</color>", msg, color);
        }

        public static float GetEvasion(RPGInfo rpgInfo)
        {
            var evasion = (float) (rpgInfo.Agility/HRK.MaxEvasion);
            return evasion;
        }

        public static float GetMaxHealth(RPGInfo rpgInfo)
        {
            var healthMultiplier = (float) (rpgInfo.Strength/HRK.MaxHealth);
            var extraHealht = rpgInfo.Strength * healthMultiplier;
            return 100 + extraHealht;
        }

        public static float GetCraftingReducer(RPGInfo rpgInfo)
        {
            return rpgInfo.Intelligence /HRK.MaxCraftingTimeReducer;
        }

        public static string TimeLeft(float availableAt, float time)
        {
            var timeLeft = availableAt - time;
            var formatableTime = new DateTime(TimeSpan.FromSeconds(timeLeft).Ticks);
            var formatedTimeLeft = String.Format("{0:mm\\:ss}", formatableTime);
            return formatedTimeLeft;
        }

        public static bool IsSkillReady(Dictionary<string, float> playerCooldowns, ref float availableAt, float time, string skillKey)
        {
            bool isReady;
            if (playerCooldowns.ContainsKey(skillKey))
            {
                availableAt = playerCooldowns[skillKey];
                isReady = time >= availableAt;
            }
            else
            {
                isReady = true;
                playerCooldowns.Add(skillKey, availableAt);
            }
            return isReady;
        }

        public static string OvenId(BaseOven oven)
        {
            var position = oven.transform.position;
            return String.Format("X{0}Y{1}Z{2}", position.x, position.y, position.z);
        }
    }
}
