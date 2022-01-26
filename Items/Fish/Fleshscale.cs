﻿using AQMod.Content.Fishing;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Fish
{
    public class Fleshscale : FishingItem
    {
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.value = Item.sellPrice(silver: 20);
            item.rare = ItemRarityID.Blue;
            item.maxStack = 999;
        }

        public override bool ValidCatchingLocation(Player player, AQPlayer aQPlayer, Item fishingRod, Item bait, int power, int liquidType, int worldLayer, int questFish)
        {
            return liquidType == Tile.Liquid_Water && worldLayer <= FishLoader.WorldLayers.HellLayer && player.ZoneCrimson && NPC.downedBoss2;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(item.type);
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.TissueSample, 5);
            r.AddRecipe();
        }
    }
}