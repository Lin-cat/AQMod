﻿using AQMod.Assets;
using AQMod.Buffs.Debuffs;
using AQMod.Buffs.Debuffs.Temperature;
using AQMod.Common;
using AQMod.Common.Configuration;
using AQMod.Common.Graphics;
using AQMod.Common.Graphics.Particles;
using AQMod.Common.Graphics.PlayerEquips;
using AQMod.Content.CursorDyes;
using AQMod.Content.Fishing;
using AQMod.Content.Seasonal.Christmas;
using AQMod.Content.World.Events.GaleStreams;
using AQMod.Content.World.Events.GlimmerEvent;
using AQMod.Dusts;
using AQMod.Effects.ScreenEffects;
using AQMod.Items;
using AQMod.Items.Accessories.Amulets;
using AQMod.Items.Accessories.FishingSeals;
using AQMod.Items.Armor.Arachnotron;
using AQMod.Items.DrawOverlays;
using AQMod.Items.Foods;
using AQMod.Items.Quest.Angler;
using AQMod.Items.Vanities;
using AQMod.Projectiles;
using AQMod.Projectiles.Pets;
using AQMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace AQMod
{
    public sealed class AQPlayer : ModPlayer
    {
        public const int MaxCelesteTorusOrbs = 5;
        public const int MAX_ARMOR = 20;
        public const int DYE_WRAP = MAX_ARMOR / 2;
        public const int FRAME_HEIGHT = 56;
        public const int FRAME_COUNT = 20;
        public const float CELESTE_Z_MULT = 0.0157f;
        public const int ARACHNOTRON_OLD_POS_LENGTH = 8;
        public const float AtmosphericCurrentsWindSpeed = 30f;
        public const byte TEMPERATURE_REGEN_NORMAL = 32;
        public const byte TEMPERATURE_REGEN_FROST_ARMOR_COLD_TEMP = 20;
        public const byte TEMPERATURE_REGEN_ON_HIT = 120;

        public static class Layers
        {
            public const string MasksPath = "AQMod/Assets/Equips/Masks/Mask_";
            public const string HeadAccsPath = "AQMod/Assets/Equips/HeadAccs/HeadAcc_";

            public static readonly PlayerHeadLayer PostDrawHead_Head = new PlayerHeadLayer("AQMod", "PostDraw", (info) =>
            {
                var player = info.drawPlayer;
                AQPlayer aQPlayer = info.drawPlayer.GetModPlayer<AQPlayer>();
                var drawingPlayer = info.drawPlayer.GetModPlayer<AQPlayer>();
                if (drawingPlayer.mask >= 0)
                {
                    var drawData = new DrawData(ModContent.GetTexture(MasksPath + drawingPlayer.mask), new Vector2((int)(info.drawPlayer.position.X - Main.screenPosition.X - info.drawPlayer.bodyFrame.Width / 2 + info.drawPlayer.width / 2), (int)(info.drawPlayer.position.Y - Main.screenPosition.Y + info.drawPlayer.height - info.drawPlayer.bodyFrame.Height)) + info.drawPlayer.headPosition + info.drawOrigin, info.drawPlayer.bodyFrame, info.armorColor, info.drawPlayer.headRotation, info.drawOrigin, info.scale, info.spriteEffects, 0);
                    GameShaders.Armor.Apply(drawingPlayer.cMask, player, drawData);
                    drawData.Draw(Main.spriteBatch);
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }

                if (drawingPlayer.headAcc >= 0)
                {
                    var drawData = new DrawData(ModContent.GetTexture(HeadAccsPath + drawingPlayer.headAcc), new Vector2((int)(info.drawPlayer.position.X - Main.screenPosition.X - info.drawPlayer.bodyFrame.Width / 2 + info.drawPlayer.width / 2), (int)(info.drawPlayer.position.Y - Main.screenPosition.Y + info.drawPlayer.height - info.drawPlayer.bodyFrame.Height)) + info.drawPlayer.headPosition + info.drawOrigin, info.drawPlayer.bodyFrame, info.armorColor, info.drawPlayer.headRotation, info.drawOrigin, info.scale, info.spriteEffects, 0);
                    if (drawingPlayer.headAcc == PlayerHeadOverlayID.FishyFins)
                        drawData.color = player.skinColor;
                    drawData.position = new Vector2((int)drawData.position.X, (int)drawData.position.Y);
                    GameShaders.Armor.Apply(drawingPlayer.cHeadAcc, player, drawData);
                    drawData.Draw(Main.spriteBatch);
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            });

            public static readonly PlayerLayer PreDraw = new PlayerLayer("AQMod", "PreDraw", (info) =>
            {
                int whoAmI = info.drawPlayer.whoAmI;
                var player = info.drawPlayer;
                var aQPlayer = player.GetModPlayer<AQPlayer>();
                var drawingPlayer = player.GetModPlayer<AQPlayer>();
                if (info.shadow == 0f)
                {
                    if (aQPlayer.blueSpheres && drawingPlayer.celesteTorusOffsetsForDrawing != null)
                    {
                        var texture = TextureGrabber.GetProjectile(ModContent.ProjectileType<CelesteTorusCollider>());
                        var frame = new Rectangle(0, 0, texture.Width, texture.Height);
                        var orig = frame.Size() / 2f;
                        for (int i = 0; i < AQPlayer.MaxCelesteTorusOrbs; i++)
                        {
                            var position = aQPlayer.GetCelesteTorusPositionOffset(i);
                            float layerValue = AQUtils.OmegaStarite3DHelper.GetParralaxScale(1f, drawingPlayer.celesteTorusOffsetsForDrawing[i].Z * CELESTE_Z_MULT);
                            if (layerValue < 1f)
                            {
                                var center = info.position + new Vector2(player.width / 2 + (int)position.X, player.height / 2 + (int)position.Y);
                                Main.playerDrawData.Add(new DrawData(texture, AQUtils.OmegaStarite3DHelper.GetParralaxPosition(center, drawingPlayer.celesteTorusOffsetsForDrawing[i].Z * AQPlayer.CELESTE_Z_MULT) - Main.screenPosition, frame, Lighting.GetColor((int)(center.X / 16f), (int)(center.Y / 16f)), 0f, orig, AQUtils.OmegaStarite3DHelper.GetParralaxScale(aQPlayer.celesteTorusScale, drawingPlayer.celesteTorusOffsetsForDrawing[i].Z * AQPlayer.CELESTE_Z_MULT), SpriteEffects.None, 0) { shader = drawingPlayer.cCelesteTorus, ignorePlayerRotation = true });
                            }
                        }
                    }
                    if (aQPlayer.chomper)
                    {
                        int count = 0;
                        int type = ModContent.ProjectileType<Chomper>();
                        var texture = TextureGrabber.GetProjectile(type);
                        int frameHeight = texture.Height / Main.projFrames[type];
                        var frame = new Rectangle(0, 0, texture.Width, frameHeight - 2);
                        var textureOrig = frame.Size() / 2f;
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].type == type && Main.projectile[i].owner == info.drawPlayer.whoAmI)
                            {
                                var drawPosition = Main.projectile[i].Center;
                                frame = new Rectangle(0, frameHeight * Main.projectile[i].frame, texture.Width, frameHeight - 2);
                                var drawColor = Lighting.GetColor((int)drawPosition.X / 16, (int)drawPosition.Y / 16);
                                float rotation;
                                if (Main.projectile[i].spriteDirection == -1 && (int)Main.projectile[i].ai[1] > 0f)
                                {
                                    rotation = Main.projectile[i].rotation - MathHelper.Pi;
                                }
                                else
                                {
                                    rotation = Main.projectile[i].rotation;
                                }
                                if (Main.myPlayer == info.drawPlayer.whoAmI)
                                {
                                    DrawChomperChain(info.drawPlayer, Main.projectile[i], drawPosition, drawColor);
                                    var chomperHead = (Chomper)Main.projectile[i].modProjectile;
                                    if (chomperHead.eatingDelay != 0 && chomperHead.eatingDelay < 35)
                                    {
                                        float intensity = (10 - chomperHead.eatingDelay) / 2.5f * AQConfigClient.c_EffectIntensity;
                                        drawPosition.X += Main.rand.NextFloat(-intensity, intensity);
                                        drawPosition.Y += Main.rand.NextFloat(-intensity, intensity);
                                    }
                                    drawPosition = new Vector2((int)(drawPosition.X - Main.screenPosition.X), (int)(drawPosition.Y - Main.screenPosition.Y));
                                    Main.playerDrawData.Add(
                                        new DrawData(texture, drawPosition, frame, drawColor, rotation, textureOrig, Main.projectile[i].scale, Main.projectile[i].spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0)
                                        { ignorePlayerRotation = true });
                                    if (count == 0)
                                    {
                                        if (aQPlayer.monoxiderBird)
                                        {
                                            var monoxiderHat = TextureGrabber.GetItem(ModContent.ItemType<MonoxideHat>());
                                            var hatPos = new Vector2(drawPosition.X, drawPosition.Y) + new Vector2(0f, -Main.projectile[i].height / 2).RotatedBy(Main.projectile[i].rotation);
                                            var monoxiderHatOrig = monoxiderHat.Size() / 2f;
                                            hatPos = new Vector2((int)hatPos.X, (int)hatPos.Y);
                                            Main.playerDrawData.Add(
                                                new DrawData(monoxiderHat, hatPos, null, drawColor, Main.projectile[i].rotation, monoxiderHatOrig, Main.projectile[i].scale, SpriteEffects.None, 0)
                                                { ignorePlayerRotation = true });
                                            Main.playerDrawData.Add(
                                                new DrawData(ModContent.GetTexture(AQUtils.GetPath<MonoxideHat>() + "_Glow"), hatPos, null, new Color(250, 250, 250, 0), Main.projectile[i].rotation, monoxiderHatOrig, Main.projectile[i].scale, SpriteEffects.None, 0)
                                                { ignorePlayerRotation = true });
                                            int headFrame = player.bodyFrame.Y / FRAME_HEIGHT;
                                            if (player.gravDir == -1)
                                                hatPos.Y += player.height + 8f;
                                            Projectiles.Summon.Monoxider.DrawHead(player, aQPlayer, hatPos, ignorePlayerRotation: true);
                                        }
                                    }
                                    count++;
                                }
                            }
                        }
                    }
                }
            });

            private static void DrawChomperChain(Player player, Projectile chomper, Vector2 drawPosition, Color drawColor)
            {
                var chomperHead = (Chomper)chomper.modProjectile;
                int frameWidth = 16;
                var frame = new Rectangle(0, 0, frameWidth - 2, 20);
                var origin = frame.Size() / 2f;
                float offset = chomper.width / 2f + frame.Height / 2f;
                var texture = ModContent.GetTexture(AQUtils.GetPath<Chomper>("_Chain"));
                Main.playerDrawData.Add(new DrawData(texture, new Vector2(drawPosition.X + chomper.width / 2 * -chomper.spriteDirection, drawPosition.Y), frame, drawColor, 0f, origin, chomper.scale, chomper.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0)
                { ignorePlayerRotation = true });
                int height = frame.Height - 4;
                frame.Y += 2;
                var playerCenter = player.Center;
                var chainStart = chomper.Center + new Vector2((chomper.width / 2 + 4) * -chomper.spriteDirection, 0f);
                var velo = Vector2.Normalize(Vector2.Lerp(chainStart + new Vector2(0f, height * 4f) - playerCenter, player.velocity, 0.5f)) * height;
                var position = playerCenter;
                var rand = new UnifiedRandom(chomper.whoAmI + player.name.GetHashCode());
                if (AQConfigClient.c_EffectQuality >= 1f)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Main.playerDrawData.Add(new DrawData(
                            texture, new Vector2((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y)), new Rectangle(frame.X + frameWidth + frameWidth * rand.Next(3), frame.Y, frame.Width, frame.Height), Lighting.GetColor((int)(position.X / 16), (int)(position.Y / 16f)), velo.ToRotation() + MathHelper.PiOver2, origin, 1f, SpriteEffects.None, 0)
                        { ignorePlayerRotation = true });
                        velo = Vector2.Normalize(Vector2.Lerp(velo, chainStart - position, 0.01f + MathHelper.Clamp(1f - Vector2.Distance(chainStart, position) / 100f, 0f, 0.99f))) * height;
                        if (Vector2.Distance(position, chainStart) <= height)
                            break;
                        velo = velo.RotatedBy(Math.Sin(Main.GlobalTime * 6f + i * 0.5f + chomper.whoAmI + rand.NextFloat(-0.02f, 0.02f)) * 0.1f * AQConfigClient.c_EffectIntensity);
                        position += velo;
                        float gravity = MathHelper.Clamp(1f - Vector2.Distance(chainStart, position) / 60f, 0f, 1f);
                        velo.Y += gravity * 3f;
                        velo.Normalize();
                        velo *= height;
                    }
                }
                else
                {
                    if (AQConfigClient.c_EffectQuality < 0.2f)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Main.playerDrawData.Add(new DrawData(
                                texture, new Vector2((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y)), new Rectangle(frame.X + frameWidth + frameWidth * (i % 3), frame.Y, frame.Width, frame.Height), Lighting.GetColor((int)(position.X / 16), (int)(position.Y / 16f)), velo.ToRotation() + MathHelper.PiOver2, origin, 1f, SpriteEffects.None, 0)
                            { ignorePlayerRotation = true });
                            velo = Vector2.Normalize(Vector2.Lerp(velo, chainStart - position, 0.01f + MathHelper.Clamp(1f - Vector2.Distance(chainStart, position) / 100f, 0f, 0.99f))) * height;
                            if (Vector2.Distance(position, chainStart) <= height)
                                break;
                            position += velo;
                            float gravity = MathHelper.Clamp(1f - Vector2.Distance(chainStart, position) / 60f, 0f, 1f);
                            velo.Y += gravity * 3f;
                            velo.Normalize();
                            velo *= height;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Main.playerDrawData.Add(new DrawData(
                                texture, new Vector2((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y)), new Rectangle(frame.X + frameWidth + frameWidth * rand.Next(3), frame.Y, frame.Width, frame.Height), Lighting.GetColor((int)(position.X / 16), (int)(position.Y / 16f)), velo.ToRotation() + MathHelper.PiOver2, origin, 1f, SpriteEffects.None, 0)
                            { ignorePlayerRotation = true });
                            velo = Vector2.Normalize(Vector2.Lerp(velo, chainStart - position, 0.01f + MathHelper.Clamp(1f - Vector2.Distance(chainStart, position) / 100f, 0f, 0.99f))) * height;
                            if (Vector2.Distance(position, chainStart) <= height)
                                break;
                            position += velo;
                            float gravity = MathHelper.Clamp(1f - Vector2.Distance(chainStart, position) / 60f, 0f, 1f);
                            velo.Y += gravity * 3f;
                            velo.Normalize();
                            velo *= height;
                        }
                    }
                }
                rand = new UnifiedRandom(chomper.whoAmI + 2 + player.name.GetHashCode());
                texture = ModContent.GetTexture(AQUtils.GetPath<Chomper>("_Leaves"));
                frame.Y -= 2;
                int numLeaves = rand.Next(4) + 3;
                float leafRotation = chomper.rotation;
                if (chomper.spriteDirection == -1 && chomper.rotation.Abs() > MathHelper.PiOver2)
                    leafRotation -= MathHelper.Pi;
                float rotOff = MathHelper.PiOver2 / numLeaves;
                float rotStart = leafRotation - MathHelper.PiOver4;
                for (int i = 0; i < numLeaves; i++)
                {
                    var leavesPos = drawPosition + new Vector2((offset - rand.NextFloat(2f)) * -chomper.spriteDirection, 0f).RotatedBy(rotStart + rotOff * i) - Main.screenPosition;
                    leafRotation = (drawPosition - Main.screenPosition - leavesPos).ToRotation();
                    Main.playerDrawData.Add(new DrawData(texture, new Vector2((int)leavesPos.X, leavesPos.Y), new Rectangle(frame.X + frameWidth * rand.Next(4), frame.Y, frame.Width, frame.Height), drawColor, leafRotation + MathHelper.PiOver2, origin, chomper.scale + rand.NextFloat(0.2f), SpriteEffects.None, 0)
                    { ignorePlayerRotation = true });
                }
            }

            public static readonly PlayerLayer PostDraw = new PlayerLayer("AQMod", "PostDraw", (info) =>
            {
                int whoAmI = info.drawPlayer.whoAmI;
                var aQMod = AQMod.GetInstance();
                var player = info.drawPlayer;
                var aQPlayer = player.GetModPlayer<AQPlayer>();
                if (Main.myPlayer == info.drawPlayer.whoAmI && info.shadow == 0f)
                {
                    bool updateOldPos = true;
                    if (ShouldDrawOldPos(info.drawPlayer))
                    {
                        if (oldPosVisual != null && oldPosVisual.Length >= oldPosLength)
                        {
                            if (arachnotronHeadTrail)
                            {
                                if (info.shadow == 0f)
                                {
                                    var headOff = new Vector2(-info.drawPlayer.bodyFrame.Width / 2 + (float)(info.drawPlayer.width / 2), info.drawPlayer.height - info.drawPlayer.bodyFrame.Height + 10f) + info.drawPlayer.headPosition + info.headOrigin;
                                    var clr = new Color(255, 255, 255, 0) * (1f - info.shadow);
                                    var drawDiff = info.position - info.drawPlayer.position;
                                    var texture = ModContent.GetTexture(AQUtils.GetPath<ArachnotronVisor>("_HeadGlow"));
                                    int count = aQPlayer.GetOldPosCountMaxed(ARACHNOTRON_OLD_POS_LENGTH);
                                    var clrMult = 1f / count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        float colorMult = 0.5f * (1f - (float)Math.Sin(Main.GlobalTime * 8f - i * 0.314f) * 0.2f);
                                        var drawData = new DrawData(texture, new Vector2((int)(oldPosVisual[i].X - Main.screenPosition.X), (int)(oldPosVisual[i].Y - Main.screenPosition.Y)) + drawDiff + headOff, info.drawPlayer.bodyFrame, clr * (clrMult * (count - i)) * colorMult, info.drawPlayer.bodyRotation, info.bodyOrigin, 1f, info.spriteEffects, 0) { shader = info.headArmorShader };
                                        Main.playerDrawData.Add(drawData);
                                    }
                                }
                            }
                            if (arachnotronBodyTrail)
                            {
                                var bodyOff = new Vector2(-info.drawPlayer.bodyFrame.Width / 2 + (float)(info.drawPlayer.width / 2), info.drawPlayer.height - info.drawPlayer.bodyFrame.Height + 4f) + info.drawPlayer.bodyPosition + new Vector2(info.drawPlayer.bodyFrame.Width / 2, info.drawPlayer.bodyFrame.Height / 2);
                                var clr = new Color(255, 255, 255, 0) * (1f - info.shadow);
                                var drawDiff = info.position - info.drawPlayer.position;
                                var texture = ModContent.GetTexture(AQUtils.GetPath<ArachnotronRibcage>("_BodyGlow"));
                                int count = aQPlayer.GetOldPosCountMaxed(ARACHNOTRON_OLD_POS_LENGTH);
                                if (info.shadow == 0f)
                                {
                                    var clrMult = 1f / count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        float colorMult = 0.5f * (1f - (float)Math.Sin(Main.GlobalTime * 8f - i * 0.314f) * 0.2f);
                                        var drawData = new DrawData(texture, new Vector2((int)(oldPosVisual[i].X - Main.screenPosition.X), (int)(oldPosVisual[i].Y - Main.screenPosition.Y)) + drawDiff + bodyOff, info.drawPlayer.bodyFrame, clr * (clrMult * (count - i)) * colorMult, info.drawPlayer.bodyRotation, info.bodyOrigin, 1f, info.spriteEffects, 0) { shader = info.bodyArmorShader };
                                        Main.playerDrawData.Add(drawData);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        updateOldPos = false;
                    }
                    if (updateOldPos)
                    {
                        if (AQGraphics.GameWorldActive && oldPosLength > 0)
                        {
                            if (oldPosVisual == null || oldPosVisual.Length != oldPosLength)
                                oldPosVisual = new Vector2[oldPosLength];
                            for (int i = oldPosLength - 1; i > 0; i--)
                            {
                                oldPosVisual[i] = oldPosVisual[i - 1];
                            }
                            oldPosVisual[0] = player.position;
                        }
                    }
                    else
                    {
                        oldPosVisual = null;
                    }
                }

                if (info.shadow == 0f && aQPlayer.blueSpheres && aQPlayer.celesteTorusOffsetsForDrawing != null)
                {
                    var texture = TextureGrabber.GetProjectile(ModContent.ProjectileType<CelesteTorusCollider>());
                    var frame = new Rectangle(0, 0, texture.Width, texture.Height);
                    var orig = frame.Size() / 2f;
                    for (int i = 0; i < MaxCelesteTorusOrbs; i++)
                    {
                        var position = aQPlayer.GetCelesteTorusPositionOffset(i);
                        float layerValue = AQUtils.OmegaStarite3DHelper.GetParralaxScale(1f, aQPlayer.celesteTorusOffsetsForDrawing[i].Z * CELESTE_Z_MULT);
                        if (layerValue >= 1f)
                        {
                            var center = info.position + new Vector2(player.width / 2 + (int)position.X, player.height / 2 + (int)position.Y);
                            Main.playerDrawData.Add(new DrawData(texture, AQUtils.OmegaStarite3DHelper.GetParralaxPosition(center, aQPlayer.celesteTorusOffsetsForDrawing[i].Z * AQPlayer.CELESTE_Z_MULT) - Main.screenPosition, frame, Lighting.GetColor((int)(center.X / 16f), (int)(center.Y / 16f)), 0f, orig, AQUtils.OmegaStarite3DHelper.GetParralaxScale(aQPlayer.celesteTorusScale, aQPlayer.celesteTorusOffsetsForDrawing[i].Z * AQPlayer.CELESTE_Z_MULT), SpriteEffects.None, 0) { shader = aQPlayer.cCelesteTorus, ignorePlayerRotation = true });
                        }
                    }
                }
            });
            public static readonly PlayerLayer PostDrawHeldItem = new PlayerLayer("AQMod", "PostDrawHeldItem", (info) =>
            {
                var player = info.drawPlayer;
                Item item = player.inventory[player.selectedItem];

                if (info.shadow != 0f || player.frozen || ((player.itemAnimation <= 0 || item.useStyle == 0) &&
                (item.holdStyle <= 0 || player.pulley)) || item.type <= ItemID.None || player.dead || item.noUseGraphic ||
                (item.noWet && player.wet) || item.type < Main.maxItemTypes)
                {
                    return;
                }

                if (item.modItem is IItemOverlaysPlayerDraw itemOverlay)
                {
                    itemOverlay.PlayerDraw.DrawUse(player, player.GetModPlayer<AQPlayer>(), item, info);
                }
                AQMod.ItemOverlays.GetOverlay(item.type)?.DrawHeld(player, player.GetModPlayer<AQPlayer>(), item, info);
            });
            public static readonly PlayerLayer PostDrawHead = new PlayerLayer("AQMod", "PostDrawHead", (info) =>
            {
                var player = info.drawPlayer;
                var aQPlayer = info.drawPlayer.GetModPlayer<AQPlayer>();
                float opacity = 1f - info.shadow;
                const float MagicOffsetForReversedGravity = 8f;
                int headFrame = info.drawPlayer.bodyFrame.Y / FRAME_HEIGHT;
                float gravityOffset = 0f;
                AQMod.ArmorOverlays.InvokeArmorOverlay(EquipLayering.Head, info);
                if (info.drawPlayer.gravDir == -1)
                    gravityOffset = MagicOffsetForReversedGravity;
                if (aQPlayer.mask >= 0)
                {
                    Vector2 position = new Vector2((int)(info.position.X - Main.screenPosition.X - info.drawPlayer.bodyFrame.Width / 2 + info.drawPlayer.width / 2), (int)(info.position.Y - Main.screenPosition.Y + info.drawPlayer.height - info.drawPlayer.bodyFrame.Height + gravityOffset)) + info.drawPlayer.headPosition + info.headOrigin;
                    Color color = Lighting.GetColor((int)info.position.X / 16, (int)(info.position.Y + gravityOffset) / 16) * opacity;
                    switch ((PlayerMaskID)aQPlayer.mask)
                    {
                        default:
                            {
                                Main.playerDrawData.Add(new DrawData(ModContent.GetTexture(MasksPath + aQPlayer.mask), position, info.drawPlayer.bodyFrame, color, info.drawPlayer.headRotation, info.headOrigin, 1f, info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                            }
                            break;

                        case PlayerMaskID.CataMask:
                            {
                                if (aQPlayer.cMask > 0)
                                    aQPlayer.cataEyeColor = new Color(100, 100, 100, 0);
                                Main.playerDrawData.Add(new DrawData(ModContent.GetTexture(MasksPath + aQPlayer.mask), position, info.drawPlayer.bodyFrame, color, info.drawPlayer.headRotation, info.headOrigin, 1f, info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                if (player.statLife == player.statLifeMax2 && info.drawPlayer.headRotation == 0)
                                {
                                    var texture = AQTextures.Lights[LightTex.Spotlight240x66];
                                    var frame = new Rectangle(0, 0, texture.Width, texture.Height);
                                    var orig = frame.Size() / 2f;
                                    var scale = new Vector2((float)(Math.Sin(Main.GlobalTime * 10f) + 1f) * 0.04f + 0.2f, 0.1f);
                                    var eyeGlowPos = position + new Vector2(2f * player.direction, Main.OffsetsPlayerHeadgear[headFrame].Y);
                                    var eyeGlowColor = aQPlayer.cataEyeColor;
                                    var value = AQUtils.GetGrad(0.25f, 0.45f, scale.X) * 0.5f;
                                    var config = ModContent.GetInstance<AQConfigClient>();
                                    var colorMult = ModContent.GetInstance<AQConfigClient>().EffectIntensity * (1f - info.shadow);
                                    Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * colorMult, 0f, orig, scale, info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                    Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * 0.3f * colorMult, 0f, orig, scale * (1.1f + value * 2), info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                    if (ModContent.GetInstance<AQConfigClient>().EffectQuality > 0.5f)
                                    {
                                        Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * 0.35f * colorMult, MathHelper.PiOver4, orig, scale * (1f - value) * 0.75f, info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                        Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * 0.35f * colorMult, -MathHelper.PiOver4, orig, scale * (1f - value) * 0.75f, info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                        Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * 0.2f * colorMult, MathHelper.PiOver2, orig, scale * (1f - value), info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                    }
                                    Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * colorMult, MathHelper.PiOver2, orig, scale * 0.5f, info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                    if (ModContent.GetInstance<AQConfigClient>().EffectIntensity > 1.5f && ModContent.GetInstance<AQConfigClient>().EffectQuality > 0.5f)
                                        Main.playerDrawData.Add(new DrawData(texture, eyeGlowPos, frame, eyeGlowColor * 0.15f * colorMult, 0f, orig, scale * (2f + value * 3f), info.spriteEffects, 0) { shader = aQPlayer.cMask, });
                                }
                            }
                            break;
                    }
                }
                if (aQPlayer.headAcc >= 0)
                {
                    Vector2 position = new Vector2((int)(info.position.X - Main.screenPosition.X - info.drawPlayer.bodyFrame.Width / 2 + info.drawPlayer.width / 2), (int)(info.position.Y - Main.screenPosition.Y + info.drawPlayer.height - info.drawPlayer.bodyFrame.Height + gravityOffset)) + info.drawPlayer.headPosition + info.headOrigin;
                    Color color = Lighting.GetColor((int)info.position.X / 16, (int)info.position.Y / 16) * opacity;
                    int shader = aQPlayer.cHeadAcc;
                    switch (aQPlayer.headAcc)
                    {
                        default:
                            {
                                Main.playerDrawData.Add(new DrawData(ModContent.GetTexture(HeadAccsPath + aQPlayer.headAcc), position, info.drawPlayer.bodyFrame, color, info.drawPlayer.headRotation, info.headOrigin, 1f, info.spriteEffects, 0) { shader = shader, });
                            }
                            break;

                        case PlayerHeadOverlayID.MonoxideHat:
                            {
                                Main.playerDrawData.Add(new DrawData(ModContent.GetTexture(HeadAccsPath + aQPlayer.headAcc), position, info.drawPlayer.bodyFrame, color, info.drawPlayer.headRotation, info.headOrigin, 1f, info.spriteEffects, 0) { shader = shader, });
                                Main.playerDrawData.Add(new DrawData(ModContent.GetTexture(HeadAccsPath + PlayerHeadOverlayID.MonoxideHatGlow), position, info.drawPlayer.bodyFrame, new Color(opacity * 0.99f, opacity * 0.99f, opacity * 0.99f, 0f), info.drawPlayer.headRotation, info.headOrigin, 1f, info.spriteEffects, 0) { shader = shader, });
                                if (aQPlayer.monoxiderBird && !aQPlayer.chomper)
                                {
                                    var hatPos = position;
                                    if (player.gravDir == -1)
                                    {
                                        hatPos.Y += player.height + Main.OffsetsPlayerHeadgear[headFrame].Y + 8f;
                                    }
                                    else
                                    {
                                        hatPos.Y += Main.OffsetsPlayerHeadgear[headFrame].Y;
                                    }
                                    Projectiles.Summon.Monoxider.DrawHead(player, aQPlayer, hatPos, ignorePlayerRotation: false);
                                }
                            }
                            break;

                        case PlayerHeadOverlayID.FishyFins:
                            {
                                Main.playerDrawData.Add(new DrawData(ModContent.GetTexture(HeadAccsPath + aQPlayer.headAcc), position, info.drawPlayer.bodyFrame, Lighting.GetColor((int)info.position.X / 16, (int)info.position.Y / 16, info.drawPlayer.skinColor), info.drawPlayer.headRotation, info.headOrigin, 1f, info.spriteEffects, 0) { shader = shader, });
                            }
                            break;
                    }
                }
            });
            public static readonly PlayerLayer PostDrawBody = new PlayerLayer("AQMod", "PostDrawBody", (info) =>
            {
                AQMod.ArmorOverlays.InvokeArmorOverlay(EquipLayering.Body, info);
            });
        }

        public static int oldPosLength;
        public static Vector2[] oldPosVisual;
        public static bool arachnotronHeadTrail;
        public static bool arachnotronBodyTrail;
        internal static bool Fidget_Spinner_Force_Autoswing;
        internal static int _moneyTroughHackIndex = -1;
        internal static ISuperClunkyMoneyTroughTypeThing _moneyTroughHack;

        public Vector3[] celesteTorusOffsetsForDrawing;

        public float discountPercentage;
        public bool blueSpheres;
        public bool hyperCrystal;
        public bool monoxiderBird;
        public bool sparkling;
        public bool chloroTransfer;
        public bool altEvilDrops;
        public bool breadsoul;
        public bool moonShoes;
        public bool extractinator;
        public bool copperSeal;
        public bool silverSeal;
        public bool goldSeal;
        public bool canDash;
        public bool dartHead;
        public int dartHeadType;
        public int dartHeadDelay;
        public int dartTrapHatTimer;
        public int extraFlightTime;
        public int thunderbirdJumpTimer;
        public int thunderbirdLightningTimer;
        public bool dreadsoul;
        public bool arachnotron;
        public bool primeTime;
        public bool omori;
        public int omoriDeathTimer;
        public int spelunkerEquipTimer;
        public bool microStarite;
        public byte spoiled;
        public bool wyvernAmulet;
        public bool voodooAmulet;
        public bool ghostAmulet;
        public bool extractinatorVisible;
        public float celesteTorusX;
        public float celesteTorusY;
        public float celesteTorusZ;
        public float celesteTorusRadius;
        public int celesteTorusDamage;
        public float celesteTorusKnockback;
        public int celesteTorusMaxRadius;
        public float celesteTorusSpeed;
        public float celesteTorusScale;
        public bool stariteMinion;
        public bool spicyEel;
        public bool striderPalms;
        public bool striderPalmsOld;
        public bool wyvernAmuletHeld;
        public bool voodooAmuletHeld;
        public bool ghostAmuletHeld;
        public bool[] veinmineTiles;
        public bool degenerationRing;
        public ushort shieldLife;

        public bool crimsonHands;
        public bool chomper;
        public bool trapperImp;

        public bool cosmicMap;
        public bool dungeonMap;
        public bool lihzahrdMap;
        public bool retroMap;
        public bool showCosmicMap = true;
        public bool showDungeonMap = true;
        public bool showLihzahrdMap = true;
        public bool showRetroMap = true;
        public byte nearGlobe;
        public ushort globeX;
        public ushort globeY;
        public bool hasMinionCarry;
        public int headMinionCarryX;
        public int headMinionCarryY;
        public int headMinionCarryXOld;
        public int headMinionCarryYOld;
        public Color cataEyeColor;
        public byte monoxiderCarry;
        public int headAcc = -1;
        public int mask = -1;
        public int cHeadAcc;
        public int cMask;
        public int cCelesteTorus;
        public bool heartMoth;
        public bool notFrostburn;
        public bool bossrush;
        public bool bossrushOld;
        public float grabReachMult; // until 1.4 comes
        public bool neutronYogurt;
        public bool mothmanMask;
        public byte mothmanExplosionDelay;
        public sbyte temperature;
        public byte temperatureRegen;
        public bool pickBreak;
        public bool crabAx;
        public sbyte redSpriteWind;
        public byte extraHP;
        public bool fidgetSpinner;
        public bool mysticUmbrellaDelay;
        public bool ignoreMoons;
        public bool cosmicanon;
        public bool antiGravityItems;
        public bool equivalenceMachine;
        public bool hotAmulet;
        public bool coldAmulet;
        public bool shockCollar;
        public bool healBeforeDeath;
        public bool glowString;

        public bool NetUpdateKillCount;
        public int[] CurrentEncoreKillCount { get; private set; }
        public int[] EncoreBossKillCountRecord { get; private set; }
        public int PopperType { get; set; }
        public int PopperBaitPower { get; set; }
        public int FishingPowerCache { get; set; }
        public int ExtractinatorCount { get; set; }
        public int CursorDyeID { get; private set; } = CursorDyeManager.ID.None;
        public string CursorDye { get; private set; } = "";
        public bool IgnoreIgnoreMoons { get; set; }
        public bool IgnoreAntiGravityItems { get; set; }

        public static bool IsQuickBuffing { get; internal set; }

        public bool AtmosphericCurrentsEvent => player.ZoneSkyHeight && Main.windSpeed > 30f;

        public override void Initialize()
        {
            omoriDeathTimer = 1;
            arachnotron = false;
            spoiled = 0;
            sparkling = false;
            nearGlobe = 0;
            headMinionCarryX = 0;
            headMinionCarryY = 0;
            headMinionCarryXOld = 0;
            headMinionCarryYOld = 0;
            headAcc = -1;
            mask = -1;
            CursorDyeID = 0;
            cHeadAcc = 0;
            cMask = 0;
            cCelesteTorus = 0;
            monoxiderCarry = 0;
            cataEyeColor = new Color(50, 155, 255, 0);
            showCosmicMap = true;
            showDungeonMap = true;
            showLihzahrdMap = true;
            showRetroMap = true;
            oldPosLength = 0;
            oldPosVisual = null;
            arachnotronHeadTrail = false;
            arachnotronBodyTrail = false;
            _moneyTroughHack = null;
            _moneyTroughHackIndex = -1;
            notFrostburn = false;
            bossrush = false;
            bossrushOld = false;
            CurrentEncoreKillCount = new int[NPCLoader.NPCCount];
            EncoreBossKillCountRecord = new int[NPCLoader.NPCCount];
            grabReachMult = 1f;
            temperature = 0;
            pickBreak = false;
            fidgetSpinner = false;
            mysticUmbrellaDelay = false;
        }

        public override void OnEnterWorld(Player player)
        {
            if (!Main.dayTime && Main.netMode != NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                GlimmerEventSky.InitNight();
        }

        public byte[] SerializeBossKills()
        {
            var writer = new BinaryWriter(new MemoryStream(1024));
            if (EncoreBossKillCountRecord == null)
            {
                writer.Write(false);
                return ((MemoryStream)writer.BaseStream).GetBuffer();
            }
            writer.Write(true);
            writer.Write((byte)0);
            for (int i = 0; i < EncoreBossKillCountRecord.Length; i++)
            {
                if (EncoreBossKillCountRecord[i] != 0)
                {
                    writer.Write(true);
                    if (i >= Main.maxNPCTypes)
                    {
                        writer.Write(true);
                        var ModNPC = NPCLoader.GetNPC(i);
                        writer.Write(ModNPC.mod.Name);
                        writer.Write(ModNPC.Name);
                        writer.Write(EncoreBossKillCountRecord[i]);
                    }
                    else
                    {
                        writer.Write(false);
                        writer.Write(i);
                        writer.Write(EncoreBossKillCountRecord[i]);
                    }
                }
            }
            writer.Write(false);
            return ((MemoryStream)writer.BaseStream).GetBuffer();
        }

        public void DeserialzeBossKills(byte[] buffer)
        {
            var reader = new BinaryReader(new MemoryStream(buffer));
            if (!reader.ReadBoolean())
                return;
            byte save = reader.ReadByte();
            while (reader.ReadBoolean())
            {
                if (reader.ReadBoolean())
                {
                    string mod = reader.ReadString();
                    string name = reader.ReadString();
                    int kills = reader.ReadInt32();
                    try
                    {
                        var Mod = ModLoader.GetMod(mod);
                        if (Mod == null)
                            continue;
                        int type = Mod.NPCType(name);
                        if (type != -1)
                            EncoreBossKillCountRecord[type] = kills;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    int type = reader.ReadInt32();
                    int kills = reader.ReadInt32();
                    EncoreBossKillCountRecord[type] = kills;
                }
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["extractinatorCount"] = ExtractinatorCount,
                ["CursorDye"] = CursorDye,
                ["bosskills"] = SerializeBossKills(),
                ["IgnoreIgnoreMoons"] = IgnoreIgnoreMoons,
                ["IgnoreAntiGravityItems"] = IgnoreAntiGravityItems,
            };
        }

        public override void Load(TagCompound tag)
        {
            IgnoreAntiGravityItems = tag.GetBool("IgnoreAntiGravityItems");
            IgnoreIgnoreMoons = tag.GetBool("IgnoreIgnoreMoons");
            ExtractinatorCount = tag.GetInt("extractinatorCount");
            string dyeKey = tag.GetString("CursorDye");
            if (!string.IsNullOrEmpty(dyeKey) && AQStringCodes.DecodeName(dyeKey, out string cursorDyeMod, out string cursorDyeName))
            {
                SetCursorDye(CursorDyeManager.Instance.GetContentID(cursorDyeMod, cursorDyeName));
            }
            else
            {
                SetCursorDye(CursorDyeManager.ID.None);
            }
            byte[] buffer = tag.GetByteArray("bosskills");
            if (buffer == null || buffer.Length == 0)
                return;
            DeserialzeBossKills(buffer);
        }

        public override void UpdateBiomeVisuals()
        {
            if (_moneyTroughHack == null)
                _moneyTroughHackIndex = -1;
            if (_moneyTroughHackIndex > -1)
            {
                if (player.flyingPigChest >= 0 || player.chest != -3 || !Main.projectile[_moneyTroughHackIndex].active || Main.projectile[_moneyTroughHackIndex].type != ModContent.ProjectileType<ATM>())
                {
                    _moneyTroughHackIndex = -1;
                    _moneyTroughHack = null;
                }
                else
                {
                    player.chestX = ((int)Main.projectile[_moneyTroughHackIndex].position.X + Main.projectile[_moneyTroughHackIndex].width / 2) / 16;
                    player.chestY = ((int)Main.projectile[_moneyTroughHackIndex].position.Y + Main.projectile[_moneyTroughHackIndex].height / 2) / 16;
                    if (!player.IsInTileInteractionRange(player.chestX, player.chestY))
                    {
                        if (player.chest != -1)
                            _moneyTroughHack.OnClose();
                        player.flyingPigChest = -1;
                        _moneyTroughHackIndex = -1;
                        player.chest = -1;
                        Recipe.FindRecipes();
                    }
                    else
                    {
                        player.flyingPigChest = _moneyTroughHackIndex;
                        player.chest = -2;
                        Main.projectile[_moneyTroughHackIndex].type = ProjectileID.FlyingPiggyBank;
                    }
                }
            }
            if (!Main.gamePaused && Main.instance.IsActive)
                ScreenShakeManager.Update();

            bool glimmerEvent = (GlimmerEvent.IsGlimmerEventCurrentlyActive() || OmegaStariteScenes.OmegaStariteIndexCache != -1) && Main.screenPosition.Y < Main.worldSurface * 16f + Main.screenHeight;
            AQUtils.UpdateSky(glimmerEvent, GlimmerEventSky.Name);

            if (glimmerEvent && OmegaStariteScenes.OmegaStariteIndexCache == -1 && ModContent.GetInstance<StariteConfig>().UltimateSwordVignette)
            {
                float intensity = 0f;
                float distance = (Main.player[Main.myPlayer].position.X - (GlimmerEvent.tileX * 16f + 8f)).Abs();
                if (distance < 6400f)
                {
                    intensity += 1f - distance / 6400f;
                }

                var filter = EffectCache.f_Vignette;
                var shader = EffectCache.f_Vignette.GetShader();
                shader.UseIntensity(intensity * 1.25f);
                if (!EffectCache.f_Vignette.IsActive())
                {
                    Filters.Scene.Activate(EffectCache.fn_Vignette);
                }
            }
            else
            {
                if (EffectCache.f_Vignette.IsActive())
                    Filters.Scene.Deactivate(EffectCache.fn_Vignette);
            }
        }

        private void UpdateTemperatureRegen()
        {
            if (temperature != 0)
            {
                sbyte minTemp = -100;
                sbyte maxTemp = 100;
                if (coldAmulet)
                {
                    minTemp = -60;
                }
                if (hotAmulet)
                {
                    maxTemp = 60;
                }
                if (temperature < minTemp)
                {
                    temperature = minTemp;
                }
                else if (temperature > maxTemp)
                {
                    temperature = maxTemp;
                }
                if (temperatureRegen == 0)
                {
                    temperatureRegen = TEMPERATURE_REGEN_NORMAL;
                    if (player.resistCold && temperature < 0)
                    {
                        temperatureRegen = TEMPERATURE_REGEN_FROST_ARMOR_COLD_TEMP;
                    }
                    if (temperature < 0)
                    {
                        temperature++;
                    }
                    else
                    {
                        temperature--;
                    }
                }
                else
                {
                    temperatureRegen--;
                }
            }
            else
            {
                temperatureRegen = TEMPERATURE_REGEN_ON_HIT;
            }
        }

        public override void ResetEffects()
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (_moneyTroughHackIndex > -1)
                {
                    player.flyingPigChest = -1;
                    player.chest = _moneyTroughHack.ChestType;
                    Main.projectile[_moneyTroughHackIndex].type = _moneyTroughHack.ProjectileType;
                }
            }
            blueSpheres = false;
            discountPercentage = 0.8f;
            hyperCrystal = false;
            monoxiderBird = false;
            sparkling = false;
            moonShoes = false;
            canDash = !(player.setSolar || player.mount.Active);
            copperSeal = false;
            silverSeal = false;
            goldSeal = false;
            extraFlightTime = 0;
            dreadsoul = false;
            breadsoul = false;
            arachnotron = false;
            primeTime = false;
            omori = false;
            microStarite = false;
            spoiled = 0;
            wyvernAmulet = false;
            voodooAmulet = false;
            ghostAmulet = false;
            extractinatorVisible = false;
            altEvilDrops = false;
            stariteMinion = false;
            spicyEel = false;
            striderPalmsOld = striderPalms;
            striderPalms = false;
            ghostAmuletHeld = InVanitySlot(player, ModContent.ItemType<GhostAmulet>());
            voodooAmuletHeld = InVanitySlot(player, ModContent.ItemType<VoodooAmulet>());
            wyvernAmuletHeld = InVanitySlot(player, ModContent.ItemType<WyvernAmulet>());
            veinmineTiles = new bool[TileLoader.TileCount];
            shieldLife = 0;

            crimsonHands = false;
            chomper = false;
            trapperImp = false;

            dungeonMap = false;
            lihzahrdMap = false;
            headMinionCarryXOld = headMinionCarryX;
            headMinionCarryYOld = headMinionCarryY;
            headMinionCarryX = 0;
            headMinionCarryY = 0;
            headAcc = -1;
            mask = -1;
            cHeadAcc = 0;
            cMask = 0;
            cCelesteTorus = 0;
            monoxiderCarry = 0;
            cataEyeColor = new Color(50, 155, 255, 0);
            heartMoth = false;
            notFrostburn = false;
            grabReachMult = 1f;
            mothmanMask = false;
            pickBreak = false;
            crabAx = false;
            fidgetSpinner = false;
            mysticUmbrellaDelay = false;
            cosmicanon = false;
            ignoreMoons = false;
            antiGravityItems = false;
            equivalenceMachine = false;
            shockCollar = false;
            healBeforeDeath = false;
            glowString = false;
            if (extraHP > 60) // to cap life max buffs at 60
            {
                extraHP = 60;
            }
            player.statLifeMax2 += extraHP;
            extraHP = 0;
            UpdateTemperatureRegen();
            hotAmulet = false;
            coldAmulet = false;
            if (GaleStreams.EventActive(player))
            {
                if (temperature < -60)
                {
                    player.AddBuff(ModContent.BuffType<Cold60>(), 4);
                }
                else if (temperature < -40)
                {
                    player.AddBuff(ModContent.BuffType<Cold40>(), 4);
                }
                else if (temperature < -20)
                {
                    player.AddBuff(ModContent.BuffType<Cold20>(), 4);
                }
                else if (temperature > 60)
                {
                    player.AddBuff(ModContent.BuffType<Hot60>(), 4);
                }
                else if (temperature > 40)
                {
                    player.AddBuff(ModContent.BuffType<Hot40>(), 4);
                }
                else if (temperature > 20)
                {
                    player.AddBuff(ModContent.BuffType<Hot20>(), 4);
                }
            }
            if (mothmanExplosionDelay > 0)
                mothmanExplosionDelay--;
            if (bossrushOld != bossrush)
            {
                if (bossrush)
                {

                }
                else
                {
                    CurrentEncoreKillCount = new int[NPCLoader.NPCCount];
                }
            }
            bossrushOld = bossrush;
            bossrush = false;
            if (nearGlobe > 0)
                nearGlobe--;
            if (!dartHead)
                dartTrapHatTimer = 240;
            dartHead = false;
            if (thunderbirdJumpTimer > 0)
            {
                canDash = false;
                thunderbirdJumpTimer--;
            }
            if (thunderbirdLightningTimer > 0)
                thunderbirdLightningTimer--;
            if (canDash)
            {
                for (int i = 3; i < 8 + player.extraAccessorySlots; i++)
                {
                    Item item = player.armor[i];
                    if (item.type == ItemID.EoCShield || item.type == ItemID.MasterNinjaGear || item.type == ItemID.Tabi)
                    {
                        canDash = false;
                        break;
                    }
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (cosmicanon && AQMod.Keybinds.CosmicanonToggle.JustPressed)
            {
                IgnoreIgnoreMoons = !IgnoreIgnoreMoons;
                if (IgnoreIgnoreMoons)
                {
                    Main.NewText(Language.GetTextValue("Mods.AQMod.ToggleCosmicanon.False"), new Color(230, 230, 255, 255));
                }
                else
                {
                    Main.NewText(Language.GetTextValue("Mods.AQMod.ToggleCosmicanon.True"), new Color(230, 230, 255, 255));
                }
            }
            if (equivalenceMachine && AQMod.Keybinds.EquivalenceMachineToggle.JustPressed)
            {
                IgnoreAntiGravityItems = !IgnoreAntiGravityItems;
                if (IgnoreAntiGravityItems)
                {
                    Main.NewText(Language.GetTextValue("Mods.AQMod.EquivalenceMachineToggle.False"), new Color(230, 230, 255, 255));
                }
                else
                {
                    Main.NewText(Language.GetTextValue("Mods.AQMod.EquivalenceMachineToggle.True"), new Color(230, 230, 255, 255));
                }
            }
        }

        public override Texture2D GetMapBackgroundImage()
        {
            if (XmasSeeds.XmasWorld && WorldGen.gen)
            {
                return ModContent.GetTexture("Terraria/MapBG12");
            }
            if (!player.ZoneCorrupt && !player.ZoneCrimson && !player.ZoneHoly && !player.ZoneDesert && !player.ZoneJungle)
            {
                if (player.position.Y < Main.worldSurface * 16f)
                {
                    if (GlimmerEvent.IsGlimmerEventCurrentlyActive())
                        return ModContent.GetTexture("AQMod/Assets/Map/Backgrounds/GlimmerEvent");
                }
            }
            return null;
        }

        public override void clientClone(ModPlayer clientClone)
        {
            var clone = (AQPlayer)clientClone;
            clone.celesteTorusX = celesteTorusX;
            clone.celesteTorusY = celesteTorusY;
            clone.celesteTorusZ = celesteTorusZ;
            clone.CurrentEncoreKillCount = CurrentEncoreKillCount;
            clone.EncoreBossKillCountRecord = EncoreBossKillCountRecord;
            clone.breadsoul = breadsoul;
            clone.dreadsoul = dreadsoul;
            clone.dartHead = dartHead;
            clone.dartHeadType = dartHeadType;
            clone.arachnotron = arachnotron;
            clone.blueSpheres = blueSpheres;
        }

        public override bool PreItemCheck()
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var item = player.inventory[player.selectedItem];
                Fidget_Spinner_Force_Autoswing = false;
                if (fidgetSpinner && player.selectedItem < Main.maxInventory && (Main.mouseItem == null || Main.mouseItem.type <= ItemID.None))
                {
                    if (CanForceAutoswing(player, item, ignoreChanneled: false))
                    {
                        Fidget_Spinner_Force_Autoswing = true;
                        item.autoReuse = true;
                    }
                }
                bool canMine = CanReach(player, item);
                if (player.noBuilding)
                    canMine = false;
                if (Main.mouseRight || !canMine)
                    crabAx = false;
                else if (!crabAx)
                    crabAx = item.type == ModContent.ItemType<Items.Tools.Crabax>();
                if (crabAx && (item.axe > 0))
                {
                    if (Main.tile[Player.tileTargetX, Player.tileTargetY].active() && player.toolTime <= 1 && player.itemAnimation > 0 && player.controlUseItem)
                    {
                        var rectangle = new Rectangle((int)(player.position.X + player.width / 2) / 16, (int)(player.position.Y + player.height / 2) / 16, 30, 30);
                        rectangle.X -= rectangle.Width / 2;
                        rectangle.Y -= rectangle.Height / 2;
                        int hitCount = 0;
                        const int HitCountMax = 8;
                        if (rectangle.X > 10 && rectangle.X < Main.maxTilesX - 10 && rectangle.Y > 10 && rectangle.Y < Main.maxTilesY - 10)
                        {
                            for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++)
                            {
                                for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++)
                                {
                                    if (Main.tile[i, j] == null)
                                    {
                                        Main.tile[i, j] = new Tile();
                                        continue;
                                    }
                                    if (Main.tile[i, j].active() && Main.tileAxe[Main.tile[i, j].type])
                                    {
                                        int tileID = player.hitTile.HitObject(i, j, 1);
                                        int tileDamage = 0;
                                        if (Main.tile[i, j].type == 80)
                                        {
                                            tileDamage += item.axe * 3;
                                        }
                                        else
                                        {
                                            TileLoader.MineDamage(item.axe, ref tileDamage);
                                        }
                                        if (Main.tile[i, j].type == TileID.Trees)
                                        {
                                            int treeStumpX = i;
                                            int treeStumpY = j;

                                            if (Main.tile[treeStumpX, treeStumpY].frameY >= 198 && Main.tile[treeStumpX, treeStumpY].frameX == 44)
                                            {
                                                treeStumpX++;
                                            }
                                            if (Main.tile[treeStumpX, treeStumpY].frameX == 66 && Main.tile[treeStumpX, treeStumpY].frameY <= 44)
                                            {
                                                treeStumpX++;
                                            }
                                            if (Main.tile[treeStumpX, treeStumpY].frameX == 44 && Main.tile[treeStumpX, treeStumpY].frameY >= 132 && Main.tile[treeStumpX, treeStumpY].frameY <= 176)
                                            {
                                                treeStumpX++;
                                            }
                                            if (Main.tile[treeStumpX, treeStumpY].frameY >= 198 && Main.tile[treeStumpX, treeStumpY].frameX == 66)
                                            {
                                                treeStumpX--;
                                            }
                                            if (Main.tile[treeStumpX, treeStumpY].frameX == 88 && Main.tile[treeStumpX, treeStumpY].frameY >= 66 && Main.tile[treeStumpX, treeStumpY].frameY <= 110)
                                            {
                                                treeStumpX--;
                                            }
                                            if (Main.tile[treeStumpX, treeStumpY].frameX == 22 && Main.tile[treeStumpX, treeStumpY].frameY >= 132 && Main.tile[treeStumpX, treeStumpY].frameY <= 176)
                                            {
                                                treeStumpX--;
                                            }

                                            i = treeStumpX + 2; // skips the current index and the next one, since this entire tree has been completed
                                            j = rectangle.Y;

                                            for (; Main.tile[treeStumpX, treeStumpY].active() && Main.tile[treeStumpX, treeStumpY].type == TileID.Trees && Main.tile[treeStumpX, treeStumpY + 1].type == TileID.Trees; treeStumpY++)
                                            {
                                            }

                                            if (Player.tileTargetX == treeStumpX && Player.tileTargetY == treeStumpY)
                                            {
                                                break;
                                            }

                                            AchievementsHelper.CurrentlyMining = true;
                                            if (!WorldGen.CanKillTile(treeStumpX, treeStumpY))
                                            {
                                                tileDamage = 0;
                                            }
                                            tileID = player.hitTile.HitObject(treeStumpX, treeStumpY, 1);
                                            if (player.hitTile.AddDamage(tileID, tileDamage) >= 100)
                                            {
                                                player.hitTile.Clear(tileID);
                                                WorldGen.KillTile(treeStumpX, treeStumpY);
                                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                                {
                                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, treeStumpX, treeStumpY);
                                                }
                                            }
                                            else
                                            {
                                                WorldGen.KillTile(i, j, fail: true);
                                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                                {
                                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, treeStumpX, treeStumpY, 1f);
                                                }
                                            }
                                            if (tileDamage != 0)
                                            {
                                                player.hitTile.Prune();
                                                hitCount++;
                                                if (hitCount > HitCountMax)
                                                {
                                                    break;
                                                }
                                            }
                                            AchievementsHelper.CurrentlyMining = false;
                                            continue;
                                        }
                                        else if (Main.tile[i, j].type == TileID.PalmTree)
                                        {
                                            int treeStumpX = i;
                                            int treeStumpY = j;

                                            for (; Main.tile[treeStumpX, treeStumpY].active() && Main.tile[treeStumpX, treeStumpY].type == TileID.PalmTree && Main.tile[treeStumpX, treeStumpY + 1].type == TileID.PalmTree; treeStumpY++)
                                            {
                                            }

                                            i = treeStumpX + 2; // skips the current index and the next one, since this entire tree has been completed
                                            j = rectangle.Y;

                                            if (Player.tileTargetX == treeStumpX && Player.tileTargetY == treeStumpY)
                                            {
                                                break;
                                            }

                                            AchievementsHelper.CurrentlyMining = true;
                                            if (!WorldGen.CanKillTile(treeStumpX, treeStumpY))
                                            {
                                                tileDamage = 0;
                                            }
                                            tileID = player.hitTile.HitObject(treeStumpX, treeStumpY, 1);
                                            if (player.hitTile.AddDamage(tileID, tileDamage) >= 100)
                                            {
                                                player.hitTile.Clear(tileID);
                                                WorldGen.KillTile(treeStumpX, treeStumpY);
                                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                                {
                                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, treeStumpX, treeStumpY);
                                                }
                                            }
                                            else
                                            {
                                                WorldGen.KillTile(i, j, fail: true);
                                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                                {
                                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, treeStumpX, treeStumpY, 1f);
                                                }
                                            }
                                            if (tileDamage != 0)
                                            {
                                                player.hitTile.Prune();
                                                hitCount++;
                                                if (hitCount > HitCountMax)
                                                {
                                                    break;
                                                }
                                            }
                                            AchievementsHelper.CurrentlyMining = false;
                                            continue;
                                        }
                                        else
                                        {
                                            AchievementsHelper.CurrentlyMining = true;
                                            if (!WorldGen.CanKillTile(i, j))
                                            {
                                                tileDamage = 0;
                                            }
                                            if (player.hitTile.AddDamage(tileID, tileDamage) >= 100)
                                            {
                                                player.hitTile.Clear(tileID);
                                                WorldGen.KillTile(i, j);
                                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                                {
                                                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, i, j);
                                                }
                                            }
                                            else
                                            {
                                                WorldGen.KillTile(i, j, fail: true);
                                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                                {
                                                    NetMessage.SendData(17, -1, -1, null, 0, i, j, 1f);
                                                }
                                            }
                                            if (tileDamage != 0)
                                            {
                                                player.hitTile.Prune();
                                                hitCount++;
                                                if (hitCount > HitCountMax)
                                                {
                                                    break;
                                                }
                                            }
                                            AchievementsHelper.CurrentlyMining = false;
                                        }
                                    }
                                }
                                if (hitCount > HitCountMax)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return base.PreItemCheck();
        }

        public override void PostItemCheck()
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (Fidget_Spinner_Force_Autoswing)
                {
                    player.inventory[player.selectedItem].autoReuse = false;
                    Fidget_Spinner_Force_Autoswing = false;
                }
                if (player.itemAnimation < 1 && player.inventory[player.selectedItem].modItem is ISpecialFood)
                {
                    player.inventory[player.selectedItem].buffType = BuffID.WellFed;
                }
            }
        }

        public override float UseTimeMultiplier(Item item)
        {
            if (Fidget_Spinner_Force_Autoswing && item.damage > 0 && !item.channel)
            {
                if (item.useTime <= 10)
                {
                    return 0.3f;
                }
                if (item.useTime < 32)
                {
                    return 0.6f;
                }
            }
            return base.UseTimeMultiplier(item);
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (AQPlayer)clientPlayer;
            if (clone.bossrush)
            {
                NetUpdateKillCount = true;
                SyncPlayer(-1, player.whoAmI, true);
            }
            else
            {
                if (clone.blueSpheres)
                {
                    Sync_CelesteTorus(toWho: -1, fromWho: -1);
                }
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = mod.GetPacket();
            if (NetUpdateKillCount)
            {
                //packet.Write(AQPacketID.UpdateAQPlayerEncoreKills);
                //packet.Write((byte)player.whoAmI);
                //byte[] buffer = SerializeBossKills();
                //packet.Write(buffer, 0, buffer.Length);
                //packet.Send(toWho, fromWho);
                NetUpdateKillCount = false;
                return;
            }
            Sync_CelesteTorus(toWho, fromWho);
        }

        private void Sync_CelesteTorus(int toWho = -1, int fromWho = -1)
        {
            return;
            //var packet = mod.GetPacket();
            //packet.Write(AQPacketID.UpdateAQPlayerCelesteTorus);
            //packet.Write((byte)player.whoAmI);
            //packet.Write(celesteTorusX);
            //packet.Write(celesteTorusY);
            //packet.Write(celesteTorusZ);
            //packet.Send(toWho, fromWho);
        }

        public override void UpdateDead()
        {
            omori = false;
            blueSpheres = false;
            sparkling = false;
            monoxiderCarry = 0;
            temperature = 0;
            temperatureRegen = TEMPERATURE_REGEN_ON_HIT;
            notFrostburn = false;
            if (Main.myPlayer == player.whoAmI)
            {
                oldPosLength = 0;
                oldPosVisual = null;
            }
        }

        public override bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (chloroTransfer && type == ProjectileID.Bullet && Main.rand.NextBool(8))
                type = ProjectileID.ChlorophyteBullet;
            return true;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (healBeforeDeath && player.potionDelay <= 0)
            {
                if (player.statLife < 0)
                {
                    player.statLife = 1;
                }
                player.QuickHeal();
                return false;
            }
            if (omori)
            {
                if (omoriDeathTimer <= 0)
                {
                    Main.PlaySound(SoundID.Item60, player.position);
                    player.statLife = 1;
                    player.immune = true;
                    player.immuneTime = 120;
                    omoriDeathTimer = 18000;
                    return false;
                }
            }
            return true;
        }

        public override void PostUpdateBuffs()
        {
            monoxiderCarry = 0;
            var monoxider = ModContent.ProjectileType<Projectiles.Summon.Monoxider>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == monoxider && p.ai[0] > 0f)
                    monoxiderCarry++;
            }
        }

        public override void UpdateVanityAccessories()
        {
            for (int i = 0; i < MAX_ARMOR; i++)
            {
                if (player.armor[i].type <= Main.maxItemTypes)
                    continue;
                bool hidden = i < 10 && player.hideVisual[i];
                if (player.armor[i].modItem is IUpdateEquipVisuals update && !hidden)
                    update.UpdateEquipVisuals(player, this, i);
            }
            if (player.GetModPlayer<AQPlayer>().monoxiderBird)
                headAcc = PlayerHeadOverlayID.MonoxideHat;
        }

        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < Chest.maxItems; i++)
                {
                    if (player.bank.item[i].type > Main.maxItemTypes && player.bank.item[i].modItem is IUpdatePiggybank update)
                        update.UpdatePiggyBank(player, i);
                    if (player.bank2.item[i].type > Main.maxItemTypes && player.bank2.item[i].modItem is IUpdatePlayerSafe update2)
                        update2.UpdatePlayerSafe(player, i);
                }
            }
            UpdateCelesteTorus();
            if (player.wingsLogic > 0)
                player.wingTimeMax += extraFlightTime;
        }

        public override void PostUpdateEquips()
        {
            if (dartHead)
            {
                if (player.velocity.Y == 0f)
                    dartTrapHatTimer--;
                if (dartTrapHatTimer <= 0)
                {
                    dartTrapHatTimer = dartHeadDelay;
                    int damage = player.GetWeaponDamage(player.armor[0]);
                    var spawnPosition = player.gravDir == -1
                        ? player.position + new Vector2(player.width / 2f + 8f * player.direction, player.height)
                        : player.position + new Vector2(player.width / 2f + 8f * player.direction, 0f);
                    int p = Projectile.NewProjectile(spawnPosition, new Vector2(10f, 0f) * player.direction, dartHeadType, damage, player.armor[0].knockBack * player.minionKB, player.whoAmI);
                    Main.projectile[p].hostile = false;
                    Main.projectile[p].friendly = true;
                }
            }
            if (arachnotron)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    int type = ModContent.ProjectileType<ArachnotronLegs>();
                    if (player.ownedProjectileCounts[type] <= 0)
                    {
                        int p = Projectile.NewProjectile(player.Center, Vector2.Zero, type, 33, 1f, player.whoAmI);
                        Main.projectile[p].netUpdate = true;
                    }
                }
            }
            if (omori)
            {
                if (omoriDeathTimer > 0)
                {
                    omoriDeathTimer--;
                    if (omoriDeathTimer == 0 && Main.myPlayer == player.whoAmI)
                        Main.PlaySound(SoundID.MaxMana, (int)player.position.X, (int)player.position.Y, 1, 0.85f, -6f);
                }
                int type = ModContent.ProjectileType<Friend>();
                if (player.ownedProjectileCounts[type] < 3)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == type && Main.projectile[i].owner == player.whoAmI)
                            Main.projectile[i].Kill();
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(player.Center, Vector2.Zero, type, 66, 4f, player.whoAmI, 1f + i);
                    }
                }
            }
            else
            {
                if (omoriDeathTimer <= 0)
                    omoriDeathTimer = 1;
            }
            if (spicyEel)
            {
                player.accRunSpeed *= 1.1f;
                player.moveSpeed *= 1.1f;
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            switch (proj.type)
            {
                case ProjectileID.SiltBall:
                case ProjectileID.SlushBall:
                    {
                        if (extractinator)
                            damage /= 4;
                    }
                    break;
            }
            var aQProjectile = proj.GetGlobalProjectile<AQProjectile>();
            if (aQProjectile.temperature != 0)
            {
                InflictTemperature(aQProjectile.temperature);
            }
        }

        public static bool CanBossChannel(NPC npc)
        {
            if (npc.chaseable || npc.dontTakeDamage)
            {
                return false;
            }
            return npc.boss || AQNPC.Sets.BossRelatedEnemy[npc.type];
        }

        public void DoHyperCrystalChannel(NPC target, int damage, float knockback, Vector2 center, Vector2 targCenter)
        {
            if (target.SpawnedFromStatue || target.type == NPCID.TargetDummy || CanBossChannel(target))
                return;
            int boss = -1;
            float closestDist = 1200f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && CanBossChannel(npc))
                {
                    float dist = (npc.Center - center).Length();
                    if (dist < closestDist)
                    {
                        boss = i;
                        closestDist = dist;
                    }
                }
            }
            if (boss != -1)
            {
                int dmg = damage > target.lifeMax ? target.lifeMax : damage;
                var normal = Vector2.Normalize(Main.npc[boss].Center - targCenter);
                int size = 4;
                var type = ModContent.DustType<MonoDust>();
                Vector2 position = target.Center - new Vector2(size / 2);
                int length = (int)(Main.npc[boss].Center - targCenter).Length();
                if (Main.myPlayer == player.whoAmI && AQConfigClient.c_TonsofScreenShakes)
                {
                    if (length < 800)
                        ScreenShakeManager.AddShake(new BasicScreenShake(12, AQGraphics.MultIntensity((800 - length) / 128)));
                }
                int dustLength = length / size;
                const float offset = MathHelper.TwoPi / 3f;
                for (int i = 0; i < dustLength; i++)
                {
                    Vector2 pos = position + normal * (i * size);
                    for (int j = 0; j < 6; j++)
                    {
                        int d = Dust.NewDust(pos, size, size, type);
                        float positionLength = Main.dust[d].position.Length() / 32f;
                        Main.dust[d].color = new Color(
                            (float)Math.Sin(positionLength) + 1f,
                            (float)Math.Sin(positionLength + offset) + 1f,
                            (float)Math.Sin(positionLength + offset * 2f) + 1f,
                            0.5f);
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    Vector2 normal2 = new Vector2(1f, 0f).RotatedBy(MathHelper.PiOver4 * i);
                    for (int j = 0; j < 4; j++)
                    {

                        float positionLength1 = (targCenter + normal2 * (j * 8f)).Length() / 32f;
                        var color = new Color(
                            (float)Math.Sin(positionLength1) + 1f,
                            (float)Math.Sin(positionLength1 + offset) + 1f,
                            (float)Math.Sin(positionLength1 + offset * 2f) + 1f,
                            0.5f);
                        int d = Dust.NewDust(targCenter, 1, 1, type, default, default, default, color);
                        Main.dust[d].velocity = normal2 * (j * 3.5f);
                    }
                }
                Projectile.NewProjectile(Main.npc[boss].Center, Vector2.Zero, ModContent.ProjectileType<HyperCrystalExplosion>(), dmg * 2, knockback * 2, player.whoAmI);
            }
        }


        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            switch (npc.type)
            {
                case NPCID.Mothron:
                case NPCID.MothronSpawn:
                case NPCID.MothronEgg:
                case NPCID.CultistBoss:
                case NPCID.CultistBossClone:
                case NPCID.CultistDragonBody1:
                case NPCID.CultistDragonBody2:
                case NPCID.CultistDragonBody3:
                case NPCID.CultistDragonBody4:
                case NPCID.CultistDragonHead:
                case NPCID.CultistDragonTail:
                case NPCID.AncientCultistSquidhead:
                    {
                        if (mothmanMask)
                            damage /= 2;
                    }
                    break;
            }
            var aQNPC = npc.GetGlobalNPC<AQNPC>();
            if (aQNPC.temperature != 0)
            {
                InflictTemperature(aQNPC.temperature);
            }
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            var center = player.Center;
            var targetCenter = target.Center;
            if (item.melee)
            {
                if (hyperCrystal)
                {
                    target.AddBuff(ModContent.BuffType<Sparkling>(), 120);
                    if (crit)
                        DoHyperCrystalChannel(target, damage, knockback, center, targetCenter);
                }
                if (primeTime)
                {
                    if (player.potionDelay <= 0)
                    {
                        player.AddBuff(ModContent.BuffType<Buffs.PrimeTime>(), 600);
                        player.AddBuff(BuffID.PotionSickness, player.potionDelayTime);
                    }
                }
            }
            HitNPCEffects(target, targetCenter, damage, knockback, crit);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            var center = player.Center;
            var targetCenter = target.Center;
            if (proj.melee && proj.whoAmI == player.heldProj && proj.aiStyle != 99)
            {
                if (hyperCrystal)
                {
                    target.AddBuff(ModContent.BuffType<Sparkling>(), 120);
                    if (crit)
                        DoHyperCrystalChannel(target, damage, knockback, center, targetCenter);
                }
                if (primeTime)
                {
                    if (player.potionDelay <= 0)
                    {
                        player.AddBuff(ModContent.BuffType<Buffs.PrimeTime>(), 600);
                        player.AddBuff(BuffID.PotionSickness, player.potionDelayTime);
                    }
                }
            }
            HitNPCEffects(target, targetCenter, damage, knockback, crit);
        }

        private void HitNPCEffects(NPC target, Vector2 targetCenter, int damage, float knockback, bool crit)
        {
            if (mothmanMask && mothmanExplosionDelay == 0 && player.statLife >= player.statLifeMax2 && crit && !target.buffImmune[ModContent.BuffType<BlueFire>()] && target.type != NPCID.TargetDummy)
            {
                target.AddBuff(ModContent.BuffType<BlueFire>(), 480);
                if (Main.myPlayer == player.whoAmI)
                {
                    Main.PlaySound(SoundID.Item74, targetCenter);
                    int amount = (int)(25 * AQConfigClient.c_EffectIntensity);
                    if (AQConfigClient.c_EffectQuality < 1f)
                    {
                        amount = (int)(amount * AQConfigClient.c_EffectQuality);
                    }
                    var pos = target.position - new Vector2(2f, 2f);
                    var rect = new Rectangle((int)pos.X, (int)pos.Y, target.width + 4, target.height + 4);
                    for (int i = 0; i < amount; i++)
                    {
                        var dustPos = new Vector2(Main.rand.Next(rect.X, rect.X + rect.Width), Main.rand.Next(rect.Y, rect.Y + rect.Height));
                        var velocity = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-10f, 2f).Abs());
                        ParticleLayers.AddParticle_PostDrawPlayers(
                            new EmberParticle(dustPos, velocity,
                            new Color(0.5f, Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.8f, 1f), 0f), Main.rand.NextFloat(0.8f, 1.1f)));
                        ParticleLayers.AddParticle_PostDrawPlayers(
                            new EmberParticle(dustPos, velocity,
                            new Color(0.5f, Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.8f, 1f), 0f) * 0.2f, 1.5f));
                    }
                    amount = (int)(120 * AQConfigClient.c_EffectIntensity);
                    if (AQConfigClient.c_EffectQuality < 1f)
                    {
                        amount = (int)(amount * AQConfigClient.c_EffectQuality);
                    }
                    if (AQConfigClient.c_Screenshakes)
                    {
                        ScreenShakeManager.AddShake(new BasicScreenShake(16, 8));
                    }
                    mothmanExplosionDelay = 60;
                    int p = Projectile.NewProjectile(targetCenter, Vector2.Normalize(targetCenter - player.Center), ModContent.ProjectileType<MothmanCritExplosion>(), damage * 2, knockback * 1.5f, player.whoAmI, 0f, target.whoAmI);
                    var size = Main.projectile[p].Size;
                    float radius = size.Length() / 5f;
                    for (int i = 0; i < amount; i++)
                    {
                        var offset = new Vector2(Main.rand.NextFloat(radius), 0f).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
                        var normal = Vector2.Normalize(offset);
                        var dustPos = targetCenter + offset;
                        var velocity = normal * Main.rand.NextFloat(6f, 12f);
                        ParticleLayers.AddParticle_PostDrawPlayers(
                            new EmberParticle(dustPos, velocity,
                            new Color(0.5f, Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.8f, 1f), 0f), Main.rand.NextFloat(0.8f, 1.1f)));
                        ParticleLayers.AddParticle_PostDrawPlayers(
                            new EmberParticle(dustPos, velocity,
                            new Color(0.5f, Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.8f, 1f), 0f) * 0.2f, 1.5f));
                        if (Main.rand.NextBool(14))
                        {
                            var sparkleClr = new Color(0.5f, Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.8f, 1f), 0f);
                            ParticleLayers.AddParticle_PostDrawPlayers(
                                new SparkleParticle(dustPos, velocity,
                                sparkleClr, 1.5f));
                            ParticleLayers.AddParticle_PostDrawPlayers(
                                new SparkleParticle(dustPos, velocity,
                                sparkleClr * 0.5f, 1f)
                                { rotation = MathHelper.PiOver4 });
                        }
                    }
                }
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (sparkling)
            {
                if (player.lifeRegen > 0)
                    player.lifeRegen = 0;
                player.lifeRegenTime = 0;
                player.lifeRegen -= 40;
            }
            if (notFrostburn)
            {
                if (player.lifeRegen > 0)
                    player.lifeRegen = 0;
                player.lifeRegenTime = 0;
                player.lifeRegen -= 10;
            }
        }

        public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
        {
            if (Main.myPlayer == player.whoAmI && AQConfigClient.Instance.ShowCompletedQuestsCount)
                CombatText.NewText(player.getRect(), Color.Aqua, player.anglerQuestsFinished);
            var item = new Item();
            if (player.anglerQuestsFinished == 2)
            {
                item.SetDefaults(ModContent.ItemType<CopperSeal>());
                rewardItems.Add(item.Clone());
                item = new Item();
            }
            else if (player.anglerQuestsFinished == 10)
            {
                item.SetDefaults(ModContent.ItemType<SilverSeal>());
                rewardItems.Add(item.Clone());
                item = new Item();
            }
            else if (player.anglerQuestsFinished == 20)
            {
                item.SetDefaults(ModContent.ItemType<GoldSeal>());
                rewardItems.Add(item.Clone());
                item = new Item();
            }
        }

        public override void CatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
        {
            //if (liquidType == Tile.Liquid_Water)
            //{
            //    //if (GlimmerEvent.IsActive)
            //    //{
            //    //    if (player.position.Y < Main.worldSurface * 16f)
            //    //    {
            //    //        if (player.ZoneCorrupt && Main.rand.NextBool(5))
            //    //        {
            //    //            caughtType = ModContent.ItemType<Fizzler>();
            //    //        }
            //    //        else if (((int)(player.position.X / 16f + player.width / 2) - GlimmerEvent.tileX).Abs() < GlimmerEvent.UltraStariteDistance && Main.rand.NextBool(7))
            //    //        {
            //    //            caughtType = ModContent.ItemType<UltraEel>();
            //    //        }
            //    //        else if (Main.rand.NextBool(6))
            //    //        {
            //    //            caughtType = ModContent.ItemType<Nessie>();
            //    //        }
            //    //        else if (Main.rand.NextBool(8))
            //    //        {
            //    //            caughtType = ModContent.ItemType<Blobfish>();
            //    //        }
            //    //        else if (Main.rand.NextBool(6))
            //    //        {
            //    //            caughtType = ModContent.ItemType<GlimmeringStatue>();
            //    //        }
            //    //        else if (Main.rand.NextBool(6))
            //    //        {
            //    //            caughtType = ModContent.ItemType<MoonlightWall>();
            //    //        }
            //    //        else
            //    //        {
            //    //            if (caughtType == ItemID.Bass || caughtType == ItemID.NeonTetra || caughtType == ItemID.Salmon)
            //    //                caughtType = ModContent.ItemType<Molite>();
            //    //        }
            //    //    }
            //    //}
            //}
            if (questFish > Main.maxItems && ItemLoader.GetItem(questFish) is AnglerQuestItem anglerQuestItem)
            {
                if (anglerQuestItem.FishingLocation.CatchFish(player, fishingRod, bait, power, liquidType, poolSize, worldLayer))
                {
                    caughtType = questFish;
                    junk = false;
                    return;
                }
            }
            int fish = FishLoader.PoolFish(player, this, fishingRod, bait, power, liquidType, poolSize, worldLayer, questFish, caughtType);
            if (fish != 0)
            {
                caughtType = fish;
                junk = false;
                return;
            }
            //if (liquidType == Tile.Liquid_Honey && NPC.downedQueenBee)
            //{
            //    if (Main.rand.NextBool(3))
            //    {
            //        caughtType = ModContent.ItemType<Combfish>();
            //    }
            //    else if (Main.rand.NextBool(5))
            //    {
            //        caughtType = ModContent.ItemType<LarvaEel>();
            //    }
            //}
        }

        private Vector2 getCataDustSpawnPos(int gravityOffset, int headFrame)
        {
            var spawnPos = new Vector2((int)(player.position.X + player.width / 2) - 3f, (int)(player.position.Y + 12f + gravityOffset) + Main.OffsetsPlayerHeadgear[headFrame].Y) + player.headPosition;
            if (player.direction == -1)
                spawnPos.X -= 4f;
            spawnPos.X -= 0.6f;
            spawnPos.Y -= 0.6f;
            return spawnPos;
        }

        private void CataEyeDust(Vector2 spawnPos)
        {
            int d = Dust.NewDust(spawnPos + new Vector2(0f, -6f), 6, 6, ModContent.DustType<MonoDust>(), 0, 0, 0, cataEyeColor);
            if (Main.rand.NextBool(600))
            {
                Main.dust[d].velocity = player.velocity.RotatedBy(Main.rand.NextFloat(-0.025f, 0.025f)) * 1.5f;
                Main.dust[d].velocity.X += Main.windSpeed * 20f + player.velocity.X / -2f;
                Main.dust[d].velocity.Y -= Main.rand.NextFloat(8f, 16f);
                Main.dust[d].scale *= Main.rand.NextFloat(0.65f, 2f);
            }
            else
            {
                Main.dust[d].velocity = player.velocity * 1.1f;
                Main.dust[d].velocity.X += Main.windSpeed * 2.5f + player.velocity.X / -2f;
                Main.dust[d].velocity.Y -= Main.rand.NextFloat(4f, 5.65f);
                Main.dust[d].scale *= Main.rand.NextFloat(0.95f, 1.4f);
            }

            Main.dust[d].shader = GameShaders.Armor.GetSecondaryShader(cMask, player);
            Main.playerDrawDust.Add(d);
        }

        public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (Main.myPlayer == drawInfo.drawPlayer.whoAmI)
            {
                oldPosLength = 0;
                arachnotronHeadTrail = false;
                arachnotronBodyTrail = false;
            }
            if (drawInfo.shadow == 0f)
            {
                if (sparkling)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int d = Dust.NewDust(drawInfo.position - new Vector2(2f, 2f), player.width + 4, player.height + 4, ModContent.DustType<UltimaDust>(), player.velocity.X * 0.4f, player.velocity.Y * 0.4f, 100, default(Color), Main.rand.NextFloat(0.45f, 1f));
                        Main.dust[d].velocity *= 2.65f;
                        Main.dust[d].velocity.Y -= 2f;
                        Main.playerDrawDust.Add(d);
                    }
                    Lighting.AddLight(player.Center, 1f, 1f, 1f);
                    fullBright = true;
                }
                if (notFrostburn)
                {
                    if (Main.netMode != NetmodeID.Server && AQGraphics.GameWorldActive)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var pos = drawInfo.position - new Vector2(2f, 2f);
                            var rect = new Rectangle((int)pos.X, (int)pos.Y, player.width + 4, player.height + 4);
                            var dustPos = new Vector2(Main.rand.Next(rect.X, rect.X + rect.Width), Main.rand.Next(rect.Y, rect.Y + rect.Height));
                            ParticleLayers.AddParticle_PostDrawPlayers(
                                new EmberParticle(dustPos, new Vector2((player.velocity.X + Main.rand.NextFloat(-3f, 3f)) * 0.3f, ((player.velocity.Y + Main.rand.NextFloat(-3f, 3f)) * 0.4f).Abs() - 2f),
                                new Color(0.5f, Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.8f, 1f), 0f), Main.rand.NextFloat(0.2f, 1.2f)));
                        }
                    }
                    Lighting.AddLight(player.Center, 0.4f, 0.4f, 1f);
                    fullBright = true;
                }
                for (int i = 0; i < DYE_WRAP; i++)
                {
                    if (player.armor[i].type > Main.maxItemTypes && !player.hideVisual[i] && player.armor[i].modItem is IUpdateEquipVisuals updateVanity)
                        updateVanity.UpdateEquipVisuals(player, this, i);
                }
                for (int i = DYE_WRAP; i < MAX_ARMOR; i++)
                {
                    if (player.armor[i].type > Main.maxItemTypes && player.armor[i].modItem is IUpdateEquipVisuals updateVanity)
                        updateVanity.UpdateEquipVisuals(player, this, i);
                }
                int gravityOffset = 0;
                int headFrame = player.bodyFrame.Y / FRAME_HEIGHT;
                if (player.gravDir == -1)
                    gravityOffset = 8;
                switch ((PlayerMaskID)mask)
                {
                    case PlayerMaskID.CataMask:
                        {
                            if (cMask > 0)
                                cataEyeColor = new Color(100, 100, 100, 0);
                            if (!player.mount.Active && !player.merman && !player.wereWolf && player.statLife == player.statLifeMax2)
                            {
                                float dustAmount = (Main.rand.Next(2, 3) + 1) * ModContent.GetInstance<AQConfigClient>().EffectQuality;
                                if (dustAmount < 1f)
                                {
                                    if (Main.rand.NextFloat(dustAmount) > 0.1f)
                                        CataEyeDust(getCataDustSpawnPos(gravityOffset, headFrame));
                                }
                                else
                                {
                                    var spawnPos = getCataDustSpawnPos(gravityOffset, headFrame);
                                    for (int i = 0; i < dustAmount; i++)
                                    {
                                        CataEyeDust(spawnPos);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            var aQPlayer = drawInfo.drawPlayer.GetModPlayer<AQPlayer>();
            var drawPlayer = drawInfo.drawPlayer.GetModPlayer<AQPlayer>();
            if (aQPlayer.blueSpheres)
            {
                celesteTorusOffsetsForDrawing = new Vector3[MaxCelesteTorusOrbs];
                for (int i = 0; i < MaxCelesteTorusOrbs; i++)
                {
                    celesteTorusOffsetsForDrawing[i] = aQPlayer.GetCelesteTorusPositionOffset(i);
                }
            }
            if (!aQPlayer.chomper && aQPlayer.monoxiderBird)
                aQPlayer.headAcc = (byte)PlayerHeadOverlayID.MonoxideHat;
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            int i = layers.FindIndex((p) => p.mod.Equals("Terraria") && p.Name.Equals("Head"));
            if (i != -1)
            {
                Layers.PostDrawHead.visible = true;
                layers.Insert(i + 1, Layers.PostDrawHead);
            }
            i = layers.FindIndex((p) => p.mod.Equals("Terraria") && p.Name.Equals("Body"));
            if (i != -1)
            {
                Layers.PostDrawBody.visible = true;
                layers.Insert(i + 1, Layers.PostDrawBody);
            }
            i = layers.FindIndex((p) => p.mod.Equals("Terraria") && p.Name.Equals("HeldItem"));
            if (i != -1)
            {
                Layers.PostDrawHeldItem.visible = true;
                layers.Insert(i + 1, Layers.PostDrawHeldItem);
            }
            Layers.PreDraw.visible = true;
            layers.Insert(0, Layers.PreDraw);
            Layers.PostDraw.visible = true;
            layers.Add(Layers.PostDraw);
        }

        public override void ModifyDrawHeadLayers(List<PlayerHeadLayer> layers)
        {
            Layers.PostDrawHead_Head.visible = true;
            layers.Add(Layers.PostDrawHead_Head);
        }

        public override void ModifyScreenPosition()
        {
            ScreenShakeManager.ModifyScreenPosition();
        }

        public void InflictTemperature(sbyte newTemperature)
        {
            temperatureRegen = TEMPERATURE_REGEN_ON_HIT;
            if (player.resistCold && newTemperature < 0)
            {
                newTemperature /= 2;
            }
            if (temperature < 0)
            {
                if (newTemperature < 0)
                {
                    if (temperature > newTemperature)
                    {
                        temperature = newTemperature;
                    }
                }
                else
                {
                    temperature /= 2;
                }
            }
            else if (temperature > 0)
            {
                if (newTemperature > 0)
                {
                    if (temperature < newTemperature)
                    {
                        temperature = newTemperature;
                    }
                }
                else
                {
                    temperature /= 2;
                }
            }
            else
            {
                temperature = (sbyte)(newTemperature / 2);
            }
            if (Main.expertMode)
            {
                if (newTemperature < 0)
                {
                    temperature -= 3;
                }
                else
                {
                    temperature += 3;
                }
            }
            else
            {
                if (newTemperature < 0)
                {
                    temperature -= 1;
                }
                else
                {
                    temperature += 1;
                }
            }
        }

        public Vector3 GetCelesteTorusPositionOffset(int i)
        {
            return Vector3.Transform(new Vector3(celesteTorusRadius, 0f, 0f), Matrix.CreateFromYawPitchRoll(celesteTorusX, celesteTorusY, celesteTorusZ + MathHelper.TwoPi / 5 * i));
        }

        public void UpdateCelesteTorus()
        {
            if (blueSpheres)
            {
                float playerPercent = player.statLife / (float)player.statLifeMax2;
                celesteTorusMaxRadius = GetCelesteTorusMaxRadius(playerPercent);
                celesteTorusRadius = MathHelper.Lerp(celesteTorusRadius, celesteTorusMaxRadius, 0.1f);
                celesteTorusDamage = GetCelesteTorusDamage();
                celesteTorusKnockback = GetCelesteTorusKnockback();

                celesteTorusScale = 1f + celesteTorusRadius * 0.006f + celesteTorusDamage * 0.009f + celesteTorusKnockback * 0.0015f;

                var type = ModContent.ProjectileType<CelesteTorusCollider>();
                if (Main.myPlayer == player.whoAmI && player.ownedProjectileCounts[type] <= 0)
                {
                    Projectile.NewProjectile(player.Center, Vector2.Zero, type, celesteTorusDamage, celesteTorusKnockback, player.whoAmI);
                }
                else
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<CelesteTorusCollider>())
                        {
                            Main.projectile[i].damage = celesteTorusDamage;
                            Main.projectile[i].knockBack = celesteTorusKnockback;
                            break;
                        }
                    }
                }
                var center = player.Center;
                bool danger = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].IsntFriendly() && Vector2.Distance(Main.npc[i].Center, center) < 2000f)
                    {
                        danger = true;
                        break;
                    }
                }

                if (danger)
                {
                    celesteTorusSpeed = 0.04f + (1f - playerPercent) * 0.0314f;
                    celesteTorusX = celesteTorusX.AngleLerp(0f, 0.01f);
                    celesteTorusY = celesteTorusY.AngleLerp(0f, 0.0075f);
                    celesteTorusZ += celesteTorusSpeed;
                }
                else
                {
                    celesteTorusSpeed = 0.0314f;
                    celesteTorusX += 0.0157f;
                    celesteTorusY += 0.01f;
                    celesteTorusZ += celesteTorusSpeed;
                }
            }
            else
            {
                celesteTorusDamage = 0;
                celesteTorusKnockback = 0f;
                celesteTorusMaxRadius = 0;
                celesteTorusRadius = 0f;
                celesteTorusScale = 1f;
                celesteTorusSpeed = 0f;
                celesteTorusX = 0f;
                celesteTorusY = 0f;
                celesteTorusZ = 0f;
            }
        }

        public int GetCelesteTorusMaxRadius(float playerPercent)
        {
            return (int)((float)Math.Sqrt(player.width * player.height) + 20f + player.wingTimeMax * 0.15f + player.wingTime * 0.15f + (1f - playerPercent) * 90f + player.statDefense);
        }

        public int GetCelesteTorusDamage()
        {
            return 25 + (int)(player.statDefense / 1.5f + player.endurance * 80f);
        }

        public float GetCelesteTorusKnockback()
        {
            return 6.5f + player.velocity.Length() * 0.8f;
        }

        public int GetOldPosCountMaxed(int maxCount)
        {
            int count = 0;
            for (; count < maxCount; count++)
            {
                if (oldPosVisual[count] == default(Vector2))
                    break;
            }
            return count;
        }

        public static bool ShouldDrawOldPos(Player player)
        {
            if (player.mount.Active || player.frozen || player.stoned || player.GetModPlayer<AQPlayer>().mask >= 0)
                return false;
            return true;
        }

        public void SetCursorDye(int type)
        {
            if (type <= CursorDyeManager.ID.None || type > CursorDyeManager.Instance.Count)
            {
                CursorDyeID = CursorDyeManager.ID.None;
                CursorDye = "";
            }
            else
            {
                CursorDyeID = type;
                var cursorDye = CursorDyeManager.Instance.GetContent(type);
                CursorDye = AQStringCodes.EncodeName(cursorDye.Mod, cursorDye.Name);
            }
        }

        public void SetMinionCarryPos(int x, int y)
        {
            hasMinionCarry = true;
            headMinionCarryX = x;
            headMinionCarryY = y;
        }

        public Vector2 GetHeadCarryPosition()
        {
            int x;
            if (headMinionCarryX != 0)
            {
                x = headMinionCarryX;
            }
            else if (headMinionCarryXOld != 0)
            {
                x = headMinionCarryXOld;
            }
            else
            {
                x = (int)player.position.X + player.width / 2;
            }
            int y;
            if (headMinionCarryY != 0)
            {
                y = headMinionCarryY;
            }
            else if (headMinionCarryYOld != 0)
            {
                y = headMinionCarryYOld;
            }
            else
            {
                y = (int)player.position.Y + player.height / 2;
            }
            return new Vector2(x, y);
        }

        /// <summary>
        /// Item is the item, int is the index.
        /// </summary>
        /// <param name="action"></param>
        public void ArmorAction(Action<Item, int> action)
        {
            for (int i = 0; i < 3; i++)
            {
                action(player.armor[i], i);
            }
        }

        /// <summary>
        /// Item is the item, bool is the hide flag, int is the index.
        /// </summary>
        /// <param name="action"></param>
        public void AccessoryAction(Action<Item, bool, int> action)
        {
            for (int i = 3; i < 8 + player.extraAccessorySlots; i++)
            {
                action(player.armor[i], player.hideVisual[i], i);
            }
        }

        public static void HeadMinionSummonCheck(int player, int type)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type != type && AQProjectile.Sets.MinionHeadType[Main.projectile[i].type] && Main.projectile[i].owner == player)
                    Main.projectile[i].Kill();
            }
        }

        public static bool HasFoodBuff(int player)
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Main.player[player].buffTime[i] > 0 && AQBuff.Sets.IsFoodBuff[Main.player[player].buffType[i]])
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PlayerCrit(int critChance, UnifiedRandom rand)
        {
            if (critChance >= 100)
                return true;
            if (critChance <= 0)
                return false;
            return rand.NextBool(100 - critChance);
        }

        public static bool CloseMoneyTrough()
        {
            if (_moneyTroughHack != null)
            {
                _moneyTroughHack.OnClose();
                Main.LocalPlayer.chest = -1;
                Recipe.FindRecipes();
                return true;
            }
            return false;
        }

        public static bool OpenMoneyTrough(ISuperClunkyMoneyTroughTypeThing moneyTrough, int index)
        {
            if (_moneyTroughHack == null)
            {
                _moneyTroughHack = moneyTrough;
                _moneyTroughHackIndex = index;
                var plr = Main.LocalPlayer;
                plr.chest = moneyTrough.ChestType;
                plr.chestX = (int)(Main.projectile[index].Center.X / 16f);
                plr.chestY = (int)(Main.projectile[index].Center.Y / 16f);
                plr.talkNPC = -1;
                Main.npcShop = 0;
                Main.playerInventory = true;
                moneyTrough.OnOpen();
                Recipe.FindRecipes();
                return true;
            }
            return false;
        }

        public static bool InVanitySlot(Player player, int type)
        {
            for (int i = DYE_WRAP; i < MAX_ARMOR; i++)
            {
                if (player.armor[i].type == type)
                    return true;
            }
            return false;
        }

        public static bool CanReach(Player player)
            => CanReach(player, Player.tileTargetX, Player.tileTargetY);
        public static bool CanReach(Player player, int x, int y)
            => !player.noBuilding && player.position.X / 16f - Player.tileRangeX - player.blockRange <= x
            && (player.position.X + player.width) / 16f + Player.tileRangeX - 1f + player.blockRange >= x
            && player.position.Y / 16f - Player.tileRangeY - player.blockRange <= y
            && (player.position.Y + player.height) / 16f + Player.tileRangeY + 2f + player.blockRange >= y;
        public static bool CanReach(Player player, Item item)
            => CanReach(player, item, Player.tileTargetX, Player.tileTargetY);
        public static bool CanReach(Player player, Item item, int x, int y)
            => player.position.X / 16f - Player.tileRangeX - item.tileBoost <= x
            && (player.position.X + player.width) / 16f + Player.tileRangeX + item.tileBoost - 1f >= x
            && player.position.Y / 16f - Player.tileRangeY - item.tileBoost <= y
            && (player.position.Y + player.height) / 16f + Player.tileRangeY + item.tileBoost - 2f >= y;

        public static bool ConsumeItem_CheckMouseToo(Player player, int type)
        {
            var item = player.ItemInHand();
            if (item != null && item.type == type)
            {
                if (ItemLoader.ConsumeItem(item, player))
                {
                    item.stack--;
                    if (item.stack <= 0)
                    {
                        item.TurnToAir();
                    }
                    return true;
                }
            }
            return player.ConsumeItem(type);
        }

        public static bool CanForceAutoswing(Player player, Item item, bool ignoreChanneled = false)
        {
            if (!item.autoReuse && item.useTime != 0 && item.useTime == item.useAnimation)
            {
                if (!ignoreChanneled && (item.channel || item.noUseGraphic))
                {
                    return player.ownedProjectileCounts[item.shoot] < item.stack;
                }
                return player.altFunctionUse != 2;
            }
            return false;
        }

        public static bool IgnoreMoons()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && Main.player[i].GetModPlayer<AQPlayer>().ignoreMoons) // dead players also allow moons to be disabled
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ShouldDoFadingBecauseOfToolsOrSomething(Player player)
        {
            if (player.HeldItem == null || player.HeldItem.type <= ItemID.None)
            {
                return false;
            }
            var item = player.HeldItem;
            return item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile > 0 || item.createWall > 0 || item.tileWand > 0;
        }
    }
}