﻿using AQMod.Items.Recipes;
using AQMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Materials
{
    public class Lightbulb : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 12;
            item.height = 12;
            item.maxStack = 999;
            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(silver: 5);
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 10;
            item.useAnimation = 15;
            item.createTile = ModContent.TileType<LightbulbTile>();
            item.autoReuse = true;
            item.useTurn = true;
            item.consumable = true;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ItemID.Glass);
            r.AddIngredient(ItemID.FallenStar);
            r.AddRecipeGroup(AQRecipeGroups.CopperOrTin, 1);
            r.AddTile(TileID.Anvils);
            r.SetResult(this, 2);
            r.AddRecipe();
        }
    }
}