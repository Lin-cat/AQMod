﻿using AQMod.Assets;
using AQMod.Dusts;
using AQMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Projectiles
{
    public class UltimateSwordDrop : ModProjectile
    {
        public override string Texture => AQMod.TextureNone;

        public override void SetDefaults()
        {
            projectile.width = 0;
            projectile.height = 0;
            projectile.aiStyle = -1;
            projectile.friendly = true;
        }

        public override void AI()
        {
            if (projectile.width == 0 && projectile.height == 0)
            {
                var item = new Item();
                item.SetDefaults(ModContent.ItemType<UltimateSword>());
                projectile.width = item.width;
                projectile.height = item.height;
                projectile.position = new Vector2(projectile.position.X - projectile.width / 2f, projectile.position.Y - projectile.height / 2f);
            }
            projectile.velocity.X *= 0.98f;
            projectile.velocity.Y += 0.35f;
            projectile.rotation += MathHelper.Clamp(projectile.velocity.Length() * 0.0157f, 0.0728f, 0.157f);
        }

        public override void PostAI()
        {
            if (Main.rand.NextBool())
                Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<UltimateEnergyDust>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var texture = Main.itemTexture[ModContent.ItemType<UltimateSword>()];
            var frame = new Rectangle(0, 0, texture.Width, texture.Height);
            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, frame.Size() / 2f, projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(AQUtils.GetTextureobj<UltimateSword>("_Glow"), projectile.Center - Main.screenPosition, frame, new Color(250, 250, 250, 0), projectile.rotation, frame.Size() / 2f, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 36;
            height = 36;
            fallThrough = projectile.position.Y + projectile.height < Main.player[projectile.owner].position.Y;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Length() > 6f)
            {
                projectile.velocity = oldVelocity * -0.4f;
                projectile.velocity.X += Main.rand.NextFloat(-1f, 1f);
                return false;
            }
            return true;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Dig, projectile.position);
            AQItem.DropInstancedItem(projectile.owner, projectile.getRect(), ModContent.ItemType<Items.Weapons.Melee.UltimateSword>());
            var dust = ModContent.DustType<UltimateEnergyDust>();
            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, dust);
            }
        }
    }
}