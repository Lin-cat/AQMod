﻿using AQMod.Assets;
using AQMod.Common.Utilities;
using AQMod.Common.WorldGeneration;
using AQMod.Content.WorldEvents.GlimmerEvent;
using AQMod.Items.Placeable;
using AQMod.Items.Tools.MapMarkers;
using AQMod.Localization;
using AQMod.Tiles.TileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Common.UserInterface
{
    public static class MapInterfaceManager
    {
        public const int MapIconFrames = 4;
        public const int MapIconWidth = 34;
        public const int MapIconWidthPadding = 2;
        public const int TrueMapIconWidth = MapIconWidth - MapIconWidthPadding;
        public const int MapIconHeight = 32;

        private static Vector2 _map;
        private static float _mapScale;

        public static Vector2 MapPos(Vector2 tileCoords)
        {
            var v = mapPosSingle(tileCoords);
            return new Vector2((int)v.X, (int)v.Y);
        }

        private static Vector2 mapPosSingle(Vector2 tileCoords)
        {
            return new Vector2(tileCoords.X * _mapScale + _map.X, tileCoords.Y * _mapScale + _map.Y);
        }

        public static Vector2 MapPosToTilePos(Vector2 mapPos)
        {
            return new Vector2((mapPos.X - _map.X) / _mapScale, (mapPos.Y - _map.Y) / _mapScale);
        }

        public static bool UnityTeleport(float x, float y, Vector2 drawPosition, Player player, bool allowedToTeleport)
        {
            var texture = TextureCache.UnityTeleportable.Value;
            float scale = Main.UIScale * 0.8f;
            drawPosition += new Vector2(-texture.Width / 2f, -texture.Height / 2f) * scale;
            Main.spriteBatch.Draw(texture, drawPosition + new Vector2(2f, 0f), null, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPosition + new Vector2(-2f, 0f), null, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPosition + new Vector2(0f, 2f), null, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPosition + new Vector2(0f, -2f), null, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            if (player.GetModPlayer<AQPlayer>().unityMirror && allowedToTeleport && Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mapFullscreen = false;
                player.UnityTeleport(new Vector2(x, y));
                return true;
            }
            return false;
        }

        public static void Apply(ref string mouseText)
        {
            bool debug = AQMod.DebugKeysPressed;
            _mapScale = Main.mapFullscreenScale / Main.UIScale;
            _map = new Vector2(-(Main.mapFullscreenPos.X * _mapScale) + Main.screenWidth / 2, -(Main.mapFullscreenPos.Y * _mapScale) + Main.screenHeight / 2);
            float alpha = (float)Math.Sin(Main.GlobalTime * 7f) + 1f;
            var plr = Main.player[Main.myPlayer];
            var aQPlayer = plr.GetModPlayer<AQPlayer>();
            if (!debug && aQPlayer.nearGlobe == 0)
                return;

            foreach (var t in TileEntity.ByID)
            {
                var texture = TextureCache.MapIconGlobe.Value;
                var frame = new Rectangle(0, 0, texture.Width, texture.Height);
                var origin = frame.Size() / 2f;
                if (t.Value is TEGlobe globe && (globe.Discovered || debug))
                {
                    var pos = MapPos(new Vector2(globe.Position.X + 1f, globe.Position.Y + 1f));
                    if (AQUtils.PositionOnScreen(pos, 8f))
                    {
                        var scale = Main.UIScale;
                        var hitbox = Utils.CenteredRectangle(pos, new Vector2(texture.Width, texture.Height) * scale);
                        if (hitbox.Contains(Main.mouseX, Main.mouseY))
                        {
                            if (string.IsNullOrEmpty(mouseText))
                                mouseText = Lang.GetItemName(ModContent.ItemType<GlobeItem>()).Value;
                            scale += 0.5f;
                        }
                        if (aQPlayer.globeX == globe.Position.X && aQPlayer.globeY == globe.Position.Y)
                            scale += alpha * 0.1f;
                        Main.spriteBatch.Draw(texture, pos, frame, new Color(255, 255, 255, 255), 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            if (debug)
            {
                foreach (var t in TileEntity.ByID)
                {
                    var texture = TextureCache.MapIconGlobe.Value;
                    var frame = new Rectangle(0, 0, texture.Width, texture.Height);
                    var origin = frame.Size() / 2f;
                    if (t.Value is TEGlimmeringStatue glimmer)
                    {
                        var pos = MapPos(new Vector2(glimmer.Position.X + 1f, glimmer.Position.Y + 1f));
                        if (AQUtils.PositionOnScreen(pos, 8f))
                        {
                            var scale = Main.UIScale;
                            var hitbox = Utils.CenteredRectangle(pos, new Vector2(texture.Width, texture.Height) * scale);
                            if (hitbox.Contains(Main.mouseX, Main.mouseY))
                            {
                                if (string.IsNullOrEmpty(mouseText))
                                    mouseText = Lang.GetItemName(ModContent.ItemType<Items.Placeable.GlimmeringStatue>()).Value;
                                scale += 0.5f;
                            }
                            if (glimmer.discovered)
                                scale += alpha * 0.1f;
                            Main.spriteBatch.Draw(texture, pos, frame, new Color(0, 0, 255, 255), 0f, origin, scale, SpriteEffects.None, 0f);
                        }
                    }
                }
            }

            var buffToggleType = new List<int>();
            var buffEnabled = new List<bool>();
            var buffToggleFunctions = new List<Action>();

            if (aQPlayer.retroMap || debug)
            {
                buffToggleType.Add(ModContent.BuffType<RetroMarkerBuff>());
                buffEnabled.Add(aQPlayer.showRetroMap);
                buffToggleFunctions.Add(() => aQPlayer.showRetroMap = !aQPlayer.showRetroMap);
                if (aQPlayer.showRetroMap)
                {
                    var texture = TextureCache.MapIconEnemyBlip.Value;
                    int frameHeight = texture.Height / 2;
                    int frameNumber = (int)(Main.GameUpdateCount % 24 / 12);
                    var frame = new Rectangle(0, frameHeight * frameNumber, texture.Width, frameHeight);
                    Vector2 origin = frameNumber == 1 ? new Vector2(3f, 5f) : new Vector2(4f, 4f);
                    var scale = Main.UIScale * 4f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && !AQNPC.Sets.NoMapBlip[Main.npc[i].type] && Main.npc[i].GetBossHeadTextureIndex() == -1 && !Main.npc[i].townNPC)
                        {
                            var drawPos = MapPos(Main.npc[i].Center / 16f);
                            var color = AQMod.MapBlipColor;
                            byte r = color.R;
                            if (r < 10)
                                r = 10;
                            byte g = color.G;
                            if (g < 10)
                                g = 10;
                            byte b = color.B;
                            if (b < 10)
                                b = 10;
                            Main.spriteBatch.Draw(texture, drawPos, frame, new Color(r, g, b, 150), 0f, origin, scale, SpriteEffects.None, 0f);
                        }
                    }
                }
            }

            if (aQPlayer.cosmicMap || debug)
            {
                //buffToggleType.Add(ModContent.BuffType<CosmicMarkerBuff>());
                //buffEnabled.Add(aQPlayer.showCosmicMap);
                //buffToggleFunctions.Add(() => aQPlayer.showCosmicMap = !aQPlayer.showCosmicMap);
                if (aQPlayer.showCosmicMap && AQMod.glimmerEvent.IsActive)
                {
                    var texture = TextureCache.MapIconGlimmerEvent.Value;
                    var frame = new Rectangle(0, 0, texture.Width, texture.Height);
                    var drawPos = MapPos(new Vector2(AQMod.glimmerEvent.tileX + 0.5f, AQMod.glimmerEvent.tileY - 3f));
                    var hitbox = Utils.CenteredRectangle(drawPos, new Vector2(texture.Width, texture.Height) * Main.UIScale);
                    var scale = Main.UIScale;
                    if (hitbox.Contains(Main.mouseX, Main.mouseY))
                    {
                        string key = "Common.GlimmerEvent";
                        if (debug)
                        {
                            mouseText = key;
                            if (Main.mouseRight)
                            {
                                var p = MapPosToTilePos(new Vector2(Main.mouseX, Main.mouseY));
                                AQMod.glimmerEvent.tileX = (ushort)(int)p.X;
                                AQMod.glimmerEvent.tileY = (ushort)(int)p.Y;
                            }
                        }
                        else
                        {
                            mouseText = AQText.ModText(key).Value;
                        }
                        scale += 0.5f;
                    }
                    Main.spriteBatch.Draw(texture, drawPos, frame, new Color(255, 255, 255, 255), 0f, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
                    texture = TextureCache.MapIconStarite.Value;
                    var stariteFrame = new Rectangle(0, 0, texture.Width, 22);
                    var arrowUpFrame = new Rectangle(0, stariteFrame.Height + 2, texture.Width, 32);
                    var arrowDownFrame = new Rectangle(0, arrowUpFrame.Y + arrowUpFrame.Height + 2, texture.Width, 32);
                    var stariteOrig = stariteFrame.Size() / 2f;
                    var arrowUpOrig = new Vector2(arrowUpFrame.Width / 2f, 0f);
                    var arrowDownOrig = new Vector2(arrowUpOrig.X, arrowUpFrame.Height);
                    float arrowBobbingY = alpha * 2f;
                    for (int i = 0; i < GlimmerEvent.Layers.Count; i++)
                    {
                        var layer = GlimmerEvent.Layers[i];
                        for (int j = 0; j < 2; j++)
                        {
                            int d = j == 1 ? -1 : 1;
                            var pos = new Vector2(AQMod.glimmerEvent.tileX + 0.5f + layer.Distance * d, 46f);
                            if (pos.X < 0f || pos.X > Main.maxTilesX)
                                continue;
                            for (int k = 0; k < 2; k++)
                            {
                                if (k == 1)
                                    pos.Y = (float)Main.worldSurface;
                                drawPos = MapPos(pos);
                                hitbox = Utils.CenteredRectangle(drawPos, new Vector2(stariteFrame.Width, stariteFrame.Height) * Main.UIScale);
                                float stariteScale = Main.UIScale;
                                if (hitbox.Contains(Main.mouseX, Main.mouseY))
                                {
                                    if (debug)
                                    {
                                        mouseText = "npc: " + layer.NPCType + "\ndistance from center: " + layer.Distance + "\nspawn chance: " + layer.SpawnChance + "\nid: " + i;
                                    }
                                    else
                                    {
                                        mouseText = string.Format(AQText.ModText("Common.SpawnAfterPoint").Value, Lang.GetNPCName(layer.NPCType));
                                    }
                                    stariteScale += 0.6f + alpha * 0.2f;
                                }
                                Main.spriteBatch.Draw(texture, drawPos, stariteFrame, new Color(255, 255, 255, 255), 0f, stariteOrig, stariteScale, SpriteEffects.None, 0f);
                                if (k == 0)
                                {
                                    Main.spriteBatch.Draw(texture, drawPos + new Vector2(0f, (arrowDownFrame.Height + stariteFrame.Height + arrowBobbingY) * Main.UIScale), arrowDownFrame, new Color(255, 255, 255, 255), 0f, arrowDownOrig, Main.UIScale, SpriteEffects.None, 0f);
                                }
                                else
                                {
                                    Main.spriteBatch.Draw(texture, drawPos - new Vector2(0f, (arrowUpFrame.Height + stariteFrame.Height + arrowBobbingY) * Main.UIScale), arrowUpFrame, new Color(255, 255, 255, 255), 0f, arrowUpOrig, Main.UIScale, SpriteEffects.None, 0f);
                                }
                            }
                        }
                    }
                }
            }

            if (aQPlayer.dungeonMap || debug)
            {
                //buffToggleType.Add(ModContent.BuffType<DungeonMarkerBuff>());
                //buffEnabled.Add(aQPlayer.showDungeonMap);
                //buffToggleFunctions.Add(() => aQPlayer.showDungeonMap = !aQPlayer.showDungeonMap);
                if (aQPlayer.showDungeonMap && (Main.Map[Main.dungeonX, Main.dungeonY].Light > 40 || NPC.downedBoss3 || Main.hardMode))
                {
                    var mapIcon = TextureCache.MapIconDungeons.Value;
                    int iconFrame;
                    switch (Framing.GetTileSafely(Main.dungeonX, Main.dungeonY).type)
                    {
                        default:
                        iconFrame = 3;
                        break;

                        case TileID.BlueDungeonBrick:
                        iconFrame = 0;
                        break;

                        case TileID.PinkDungeonBrick:
                        iconFrame = 1;
                        break;

                        case TileID.GreenDungeonBrick:
                        iconFrame = 2;
                        break;
                    }
                    var frame = new Rectangle(MapIconWidth * iconFrame, 0, TrueMapIconWidth, MapIconHeight);
                    var drawPos = MapPos(new Vector2(Main.dungeonX + 0.5f, Main.dungeonY - 2.5f));
                    var hitbox = Utils.CenteredRectangle(drawPos, new Vector2(frame.Width, frame.Height) * Main.UIScale);
                    var scale = Main.UIScale;
                    bool hovering = hitbox.Contains(Main.mouseX, Main.mouseY);
                    if (hovering)
                    {
                        mouseText = "Dungeon";
                        scale += 0.1f;
                    }
                    Main.spriteBatch.Draw(mapIcon, drawPos, frame, new Color(255, 255, 255, 255), 0f, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
                    UnityTeleport((Main.dungeonX + 0.5f) * 16f, Main.dungeonY * 16f - plr.height, drawPos + new Vector2(frame.Width / 2f + 2f, frame.Height / 2f) * scale, plr, (NPC.downedBoss3 || Main.hardMode) && hovering);
                }
            }

            if (aQPlayer.lihzahrdMap || debug)
            {
                buffToggleType.Add(ModContent.BuffType<LihzahrdMarkerBuff>());
                buffEnabled.Add(aQPlayer.showLihzahrdMap);
                buffToggleFunctions.Add(() => aQPlayer.showLihzahrdMap = !aQPlayer.showLihzahrdMap);
                if (aQPlayer.showLihzahrdMap && CommonStructureSearchMethods.LihzahrdAltar(out var position) && (Main.Map[position.X, position.Y].Light > 40 || NPC.downedPlantBoss))
                {
                    var mapIcon = TextureCache.MapIconDungeons.Value;
                    var frame = new Rectangle(MapIconWidth * 4, 0, TrueMapIconWidth, MapIconHeight);
                    var drawPos = MapPos(new Vector2(position.X + 1.5f, position.Y - 0.5f));
                    var hitbox = Utils.CenteredRectangle(drawPos, new Vector2(frame.Width, frame.Height) * Main.UIScale);
                    var scale = Main.UIScale;
                    bool hovering = hitbox.Contains(Main.mouseX, Main.mouseY);
                    if (hovering)
                        scale += 0.1f;
                    Main.spriteBatch.Draw(mapIcon, drawPos, frame, new Color(255, 255, 255, 255), 0f, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
                    UnityTeleport((position.X + 1f) * 16f, (position.Y + 2f) * 16f - plr.height, drawPos + new Vector2(frame.Width / 2f + 2f, frame.Height / 2f) * scale, plr, NPC.downedPlantBoss && hovering);
                }
            }

            for (int i = 0; i < buffToggleType.Count; i++)
            {
                var buffTexture = TextureCache.GetBuff(buffToggleType[i]);
                float scale = Main.UIScale;
                var drawPosition = new Vector2((8f + (buffTexture.Width + 8f) * i) * scale, (8f + buffTexture.Height / 2f) * scale);
                var drawColor = new Color(255, 255, 255, 255);
                if (!buffEnabled[i])
                {
                    drawColor *= 0.5f;
                    drawColor.A = 200;
                }
                var hitbox = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, (int)(buffTexture.Width * scale), (int)(buffTexture.Height * scale));
                if (hitbox.Contains(Main.mouseX, Main.mouseY))
                {
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                        buffToggleFunctions[i].Invoke();
                    mouseText = Lang.GetBuffName(buffToggleType[i]);
                    Main.spriteBatch.Draw(TextureCache.BuffOutline.Value, drawPosition, null, drawColor, 0f, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
                }
                Main.spriteBatch.Draw(buffTexture, drawPosition, null, drawColor, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            }
        }
    }
}