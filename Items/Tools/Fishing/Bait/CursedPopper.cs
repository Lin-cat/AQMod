﻿using AQMod.Effects.WorldEffects;
using AQMod.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Tools.Fishing.Bait
{
    public class CursedPopper : PopperBaitItem
    {
        public override void SetDefaults()
        {
            item.width = 6;
            item.height = 6;
            item.bait = 30;
            item.maxStack = 999;
            item.consumable = true;
            item.value = Item.sellPrice(silver: 1);
            item.rare = ItemRarityID.Green;
        }

        public override int GetExtraFishingPower(Player player, AQPlayer aQPlayer)
        {
            if (player.ZoneCorrupt)
                return 30;
            return 0;
        }

        public override void PopperEffects(Player player, AQPlayer aQPlayer, Projectile bobber, Tile tile)
        {
            AQMod.WorldEffects.Add(new FishingPopperEffect((int)bobber.position.X, (int)bobber.position.Y, tile.liquid, 75, default(Color)));
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ItemID.CursedFlame, 10);
            r.AddIngredient(ItemID.UnholyWater);
            r.AddTile(ModContent.TileType<FishingCraftingStation>());
            r.SetResult(this, 10);
            r.AddRecipe();
        }
    }
}