﻿using AQMod.Common.Graphics;
using AQMod.Items.Materials.Energies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Weapons.Melee
{
    public class StariteSpinner : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Yoyo[item.type] = true;
            ItemID.Sets.GamepadExtraRange[item.type] = 12;
            ItemID.Sets.GamepadSmartQuickReach[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.useAnimation = 25;
            item.useTime = 25;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.melee = true;
            item.damage = 16;
            item.knockBack = 2.11f;
            item.value = AQItem.Prices.GlimmerWeaponValue;
            item.UseSound = SoundID.Item1;
            item.rare = AQItem.Rarities.StariteWeaponRare;
            item.channel = true;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.shootSpeed = 10f;
            item.shoot = ModContent.ProjectileType<Projectiles.Melee.Yoyo.StariteSpinner>();
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 255);
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            spriteBatch.Draw(Main.itemTexture[item.type], position, frame, new Color(255, 255, 255, 0) * AQUtils.Wave(Main.GlobalTime * 3f, 0f, 0.3f), 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            AQGraphics.Rendering.FallenStarAura(item, spriteBatch, scale, new Color(20, 20, 80, 50), new Color(50, 50, 180, 127));
            Rectangle frame;
            if (Main.itemAnimations[item.type] != null)
            {
                frame = Main.itemAnimations[item.type].GetFrame(Main.itemTexture[item.type]);
            }
            else
            {
                frame = Main.itemTexture[item.type].Frame();
            }
            var origin = frame.Size() / 2f;
            var drawCoordinates = item.position - Main.screenPosition + origin + new Vector2(item.width / 2 - origin.X, item.height - frame.Height);
            Main.spriteBatch.Draw(Main.itemTexture[item.type], drawCoordinates, frame, item.GetAlpha(lightColor), rotation, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Main.itemTexture[item.type], drawCoordinates, frame, new Color(255, 255, 255, 0) * AQUtils.Wave(Main.GlobalTime * 6f, 0f, 0.5f), rotation, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ItemID.WoodYoyo);
            r.AddIngredient(ItemID.FallenStar, 5);
            r.AddIngredient(ModContent.ItemType<CosmicEnergy>());
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}