using System;
using System.Collections.Generic;
using Hunt.RPG.Keys;

namespace Hunt.RPG
{

    public class SkillMethods
    {
        const string IncorrectNumberOfParameters = "Incorrect number of parameters";

        public static int GatherModifier(int skillpoints, int levelmodule, int itemamount)
        {
            var baseMultiplier = (float)skillpoints / (float) levelmodule;
            baseMultiplier += 1;
            float newAmount = (float) (baseMultiplier * (float) itemamount);
            return (int)Math.Ceiling(newAmount);
        }
        
        public static float CooldownModifier(int skillpoints, int basecooldown, int levelmodule, float currenttime)
        {
            float baseCooldown = basecooldown* 60;
            float timeToReduce = ((skillpoints - 1) * levelmodule) * 60;
            float finalCooldown = baseCooldown - timeToReduce;
            return finalCooldown + currenttime;
        }
    }
}