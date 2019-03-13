using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thor.Stardew.Mods.HealthBars
{
    /// <summary>
    /// Mod Configration class
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// If true, the life counter needs XP + killcount of mobs to show the life level
        /// If false, always show the life level
        /// </summary>
        public bool EnableXPNeeded { get; set; }
        /// <summary>
        /// Allow selecting the color scheme of the life bar
        /// </summary>
        public int ColorScheme { get; set; }

        /// <summary>
        /// Initialization of default values
        /// </summary>
        public ModConfig()
        {
            EnableXPNeeded = true;
            ColorScheme = 0;
        }
    }
}
