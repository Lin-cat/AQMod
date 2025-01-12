﻿using AQMod.Common.Graphics;
using AQMod.Common.WorldGeneration;
using AQMod.Effects.Particles;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Projectiles
{
    public class FriendlyWind : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            projectile.width = 40;
            projectile.height = 40;
            projectile.friendly = true;
            projectile.aiStyle = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 240;
            projectile.alpha = 255;
            projectile.hide = true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            int minX = (int)projectile.position.X / 16;
            int minY = (int)projectile.position.Y / 16;
            int maxX = minX + Math.Min(projectile.width / 16, 1);
            int maxY = minY + Math.Min(projectile.height / 16, 1);
            int colldingTiles = 0;
            for (int i = minX; i < maxX; i++)
            {
                for (int j = minY; j < maxY; j++)
                {
                    if (Main.tile[i, j] == null)
                    {
                        Main.tile[i, j] = new Tile();
                        continue;
                    }
                    if (Main.tile[i, j].active() && AQWorldGen.ActiveAndFullySolid(Main.tile[i, j]))
                    {
                        colldingTiles++;
                    }
                }
            }
            if (colldingTiles > 8)
            {
                projectile.velocity *= 0.97f - (colldingTiles - 8) * 0.01f;
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var target = Main.npc[i];
                if (target.active)
                {
                    var aQNPC = target.GetGlobalNPC<AQNPC>();
                    if (aQNPC.ShouldApplyWindMechanics(target) && projectile.getRect().Intersects(target.getRect()))
                    {
                        aQNPC.ApplyWindMechanics(target, Vector2.Normalize(projectile.velocity) * projectile.knockBack);
                        target.netUpdate = true;
                    }
                }
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                var target = Main.projectile[i];
                if (i != projectile.whoAmI && target.active)
                {
                    if (projectile.Colliding(projectile.getRect(), target.getRect()))
                    {
                        var aQProjectile = target.GetGlobalProjectile<AQProjectile>();
                        if (aQProjectile.CanApplyWind(target))
                        {
                            aQProjectile.ApplyWindMechanics(target, Vector2.Normalize(projectile.velocity) * projectile.knockBack);
                            target.netUpdate = true;
                        }
                    }
                }
            }
            if (Main.netMode != NetmodeID.Server && AQGraphics.GameWorldActive && Main.rand.NextBool(3))
            {
                var rect = new Rectangle((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height);
                var dustPos = new Vector2(Main.rand.Next(rect.X, rect.X + rect.Width), Main.rand.Next(rect.Y, rect.Y + rect.Height));
                var velocity = new Vector2(-projectile.velocity.X + Main.rand.NextFloat(-1f, 1f) + Main.windSpeed, -projectile.velocity.Y + Main.rand.NextFloat(-1f, 1f));
                AQMod.Particles.PostDrawPlayers.AddParticle(
                    new MonoParticle(dustPos, velocity * 0.5f,
                    new Color(0.5f, 0.5f, 0.5f, 0f), Main.rand.NextFloat(0.6f, 1.2f)));
            }
            if (projectile.timeLeft < 80)
            {
                projectile.alpha += 3;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.width);
            writer.Write(projectile.height);
            writer.Write(projectile.extraUpdates);
            writer.Write(projectile.hide);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.width = reader.ReadInt32();
            projectile.height = reader.ReadInt32();
            projectile.extraUpdates = reader.ReadInt32();
            projectile.hide = reader.ReadBoolean();
        }

        public override void Kill(int timeLeft)
        {
            if (projectile.hide)
            {
                return;
            }
            AQGraphics.SetCullPadding(padding: 50);
            if (Main.netMode != NetmodeID.Server && AQGraphics.GameWorldActive && AQGraphics.Cull_WorldPosition(projectile.getRect()))
            {
                var trueOldPos = projectile.oldPos.AsAddAll(new Vector2(projectile.width / 2f, projectile.height / 2f));
                var rect = new Rectangle((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height);
                for (int i = 0; i < 5; i++)
                {
                    var dustPos = new Vector2(Main.rand.Next(rect.X, rect.X + rect.Width), Main.rand.Next(rect.Y, rect.Y + rect.Height));
                    var velocity = new Vector2(-projectile.velocity.X + Main.rand.NextFloat(-1f, 1f) + Main.windSpeed, -projectile.velocity.Y + Main.rand.NextFloat(-1f, 1f));
                    AQMod.Particles.PostDrawPlayers.AddParticle(
                        new MonoParticle(dustPos, velocity * 0.5f,
                        new Color(0.5f, 0.5f, 0.5f, 0f), Main.rand.NextFloat(0.8f, 1.45f)));
                }
            }
        }
    }
}