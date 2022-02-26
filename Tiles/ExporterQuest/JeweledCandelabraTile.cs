﻿using AQMod.Items.Misc.ExporterQuest;
using AQMod.NPCs.Friendly;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AQMod.Tiles.ExporterQuest
{
    public class JeweledCandelabraTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);
            AddMapEntry(Robster.JeweledTileMapColor, Lang.GetItemName(ModContent.ItemType<JeweledCandelabra>()));
            soundStyle = SoundID.Dig;
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Chandeliers };
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Robster.CheckTileBreakSights(i, j, alsoResetQuest: true) == false)
            {
                Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<JeweledCandelabra>());
            }
        }
    }
}