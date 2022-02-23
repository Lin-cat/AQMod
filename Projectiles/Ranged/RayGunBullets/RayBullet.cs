﻿using AQMod.Assets;
using AQMod.Common.ID;
using AQMod.Dusts;
using AQMod.Effects.Trails;
using AQMod.Effects.Trails.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Projectiles.Ranged.RayGunBullets
{
    public class RayBullet : ModProjectile
    {
        public static int BulletProjectileIDToRayProjectileID(int type)
        {
            switch (type)
            {
                case ProjectileID.Bullet:
                    return ModContent.ProjectileType<RayBullet>();
                case ProjectileID.MeteorShot:
                    return ModContent.ProjectileType<RayMeteorBullet>();
                case ProjectileID.ChlorophyteBullet:
                    return ModContent.ProjectileType<RayChlorophyteBullet>();
                case ProjectileID.VenomBullet:
                    return ModContent.ProjectileType<RayVenomBullet>();
                case ProjectileID.CursedBullet:
                    return ModContent.ProjectileType<RayCursedBullet>();
                case ProjectileID.IchorBullet:
                    return ModContent.ProjectileType<RayIchorBullet>();
            }
            return type;
        }

        protected Func<float, Vector2> GetSizeMethod() => (p) => new Vector2(8f - p * 8f);
        protected Func<float, Color> GetColorMethod(Color lightColor) => (p) => lightColor * (1f - p);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 200;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.aiStyle = -1;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.timeLeft = 600;
            projectile.extraUpdates = 6;
            projectile.hide = true;
        }

        public virtual Color GetColor() => new Color(60, 255, 60, 5);

        public override void AI()
        {
            int targetIndex = -1;
            float distance = 50f;
            var center = projectile.Center;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].CanBeChasedBy())
                {
                    float dist = (Main.npc[i].Center - center).Length() - (float)Math.Sqrt(Main.npc[i].width * Main.npc[i].width + Main.npc[i].height * Main.npc[i].height);
                    if (dist < distance)
                    {
                        targetIndex = i;
                        distance = dist;
                    }
                }
            }
            if (targetIndex != -1)
            {
                projectile.tileCollide = false;
                projectile.timeLeft = 60;
                projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.Normalize(Main.npc[targetIndex].Center - projectile.Center) * (Main.npc[targetIndex].velocity.Length() * 0.5f + 8f), 0.1f);
            }
            projectile.localAI[1]++;
            if (projectile.localAI[1] > 6f && projectile.hide)
                projectile.hide = false;
            if (!projectile.hide)
            {
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = GetColor();
            var spotlight = LegacyTextureCache.Lights[LightTex.Spotlight24x24];
            var center = projectile.Center;
            var orig = spotlight.Size() / 2f;
            var texture = TextureGrabber.GetProjectile(projectile.type);
            var textureOrig = new Vector2(texture.Width / 2f, 2f);
            var offset = new Vector2(projectile.width / 2f, projectile.height / 2f);
            if (PrimitivesRenderer.ShouldDrawVertexTrails(PrimitivesRenderer.GetVertexDrawingContext_Projectile(projectile)))
            {
                var renderingPositions = PrimitivesRenderer.GetValidRenderingPositions(projectile.oldPos, new Vector2(projectile.width / 2f - Main.screenPosition.X, projectile.height / 2f - Main.screenPosition.Y));
                if (renderingPositions.Count > 1)
                {
                    PrimitivesRenderer.FullDraw(LegacyTextureCache.Trails[TrailTex.ThickerLine], PrimitivesRenderer.TextureTrail,
                        renderingPositions.ToArray(), GetSizeMethod(), GetColorMethod(lightColor));
                }
            }
            else
            {
                int trailLength = ProjectileID.Sets.TrailCacheLength[projectile.type];
                for (int i = 0; i < trailLength; i++)
                {
                    if (projectile.oldPos[i] == new Vector2(0f, 0f))
                        break;
                    float progress = 1f - 1f / trailLength * i;
                    var trailClr = lightColor;
                    Main.spriteBatch.Draw(texture, projectile.oldPos[i] + offset - Main.screenPosition, null, trailClr * progress, projectile.rotation, textureOrig, projectile.scale, SpriteEffects.None, 0f);
                }
            }
            int targetIndex = AQNPC.FindTarget(center, 800f);
            Main.spriteBatch.Draw(texture, center - Main.screenPosition, null, lightColor, projectile.rotation, new Vector2(texture.Width / 2f, 2f), projectile.scale, SpriteEffects.None, 0f);
            if (targetIndex != -1)
            {
                var distance = (center - Main.npc[targetIndex].Center).Length();
                var intensity = (1f - distance / 800f) * ModContent.GetInstance<AQConfigClient>().EffectIntensity;
                var drawColor = lightColor * 15;
                var crossScale = new Vector2(projectile.scale * 0.4f * intensity, projectile.scale * 3f * intensity);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, Color.Lerp(lightColor, drawColor, MathHelper.Clamp(intensity, 0f, 1f)), 0f, orig, projectile.scale * 2f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, lightColor * 0.1f, 0f, orig, projectile.scale * intensity * 2.5f, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, drawColor * intensity, 0f, orig, crossScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, drawColor * intensity, MathHelper.PiOver2, orig, crossScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, lightColor * intensity * 0.1f, 0f, orig, crossScale * 2f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, lightColor * intensity * 0.1f, MathHelper.PiOver2, orig, crossScale * 2f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, lightColor * intensity * 0.4f, MathHelper.PiOver4, orig, crossScale * 0.6f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, lightColor * intensity * 0.4f, MathHelper.PiOver4 * 3f, orig, crossScale * 0.6f, SpriteEffects.None, 0f);
            }
            else
            {
                Main.spriteBatch.Draw(spotlight, center - Main.screenPosition, null, lightColor, 0f, orig, projectile.scale * 2f, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Main.rand.NextBool(4))
                target.AddBuff(ModContent.BuffType<Buffs.Debuffs.Sparkling>(), 600);
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                var renderingPositions = PrimitivesRenderer.GetValidRenderingPositions(projectile.oldPos, new Vector2(projectile.width / 2f, projectile.height / 2f));
                if (renderingPositions.Count > 3)
                {
                    renderingPositions.RemoveAt(renderingPositions.Count - 1);
                    Trail.PreDrawProjectiles.NewTrail(new DeathTrail(LegacyTextureCache.Trails[TrailTex.ThickerLine], PrimitivesRenderer.TextureTrail,
                    renderingPositions, GetSizeMethod(), GetColorMethod(GetColor()), default, default, projectile.extraUpdates));
                }
            }
            int dustType = ModContent.DustType<MonoDust>();
            var dustColor = GetColor();
            for (int i = 0; i < 10; i++)
            {
                Main.dust[Dust.NewDust(projectile.position - new Vector2(8f, 8f), 16, 16, dustType, 0f, 0f, 0, dustColor, 1.25f)].velocity *= 0.015f;
            }
        }
    }
}