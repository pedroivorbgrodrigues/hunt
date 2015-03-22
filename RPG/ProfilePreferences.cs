using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunt.RPG
{
    public class ProfilePreferences
    {
        public ProfilePreferences()
        {
            ShowXPMessagePercent = 0.01f;
            ShowCraftMessage = true;
            UseBlinkArrow = true;
            AutoToggleBlinkArrow = true;
        }

        public float ShowXPMessagePercent { get; set; }
        public bool ShowCraftMessage { get; set; }
        public bool UseBlinkArrow { get; set; }
        public bool AutoToggleBlinkArrow { get; set; }
    }
}
