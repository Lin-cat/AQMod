﻿using AQMod.Items.Misc;
using AQMod.NPCs.Friendly;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AQMod.Tiles.ExporterQuest
{
    public class JeweledChaliceTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);
            AddMapEntry(Robster.JeweledTileMapColor, Lang.GetItemName(ModContent.ItemType<JeweledChalice>()));
            soundStyle = SoundID.Dig;
            disableSmartCursor = true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }

        public override bool Drop(int i, int j)
        {
            if (Robster.CheckTileBreakSights(i, j, alsoResetQuest: true) == false)
            {
                Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<JeweledChalice>());
            }
            return true;
        }
    }
}
