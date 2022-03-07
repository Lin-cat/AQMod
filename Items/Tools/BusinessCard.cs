﻿using AQMod.Items.Materials.Energies;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Tools
{
    public class BusinessCard : ModItem, IUpdatePiggybank
    {
        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 24;
            item.accessory = true;
            item.rare = ItemRarityID.Yellow;
            item.value = Item.sellPrice(gold: 8);
        }

        private void Update(Player player)
        {
            var aQPlayer = player.GetModPlayer<AQPlayer>();
            if (aQPlayer.discountPercentage > 0.6f)
                aQPlayer.discountPercentage = 0.6f;
            player.discount = true;
        }

        public override void UpdateInventory(Player player)
        {
            Update(player);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            Update(player);
        }

        void IUpdatePiggybank.UpdatePiggyBank(Player player, int i)
        {
            Update(player);
        }

        public override void AddRecipes()
        {
            var recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DiscountCard);
            recipe.AddIngredient(ModContent.ItemType<DemonicEnergy>(), 5);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}