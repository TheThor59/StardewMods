using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thor.Stardew.Mods.HealthBars
{
    /// <summary>
    /// Main class of the mod
    /// </summary>
    public class ModEntry : Mod
    {
        private Texture2D _whitePixel;
        /// <summary>
        /// Contains the configuration of the mod
        /// </summary>
        private ModConfig _config;

        /// <summary>
        /// Available colour schemes of the life bar
        /// </summary>
        private static readonly Color[][] ColourSchemes =
        {
            new Color[] { Color.LawnGreen, Color.YellowGreen, Color.Gold, Color.DarkOrange, Color.Crimson },
            new Color[] { Color.Crimson, Color.DarkOrange, Color.Gold, Color.YellowGreen, Color.LawnGreen },
        };

        /// <summary>
        /// Mod initialization method
        /// </summary>
        /// <param name="helper">helper provided by SMAPI</param>
        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            EnsureCorrectConfig();
            helper.Events.Display.RenderedWorld += RenderLifeBars;
        }

        /// <summary>
        /// Method that ensure the configuration provided by user is correct and will not break the game
        /// </summary>
        private void EnsureCorrectConfig()
        {
            bool needUpdateConfig = false;
            if (_config.ColorScheme >= ColourSchemes.Length || _config.ColorScheme < 0)
            {
                _config.ColorScheme = 0;
                needUpdateConfig = true;
            }

            if (needUpdateConfig)
            {
                Helper.WriteConfig(_config);
            }
        }

        /// <summary>
        /// Handle the rendering of mobs life bars
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void RenderLifeBars(object sender, RenderedWorldEventArgs e)
        {
            if (Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.menuUp || Game1.activeClickableMenu != null) return;

            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                _whitePixel.SetData(new Color[] { Color.White });
            }

            // Iterate through all NPC
            foreach (NPC character in Game1.currentLocation.characters)
            {
                // We only care about monsters
                if (!(character is Monster))
                {
                    continue;
                }
                Monster monster = (Monster)character;
                // If monster is not visible, next
                if (monster.isInvisible || !Utility.isOnScreen(monster.position, 3 * Game1.tileSize))
                {
                    continue;
                }

                // Get all infos about the monster
                int health = monster.Health;
                int maxHealth = monster.MaxHealth;
                if (health > maxHealth) maxHealth = health;
                
                // If monster has already been killed once by player, we get the number of kills, else it's 0
                int monsterKilledAmount = Game1.stats.specificMonstersKilled.ContainsKey(monster.name) ? Game1.stats.specificMonstersKilled[monster.name] : 0;
                String healthText = "???";

                // By default, color bar is grey
                Color barColor = Color.DarkSlateGray;
                // By default, color bar full
                float barLengthPercent = 1f;

                // If level system is deactivated or the basic level is OK, we display the colours
                if (!_config.EnableXPNeeded || monsterKilledAmount + Game1.player.combatLevel > Globals.EXPERIENCE_BASIC_STATS_LEVEL)
                {
                    float monsterHealthPercent = (float)health / (float)maxHealth;
                    if (monsterHealthPercent > 0.9f) barColor = ColourSchemes[_config.ColorScheme][0];
                    else if (monsterHealthPercent > 0.65f) barColor = ColourSchemes[_config.ColorScheme][1];
                    else if (monsterHealthPercent > 0.35f) barColor = ColourSchemes[_config.ColorScheme][2];
                    else if (monsterHealthPercent > 0.15f) barColor = ColourSchemes[_config.ColorScheme][3];
                    else barColor = ColourSchemes[_config.ColorScheme][4];

                    // If level system is deactivated or the full level is OK, we display the stats
                    if (!_config.EnableXPNeeded || monsterKilledAmount + Game1.player.combatLevel * 4 > Globals.EXPERIENCE_FULL_STATS_LEVEL)
                    {
                        barLengthPercent = monsterHealthPercent;
                        // If it's a very strong monster, we hide the life counter
                        if (_config.EnableXPNeeded && monster.health > 999) healthText = "!!!";
                        else healthText = String.Format("{0:000}", health);
                    }
                }

                // Display the life bar
                GreenSlime slime;
                Rectangle monsterBox;
                Rectangle lifeBox;
                Vector2 monsterLocalPosition;
                monsterLocalPosition = monster.getLocalPosition(Game1.viewport);
                monsterBox = new Rectangle((int)monsterLocalPosition.X, (int)monsterLocalPosition.Y - monster.Sprite.spriteHeight / 2 * Game1.pixelZoom, monster.Sprite.spriteWidth * Game1.pixelZoom, 16);
                if (monster is GreenSlime)
                {
                    slime = (GreenSlime)monster;
                    if (slime.hasSpecialItem)
                    {
                        monsterBox.X -= 5;
                        monsterBox.Width += 10;
                    }
                    else if (slime.cute)
                    {
                        monsterBox.X -= 2;
                        monsterBox.Width += 4;
                    }
                    else
                    {
                        monsterBox.Y += 5 * Game1.pixelZoom;
                    }
                }
                else if (monster is RockCrab || monster is LavaCrab)
                {
                    if (monster.Sprite.CurrentFrame % 4 == 0) continue;
                }
                else if (monster is RockGolem)
                {
                    if (monster.health == monster.maxHealth) continue;
                    monsterBox.Y = (int)monsterLocalPosition.Y - monster.Sprite.spriteHeight * Game1.pixelZoom * 3 / 4;
                }
                else if (monster is Bug)
                {
                    if (((Bug)monster).isArmoredBug) continue;
                    monsterBox.Y -= 15 * Game1.pixelZoom;
                }
                else if (monster is Grub)
                {
                    if (monster.Sprite.CurrentFrame == 19) continue;
                    monsterBox.Y = (int)monsterLocalPosition.Y - monster.Sprite.spriteHeight * Game1.pixelZoom * 4 / 7;
                }
                else if (monster is Fly)
                {
                    monsterBox.Y = (int)monsterLocalPosition.Y - monster.Sprite.spriteHeight * Game1.pixelZoom * 5 / 7;
                }
                else if (monster is DustSpirit)
                {
                    monsterBox.X += 3;
                    monsterBox.Width -= 6;
                    monsterBox.Y += 5 * Game1.pixelZoom;
                }
                else if (monster is Bat)
                {
                    if (monster.Sprite.CurrentFrame == 4) continue;
                    monsterBox.X -= 1;
                    monsterBox.Width -= 2;
                    monsterBox.Y += 1 * Game1.pixelZoom;
                }
                else if (monster is MetalHead || monster is Mummy)
                {
                    monsterBox.Y -= 2 * Game1.pixelZoom;
                }
                else if (monster is Skeleton || monster is ShadowBrute || monster is ShadowShaman || monster is SquidKid)
                {
                    if (monster.health == monster.maxHealth) continue;
                    monsterBox.Y -= 7 * Game1.pixelZoom;
                }
                monsterBox.X = (int)((float)monsterBox.X);
                monsterBox.Y = (int)((float)monsterBox.Y);
                monsterBox.Width = (int)((float)monsterBox.Width);
                monsterBox.Height = (int)((float)monsterBox.Height);
                lifeBox = new Rectangle(monsterBox.X+1, monsterBox.Y+1, monsterBox.Width - 2, monsterBox.Height - 2);
                // Draw life bar border
                Game1.spriteBatch.Draw(_whitePixel, monsterBox, Color.BurlyWood);
                Game1.spriteBatch.Draw(_whitePixel, lifeBox, Color.SaddleBrown);
                lifeBox.Width = (int)((float)lifeBox.Width * barLengthPercent);
                // Draw life bar
                Game1.spriteBatch.Draw(_whitePixel, lifeBox, barColor);
                Color textColor = (barColor == Color.DarkSlateGray || barLengthPercent < 0.35f) ? Color.AntiqueWhite : Color.DarkSlateGray;
                // Draw text
                Utility.drawTextWithShadow(Game1.spriteBatch, healthText, Game1.tinyFont, new Vector2(monsterBox.X + (float)monsterBox.Width / 2 - Game1.tinyFont.MeasureString(healthText).X * Globals.TEXT_SCALE_LEVEL / 2, monsterBox.Y + (float)monsterBox.Height / 2 - Game1.tinyFont.MeasureString(healthText).Y * Globals.TEXT_SCALE_LEVEL / 2), textColor, Globals.TEXT_SCALE_LEVEL, -1, 1, -1, 0.4f, 0);

            }
        }
    }
}
