﻿using AQMod.Dusts.GaleStreams;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Projectiles.Monster
{
    public class RedSpriteCloud : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 16;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;

            projectile.GetGlobalProjectile<AQProjectile>().SetupTemperatureStats(20);
        }

        public override Color? GetAlpha(Color drawColor)
        {
            if (drawColor.R < 60)
            {
                drawColor.R = 60;
            }
            if (drawColor.G < 60)
            {
                drawColor.G = 60;
            }
            if (drawColor.B < 60)
            {
                drawColor.B = 60;
            }
            return drawColor;
        }

        public override void AI()
        {
            if ((int)projectile.ai[0] == 1)
            {
                projectile.rotation = 0f;
                projectile.velocity *= 0.2f;
                if (projectile.frame <= 3)
                {
                    projectile.frame = 4;
                }
                projectile.frameCounter++;
                if (projectile.frameCounter > 4)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                    if (projectile.frame >= Main.projFrames[projectile.type])
                    {
                        projectile.frame = 4;
                    }
                }
                if (projectile.timeLeft < 60)
                {
                    if (projectile.timeLeft < 20)
                    {
                        projectile.scale -= projectile.scale * 0.1f;
                    }
                    else
                    {
                        projectile.scale += projectile.scale * 0.02f;
                    }
                }
                projectile.ai[1]++;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (projectile.ai[1] > 6f)
                    {
                        projectile.ai[1] = 0f;
                        Projectile.NewProjectile(new Vector2(projectile.position.X + Main.rand.NextFloat(projectile.width), projectile.position.Y + projectile.height), new Vector2(0f, 10f), ModContent.ProjectileType<RedSpriteCloudLightning>(), projectile.damage, projectile.knockBack, Main.myPlayer);
                    }
                }
            }
            else
            {
                projectile.frameCounter++;
                if (projectile.frameCounter > 4)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                    if (projectile.frame > 3)
                    {
                        projectile.frame = 0;
                    }
                }
                if (projectile.timeLeft < 2)
                {
                    projectile.timeLeft = 600;
                    projectile.netUpdate = true;
                    projectile.ai[0] = 1f;
                }
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.timeLeft);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.timeLeft = reader.ReadInt32();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var texture = Main.projectileTexture[projectile.type];
            var orig = texture.Size() / 2f;
            var drawPosition = projectile.Center;
            var scale = new Vector2(projectile.scale, projectile.scale);
            float speedX = projectile.velocity.X.Abs();
            lightColor = projectile.GetAlpha(lightColor);
            if (speedX > 8f)
            {
                scale.X += (speedX - 8f) / 40f;
                drawPosition.X -= (scale.X - 1f) * 16f;
            }
            var frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            var origin = frame.Size() / 2f;
            float electric = 3f + (float)Math.Sin(Main.GlobalTime * 5f);
            if ((int)projectile.ai[0] == 1)
            {
                if (projectile.localAI[0] < 10)
                    projectile.localAI[0]++;
                electric += projectile.localAI[0] / 5f;
            }
            if (electric > 0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    Main.spriteBatch.Draw(texture, drawPosition - Main.screenPosition + new Vector2(electric, 0f).RotatedBy(MathHelper.PiOver4 * i + Main.GlobalTime * 5f), frame, new Color(150, 255, 0, 20), projectile.rotation, origin, scale, SpriteEffects.None, 0f);
                }
            }
            Main.spriteBatch.Draw(texture, drawPosition - Main.screenPosition, frame, lightColor, projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            var center = projectile.Center;
            for (int i = 0; i < 50; i++)
            {
                int d = Dust.NewDust(projectile.position, 16, 16, ModContent.DustType<RedSpriteDust>());
                Main.dust[d].velocity = (Main.dust[d].position - center) / 8f;
            }
        }
    }
}