﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Placeable.Nature
{
    public class ExoticStarfish : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 12;
            item.height = 12;
            item.value = Item.sellPrice(copper: 50);
            item.maxStack = 999;
            item.useTime = 10;
            item.useAnimation = 15;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.createTile = ModContent.TileType<Tiles.Nature.CrabCrevice.ExoticCoral>();
            item.consumable = true;
            item.placeStyle = 8;
            item.useTurn = true;
            item.autoReuse = true;
        }
    }
}