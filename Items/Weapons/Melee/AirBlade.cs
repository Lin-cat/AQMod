﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Weapons.Melee
{
    public class AirBlade : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.melee = true;
            item.damage = 90;
            item.knockBack = 3f;
            item.useAnimation = 12;
            item.useTime = 12;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.UseSound = SoundID.Item1;
            item.rare = AQItem.RarityGaleStreams;
            item.value = Item.sellPrice(silver: 75);
            item.shootSpeed = 12.5f;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.shoot = ModContent.ProjectileType<Projectiles.Melee.AirBlade>();
            item.autoReuse = true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(250, 250, 250, 250) * AQUtils.Wave(Main.GlobalTime * 6f, 0.9f, 1f);
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[item.shoot] < 1;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            damage = (int)(damage * 0.5f);
            return true;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<CrystalDagger>());
            r.AddIngredient(ModContent.ItemType<Materials.Energies.AtmosphericEnergy>());
            r.AddIngredient(ModContent.ItemType<Materials.Energies.OrganicEnergy>(), 2);
            r.AddIngredient(ItemID.SoulofFlight, 12);
            r.AddIngredient(ItemID.SoulofLight, 8);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}