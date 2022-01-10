﻿using AQMod.Assets;
using AQMod.Common.Graphics.SceneLayers;
using AQMod.Content.Players;
using AQMod.Content.World.Events.GlimmerEvent;
using AQMod.Dusts;
using AQMod.Effects.WorldEffects;
using AQMod.Items.Weapons.Melee;
using AQMod.NPCs.Boss;
using AQMod.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace AQMod.Effects
{
    public static class CustomRenderUltimateSword
    {
        private static byte _swordEffectDelay;
        private static UnifiedRandom _rand;

        internal static void Initialize()
        {
            _swordEffectDelay = 0;
            _rand = new UnifiedRandom();
        }

        internal static void RenderUltimateSword()
        {
            if (!IsCloseEnoughToDraw() || OmegaStariteScenes.SceneType > 1 ||
                Main.netMode != NetmodeID.SinglePlayer && NPC.AnyNPCs(ModContent.NPCType<OmegaStarite>()))
                return;
            var drawPos = SwordPos();
            if (OmegaStariteScenes.OmegaStariteIndexCache == -1)
                OmegaStariteScenes.SceneType = 0;
            var texture = TextureGrabber.GetItem(ModContent.ItemType<UltimateSword>());
            var frame = new Rectangle(0, 0, texture.Width, texture.Height);
            var origin = new Vector2(frame.Width, 0f);
            Main.spriteBatch.Draw(texture, drawPos - Main.screenPosition, frame, new Color(255, 255, 255, 255), MathHelper.PiOver4 * 3f, origin, 1f, SpriteEffects.None, 0f);

            float bobbing = (Bobbing() + 1f) / 2f;
            var blurTexture = ModContent.GetTexture(AQUtils.GetPath<UltimateSword>("_Blur"));
            var blurFrame = new Rectangle(0, 0, blurTexture.Width, blurTexture.Height);
            var blurOrigin = new Vector2(origin.X, blurTexture.Height - texture.Height);
            Main.spriteBatch.Draw(blurTexture, drawPos - Main.screenPosition, blurFrame, new Color(80 + Main.DiscoR / 60, 80 + Main.DiscoG / 60, 80 + Main.DiscoB / 60, 0) * (1f - bobbing), MathHelper.PiOver4 * 3f, blurOrigin, 1f, SpriteEffects.None, 0f);

            var hitbox = new Rectangle((int)drawPos.X - 10, (int)drawPos.Y - 60, 20, 60);
            Vector2 trueMouseworld = AQUtils.TrueMouseworld;
            if (hitbox.Contains((int)trueMouseworld.X, (int)trueMouseworld.Y) && GlimmerEvent.IsGlimmerEventCurrentlyActive())
            {
                int omegaStariteID = ModContent.NPCType<OmegaStarite>();
                if (OmegaStariteScenes.SceneType == 0 && !Main.gameMenu && !Main.gamePaused && Main.LocalPlayer.IsInTileInteractionRange((int)trueMouseworld.X / 16, (int)trueMouseworld.Y / 16))
                {
                    var plr = Main.LocalPlayer;
                    plr.mouseInterface = true;
                    plr.noThrow = 2;
                    plr.showItemIcon = true;
                    plr.showItemIcon2 = ModContent.ItemType<UltimateSword>();
                    var highlightTexture = ModContent.GetTexture(AQUtils.GetPath<UltimateSword>("_Highlight"));
                    Main.spriteBatch.Draw(highlightTexture, drawPos - Main.screenPosition, frame, new Color(255, 255, 255, 255), MathHelper.PiOver4 * 3f, origin, 1f, SpriteEffects.None, 0f);
                    if (Main.mouseRight && Main.mouseRightRelease)
                    {
                        plr.tileInteractAttempted = true;
                        plr.tileInteractionHappened = true;
                        plr.releaseUseTile = false;
                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            AQMod.spawnStarite = true;
                        }
                        else
                        {
                            NetHelper.RequestOmegaStarite();
                        }
                        Main.PlaySound(SoundID.Item, (int)drawPos.X, (int)drawPos.Y, 4, 0.5f, -2.5f);
                    }
                }
            }
        }

        internal static void UpdateUltimateSword()
        {
            if (!GlimmerEvent.IsGlimmerEventCurrentlyActive() || OmegaStariteScenes.SceneType > 1 || !IsCloseEnoughToDraw())
                return;
            var position = SwordPos();
            Lighting.AddLight(position, new Vector3(1f, 1f, 1f));
            if (_rand.NextBool(10))
            {
                int d = Dust.NewDust(position + new Vector2(_rand.Next(-6, 6), -_rand.Next(60)), 2, 2, ModContent.DustType<MonoDust>(), 0f, 0f, 0, new Color(160, 160, 160, 80));
                Main.dust[d].velocity *= 0.1f;
                Main.dust[d].noGravity = true;
            }
            if (_swordEffectDelay > 0)
            {
                _swordEffectDelay--;
            }
            else if (_rand.NextBool(10 + (int)(20 * (1f - AQConfigClient.c_EffectIntensity))))
            {
                AQMod.WorldEffects.Add(new UltimateSwordEffect(_rand));
                _swordEffectDelay = (byte)(int)(8 * (1f - AQConfigClient.c_EffectIntensity));
            }
        }

        internal static float Bobbing()
        {
            return (float)Math.Sin(Main.GameUpdateCount * 0.0157f);
        }

        internal static Vector2 SwordPos()
        {
            float x = GlimmerEvent.tileX * 16f;
            if (Framing.GetTileSafely(GlimmerEvent.tileX, GlimmerEvent.tileY).type == ModContent.TileType<GlimmeringStatue>())
            {
                x += 16f;
            }
            else
            {
                x += 8f;
            }
            float y = GlimmerEvent.tileY * 16 - 80f + Bobbing() * 8f;
            return new Vector2(x, y);
        }

        private static bool IsCloseEnoughToDraw()
        {
            return Main.LocalPlayer.position.X - GlimmerEvent.tileX * 16f < Main.screenWidth + 200f;
        }
    }
}