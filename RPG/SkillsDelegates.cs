using System;
using System.Collections.Generic;
using Oxide.Ext.Hunt.RPG.Keys;

namespace Oxide.Ext.Hunt.RPG
{
    static class SkillsDelegates
    {
        const string IncorrectNumberOfParameters = "Incorrect number of parameters";

        public static Dictionary<string, Delegate> ModifiersDict = new Dictionary<string, Delegate>() { { HRK.GatherModifier, GatherModifier } };

        // 0 - SkillPoints| 1 - LevelModule | 2 - item.amount
        static readonly Handler<int, int[]> GatherModifier = delegate(int[] args)
        {
            if (args.Length != 3)
                throw new ArgumentException(IncorrectNumberOfParameters);
            var baseMultiplier = (float)args[0] / args[1];
            baseMultiplier += 1;
            float newAmount = baseMultiplier * args[2];
            return (int)Math.Ceiling(newAmount);
        };
    }
}
