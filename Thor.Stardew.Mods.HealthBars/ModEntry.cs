using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class ModEntry : Mod
    {
        private Texture2D _whitePixel;
        private static readonly Color[][] ColourSchemes =
        {
            new Color[] { Color.LawnGreen, Color.YellowGreen, Color.Gold, Color.DarkOrange, Color.Crimson },
            new Color[] { Color.Crimson, Color.DarkOrange, Color.Gold, Color.YellowGreen, Color.LawnGreen },
        };

        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedWorld += DrawTickEvent;
        }
        /// <summary>
        /// Handle button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            Monitor.Log($"{Game1.player.Name} pressed {e.Button}.");
        }

        private void DrawTickEvent(object sender, RenderedWorldEventArgs e)
        {
            if (Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.menuUp || Game1.activeClickableMenu != null) return;

            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                _whitePixel.SetData(new Color[] { Color.White });
            }

            //Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (NPC character in Game1.currentLocation.characters)
            {
                Monster monster;
                GreenSlime slime;
                Rectangle monsterBox;
                Rectangle lifeBox;
                Vector2 monsterLocalPosition;
                Color barColor;
                String healthText;
                float monsterHealthPercent;
                float barLengthPercent;
                int monsterKilledAmount;
                if (character is Monster)
                {
                    monster = (Monster)character;

                    if (!monster.isInvisible && Utility.isOnScreen(monster.position, 3 * Game1.tileSize))
                    {
                        Netcode.NetInt health = monster.health;
                        Netcode.NetInt maxHealth = monster.maxHealth;
                        if (health > maxHealth) maxHealth = health;

                        if (Game1.stats.specificMonstersKilled.ContainsKey(monster.name))
                        {
                            monsterKilledAmount = Game1.stats.specificMonstersKilled[monster.name];
                        }
                        else
                        {
                            monsterKilledAmount = 0;
                        }

                        healthText = "???";
                        if (monsterKilledAmount + Game1.player.combatLevel > 15)
                        {
                            //basic stats
                            monsterHealthPercent = (float)health / (float)maxHealth;
                            barLengthPercent = 1f;
                            if (monsterHealthPercent > 0.9f) barColor = ColourSchemes[0][0];
                            else if (monsterHealthPercent > 0.65f) barColor = ColourSchemes[0][1];
                            else if (monsterHealthPercent > 0.35f) barColor = ColourSchemes[0][2];
                            else if (monsterHealthPercent > 0.15f) barColor = ColourSchemes[0][3];
                            else barColor = ColourSchemes[0][4];

                            if (monsterKilledAmount + Game1.player.combatLevel * 4 > 45)
                            {
                                barLengthPercent = monsterHealthPercent;
                                if (monster.health > 999) healthText = "!!!";
                                else healthText = String.Format("{0:000}", monster.health);
                            }
                        }
                        else
                        {
                            barLengthPercent = 1f;
                            barColor = Color.DarkSlateGray;
                        }

                        monsterLocalPosition = monster.getLocalPosition(Game1.viewport);
                        monsterBox = new Rectangle((int)monsterLocalPosition.X, (int)monsterLocalPosition.Y - monster.Sprite.spriteHeight / 2 * Game1.pixelZoom, monster.Sprite.spriteWidth * Game1.pixelZoom, 12);
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
                        monsterBox.X = (int)((float)monsterBox.X * Game1.options.zoomLevel);
                        monsterBox.Y = (int)((float)monsterBox.Y * Game1.options.zoomLevel);
                        monsterBox.Width = (int)((float)monsterBox.Width * Game1.options.zoomLevel);
                        monsterBox.Height = (int)((float)monsterBox.Height * Game1.options.zoomLevel);
                        lifeBox = monsterBox;
                        ++lifeBox.X;
                        ++lifeBox.Y;
                        lifeBox.Height = monsterBox.Height - 2;
                        lifeBox.Width = monsterBox.Width - 2;
                        Game1.spriteBatch.Draw(_whitePixel, monsterBox, Color.BurlyWood);
                        Game1.spriteBatch.Draw(_whitePixel, lifeBox, Color.SaddleBrown);
                        lifeBox.Width = (int)((float)lifeBox.Width * barLengthPercent);
                        Game1.spriteBatch.Draw(_whitePixel, lifeBox, barColor);
                        if (barColor == Color.DarkSlateGray || barLengthPercent < 0.35f)
                            Utility.drawTextWithShadow(Game1.spriteBatch, healthText, Game1.smallFont, new Vector2(monsterBox.X + (float)monsterBox.Width / 2 - 9 * Game1.options.zoomLevel, monsterBox.Y + 2), Color.AntiqueWhite, Game1.options.zoomLevel * 0.4f, -1, 0, 0, 0, 0);
                        else
                            Utility.drawTextWithShadow(Game1.spriteBatch, healthText, Game1.smallFont, new Vector2(monsterBox.X + (float)monsterBox.Width / 2 - 9 * Game1.options.zoomLevel, monsterBox.Y + 2), Color.DarkSlateGray, Game1.options.zoomLevel * 0.4f, -1, 0, 0, 0, 0);
                    }
                }
            }

            //Game1.spriteBatch.End();
        }
    }
}
