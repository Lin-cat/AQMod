﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Accessories
{
    public class Extractor : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(gold: 1);
            item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Suffocation] = true;
            var aQPlayer = player.GetModPlayer<AQPlayer>();
            aQPlayer.extractinator = true;
            if (!hideVisual)
            {
                aQPlayer.extractinatorVisible = true;
                aQPlayer.VeinmineTiles[TileID.Silt] = true;
                aQPlayer.VeinmineTiles[TileID.Slush] = true;
                aQPlayer.VeinmineTiles[TileID.DesertFossil] = true;
            }
        }
    }
}