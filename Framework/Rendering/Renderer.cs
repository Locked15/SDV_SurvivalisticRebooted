using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survivalistic.Framework.Bars;
using Survivalistic.Framework.Common;
using DaLion.Stardew.Professions.Framework.Ultimates;
using Survive_Net5;
using System;

namespace Survivalistic.Framework.Rendering
{
    public class Renderer
    {
        private static Color specialAbilityTextColor = ModEntry.config.wof_colored_bar ? 
                                                       new Color(148, 120, 188) : 
                                                       Color.White;

        public static void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.CurrentEvent != null) return;

            CheckMouseHovering();

            e.SpriteBatch.Draw(Textures.hunger_sprite, new Rectangle((int)BarsPosition.barPosition.X, (int)BarsPosition.barPosition.Y - 240, Textures.hunger_sprite.Width * 4, Textures.hunger_sprite.Height * 4), Color.White);
            e.SpriteBatch.Draw(Textures.thirst_sprite, new Rectangle((int)BarsPosition.barPosition.X - 60, (int)BarsPosition.barPosition.Y - 240, Textures.thirst_sprite.Width * 4, Textures.thirst_sprite.Height * 4), Color.White);

            e.SpriteBatch.Draw(Textures.filler_sprite, new Vector2(BarsPosition.barPosition.X + 36, BarsPosition.barPosition.Y - 25), new Rectangle(0, 0, Textures.filler_sprite.Width * 4, (int)BarsInformations.hunger_percentage), BarsInformations.GetOffsetHungerColor(), 3.138997f, new Vector2(0.5f, 0.5f), 1f, SpriteEffects.None, 1f);
            e.SpriteBatch.Draw(Textures.filler_sprite, new Vector2(BarsPosition.barPosition.X - 24, BarsPosition.barPosition.Y - 25), new Rectangle(0, 0, Textures.filler_sprite.Width * 4, (int)BarsInformations.thirst_percentage), BarsInformations.GetOffsetThirstyColor(), 3.138997f, new Vector2(0.5f, 0.5f), 1f, SpriteEffects.None, 1f);
        
            if (BarsDatabase.render_numerical_hunger)
            {
                string information = $"{(int)ModEntry.data.actual_hunger}/{(int)ModEntry.data.max_hunger}";
                Vector2 text_size = Game1.dialogueFont.MeasureString(information);
                Vector2 text_position;
                if (BarsDatabase.right_side) text_position = new Vector2(-12, text_size.X);
                else text_position = new Vector2(12 + Textures.hunger_sprite.Width * 4, 0);

                Game1.spriteBatch.DrawString(
                    Game1.dialogueFont,
                    information,
                    new Vector2(BarsPosition.barPosition.X + text_position.X, BarsPosition.barPosition.Y - 240 + ((Textures.hunger_sprite.Height * 4) / 4) + 8),
                    BarsInformations.hunger_color,
                    0f,
                    new Vector2(text_position.Y, 0),
                    1,
                    SpriteEffects.None,
                    0f);
            }

            if (BarsDatabase.render_numerical_thirst)
            {
                string information = $"{(int)ModEntry.data.actual_thirst}/{(int)ModEntry.data.max_thirst}";
                Vector2 text_size = Game1.dialogueFont.MeasureString(information);
                Vector2 text_position;
                if (BarsDatabase.right_side) text_position = new Vector2(-12, text_size.X);
                else text_position = new Vector2(12 + Textures.hunger_sprite.Width * 4, 0);

                Game1.spriteBatch.DrawString(
                    Game1.dialogueFont,
                    information,
                    new Vector2(BarsPosition.barPosition.X - 60 + text_position.X, BarsPosition.barPosition.Y - 240 + ((Textures.hunger_sprite.Height * 4) / 4) + 8),
                    BarsInformations.thirst_color,
                    0f,
                    new Vector2(text_position.Y, 0),
                    1,
                    SpriteEffects.None,
                    0f);
            }

            CheckAndDrawSpecialAbilityHoverText();
        }

        private static void CheckAndDrawSpecialAbilityHoverText()
        {
            bool ultimateIsVisible = WalkOfLifeIntegration.Instance?.UltimateBarIsCurrentlyVisible ?? false;

            if (ultimateIsVisible)
            {
                // Cause we inside, we always will get 'Instance'.
                var currentCharge = WalkOfLifeIntegration.Instance.CurrentUltimateAbility.ChargeValue;
                var maxCharge = WalkOfLifeIntegration.Instance.CurrentUltimateAbility.MaxValue;
                var bonusLevelHeight = (WalkOfLifeIntegration.Instance.CurrentUltimateAbility.MaxValue - Ultimate.BASE_MAX_VALUE_I) * 0.2;

                // get bar position
                var topOfBar = new Vector2(
                    Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 56,
                    Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - 16 - 56 * 4 - (float)bonusLevelHeight
                );

                if (Game1.isOutdoorMapSmallerThanViewport())
                {
                    topOfBar.X = Math.Min(topOfBar.X,
                        -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 48);
                }

                if (Game1.showingHealth)
                {
                    topOfBar.X -= 112;
                }
                else
                {
                    topOfBar.X -= 56;
                }

                if (Game1.getOldMouseX() >= topOfBar.X && Game1.getOldMouseY() >= topOfBar.Y &&
                    Game1.getOldMouseX() < topOfBar.X + 36f)
                {
                    Game1.drawWithBorder(
                    Math.Max(0, (int)currentCharge) + "/" + maxCharge,
                    Color.Black * 0f,
                    specialAbilityTextColor,
                    topOfBar + new Vector2(
                                   0f - Game1.dialogueFont.MeasureString("999/999")
                                   .X - 4f, 64f));
                }
            }
        }

        public static void CheckMouseHovering()
        {
            Vector2 _mouse_position = new Vector2(Game1.getMousePosition(true).X, Game1.getMousePosition(true).Y);

            if (_mouse_position.X >= BarsPosition.barPosition.X &&
                _mouse_position.X <= BarsPosition.barPosition.X + Textures.hunger_sprite.Width * 4 &&
                _mouse_position.Y >= BarsPosition.barPosition.Y - 240 &&
                _mouse_position.Y <= BarsPosition.barPosition.Y - 240 + Textures.hunger_sprite.Height * 4)
            {
                BarsDatabase.render_numerical_hunger = true;
            }
            else
            {
                BarsDatabase.render_numerical_hunger = false;
            }

            if (_mouse_position.X >= BarsPosition.barPosition.X - 60 &&
                _mouse_position.X <= BarsPosition.barPosition.X - 60 + Textures.hunger_sprite.Width * 4 &&
                _mouse_position.Y >= BarsPosition.barPosition.Y - 240 &&
                _mouse_position.Y <= BarsPosition.barPosition.Y - 240 + Textures.hunger_sprite.Height * 4)
            {
                BarsDatabase.render_numerical_thirst = true;
            }
            else
            {
                BarsDatabase.render_numerical_thirst = false;
            }
        }
    }
}
