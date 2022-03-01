﻿using AQMod.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.DrawOverlays
{
    public class GlowmaskOverlay : IOverlayDrawWorld, IOverlayDrawInventory, IOverlayDrawPlayerUse
    {
        public readonly string Path;
        public readonly Func<Color> GetDrawColor;
        public readonly bool DrawInventory;
        public int Shader;

        public static Color DefaultGlowmaskColor => new Color(250, 250, 250, 0);

        internal GlowmaskOverlay(string path, int shader = 0, bool drawInventory = false)
        {
            Path = path;
            GetDrawColor = () => DefaultGlowmaskColor;
            DrawInventory = drawInventory;
            Shader = shader;
        }

        internal GlowmaskOverlay(string path, Color color, int shader = 0, bool drawInventory = false)
        {
            Path = path;
            GetDrawColor = () => color;
            DrawInventory = drawInventory;
            Shader = shader;
        }

        internal GlowmaskOverlay(string path, Func<Color> color, int shader = 0, bool drawInventory = false)
        {
            Path = path;
            GetDrawColor = color;
            DrawInventory = drawInventory;
            Shader = shader;
        }

        private void texturetest()
        {
            ModContent.GetTexture(Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawingOnPlayer">Whether this is called on the player's glowmask drawcode</param>
        /// <returns>Whether to draw the glowmask</returns>
        protected virtual bool ShouldDraw(bool drawingOnPlayer, Item item)
        {
            return true;
        }

        bool IOverlayDrawWorld.PreDrawWorld(Item item, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return false;
        }

        void IOverlayDrawWorld.PostDrawWorld(Item item, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            var texture = ModContent.GetTexture(Path);
            var drawCoordinates = new Vector2(item.position.X - Main.screenPosition.X + texture.Width / 2 + item.width / 2 - texture.Width / 2, item.position.Y - Main.screenPosition.Y + texture.Height / 2 + item.height - texture.Height + 2f);
            var drawFrame = new Rectangle(0, 0, texture.Width, texture.Height);
            var drawRotation = rotation;
            var origin = Main.itemTexture[item.type].Size() / 2;
            if (ShouldDraw(false, item))
            {
                var drawData = new DrawData(texture, drawCoordinates, drawFrame, GetDrawColor(), drawRotation, origin, scale, SpriteEffects.None, 0);
                if (Shader != 0)
                {
                    Main.spriteBatch.End();
                    BatcherMethods.GeneralEntities.BeginShader(Main.spriteBatch);
                    GameShaders.Armor.Apply(Shader, item, drawData);
                }
                drawData.Draw(Main.spriteBatch);
                if (Shader != 0)
                {
                    Main.spriteBatch.End();
                    BatcherMethods.GeneralEntities.Begin(Main.spriteBatch);
                }
            }
        }

        bool IOverlayDrawInventory.PreDrawInv(Player player, AQPlayer aQPlayer, Item item, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return false;
        }

        void IOverlayDrawInventory.PostDrawInv(Player player, AQPlayer aQPlayer, Item item, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (DrawInventory)
            {
                var texture = ModContent.GetTexture(Path);
                Main.spriteBatch.Draw(texture, position, frame, GetDrawColor(), 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        void IOverlayDrawPlayerUse.DrawUse(Player player, AQPlayer aQPlayer, Item item, PlayerDrawInfo info)
        {
            var texture = ModContent.GetTexture(Path);
            if (item.useStyle == ItemUseStyleID.HoldingOut)
            {
                if (Item.staff[item.type])
                {
                    float drawRotation3 = info.drawPlayer.itemRotation + 0.785f * info.drawPlayer.direction;
                    int offsetX1 = 0;
                    int offsetY = 0;
                    var origin3 = new Vector2(0f, Main.itemTexture[info.drawPlayer.inventory[info.drawPlayer.selectedItem].type].Height);
                    if (info.drawPlayer.gravDir == -1f)
                    {
                        if (info.drawPlayer.direction == -1)
                        {
                            drawRotation3 += 1.57f;
                            origin3 = new Vector2(Main.itemTexture[info.drawPlayer.inventory[info.drawPlayer.selectedItem].type].Width, 0f);
                            offsetX1 -= Main.itemTexture[info.drawPlayer.inventory[info.drawPlayer.selectedItem].type].Width;
                        }
                        else
                        {
                            drawRotation3 -= 1.57f;
                            origin3 = Vector2.Zero;
                        }
                    }
                    else if (info.drawPlayer.direction == -1)
                    {
                        origin3 = new Vector2(Main.itemTexture[info.drawPlayer.inventory[info.drawPlayer.selectedItem].type].Width, Main.itemTexture[info.drawPlayer.inventory[info.drawPlayer.selectedItem].type].Height);
                        offsetX1 -= Main.itemTexture[info.drawPlayer.inventory[info.drawPlayer.selectedItem].type].Width;
                    }
                    Vector2 holdoutOrigin = Vector2.Zero;
                    ItemLoader.HoldoutOrigin(info.drawPlayer, ref holdoutOrigin);
                    var drawCoordinates3 = new Vector2((int)(info.itemLocation.X - Main.screenPosition.X + origin3.X + offsetX1), (int)(info.itemLocation.Y - Main.screenPosition.Y + offsetY));
                    var drawFrame3 = new Rectangle(0, 0, texture.Width, texture.Height);
                    origin3 += holdoutOrigin;
                    if (ShouldDraw(true, item))
                        Main.playerDrawData.Add(new DrawData(texture, drawCoordinates3, drawFrame3, GetDrawColor(), drawRotation3, origin3, item.scale, info.spriteEffects, 0) { shader = Shader });
                    return;
                }
                var spriteEffects = (SpriteEffects)(player.gravDir != 1f ? player.direction != 1 ? 3 : 2 : player.direction != 1 ? 1 : 0);
                var offset = new Vector2(texture.Width / 2, texture.Height / 2);
                Vector2 holdoutOffset = item.modItem.HoldoutOffset().GetValueOrDefault(new Vector2(10f, 0f)) * player.gravDir;
                int offsetX = (int)holdoutOffset.X;
                offset.Y += holdoutOffset.Y;
                var origin2 = player.direction == -1 ? new Vector2(texture.Width + offsetX, texture.Height / 2) : new Vector2(-offsetX, texture.Height / 2);
                var drawCoordinates2 = new Vector2((int)(player.itemLocation.X - Main.screenPosition.X + offset.X), (int)(player.itemLocation.Y - Main.screenPosition.Y + offset.Y));
                var drawFrame2 = new Rectangle(0, 0, texture.Width, texture.Height);
                var drawRotation2 = player.itemRotation;
                if (ShouldDraw(true, item))
                    Main.playerDrawData.Add(new DrawData(texture, drawCoordinates2, drawFrame2, GetDrawColor(), drawRotation2, origin2, item.scale, spriteEffects, 0) { shader = Shader });
                return;
            }
            if (player.gravDir == -1f)
            {
                var drawCoordinates2 = new Vector2((int)(info.itemLocation.X - Main.screenPosition.X), (int)(info.itemLocation.Y - Main.screenPosition.Y));
                var drawFrame2 = new Rectangle(0, 0, texture.Width, texture.Height);
                var drawRotation2 = player.itemRotation;
                var origin2 = new Vector2(texture.Width * 0.5f - texture.Width * 0.5f * player.direction, 0f);
                if (ShouldDraw(true, item))
                    Main.playerDrawData.Add(new DrawData(texture, drawCoordinates2, drawFrame2, GetDrawColor(), drawRotation2, origin2, item.scale, info.spriteEffects, 0) { shader = Shader });
                return;
            }
            var drawCoordinates = new Vector2((int)(info.itemLocation.X - Main.screenPosition.X), (int)(info.itemLocation.Y - Main.screenPosition.Y));
            var drawFrame = new Rectangle(0, 0, texture.Width, texture.Height);
            var drawRotation = player.itemRotation;
            var origin = new Vector2(texture.Width * 0.5f - texture.Width * 0.5f * player.direction, texture.Height);
            if (ShouldDraw(true, item))
                Main.playerDrawData.Add(new DrawData(texture, drawCoordinates, drawFrame, GetDrawColor(), drawRotation, origin, item.scale, info.spriteEffects, 0) { shader = Shader });
        }
    }
}