﻿using AQMod.Buffs;
using AQMod.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Potions
{
    public class SpreadPotion : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 26;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.useAnimation = 17;
            item.useTime = 17;
            item.useTurn = true;
            item.UseSound = SoundID.Item3;
            item.consumable = true;
            item.rare = ItemRarityID.Green;
            item.value = AQItem.Prices.PotionValue;
            item.maxStack = 999;
            item.buffTime = 28800;
            item.buffType = ModContent.BuffType<DebuffSpread>();
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ItemID.BottledWater);
            r.AddIngredient(ItemID.Deathweed);
            r.AddIngredient(ItemID.Moonglow);
            r.AddIngredient(ModContent.ItemType<Fizzler>());
            r.AddTile(TileID.Bottles);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}