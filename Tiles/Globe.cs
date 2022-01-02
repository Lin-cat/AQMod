﻿using AQMod.Common.WorldGeneration;
using AQMod.Items.Placeable;
using AQMod.Tiles.TileEntities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AQMod.Tiles
{
    public class Globe : ModTile
    {
        public static bool GenGlobeTemple(int x, int y)
        {
            if (!AQWorldGen.ActiveAndSolid(x, y) && AQWorldGen.ActiveAndSolid(x, y + 1) && Main.tile[x, y].wall == WallID.None)
            {
                Main.tile[x, y + 1].type = TileID.GrayBrick;
                Main.tile[x, y + 1].halfBrick(halfBrick: false);
                Main.tile[x, y + 1].slope(slope: 0);
                Framing.GetTileSafely(x + 1, y + 1).active(active: true);
                Main.tile[x + 1, y + 1].type = TileID.GrayBrick;
                Main.tile[x + 1, y + 1].halfBrick(halfBrick: false);
                Main.tile[x + 1, y + 1].slope(slope: 0);
                Framing.GetTileSafely(x - 1, y + 1).active(active: true);
                Main.tile[x - 1, y + 1].type = TileID.GrayBrick;
                Main.tile[x - 1, y + 1].halfBrick(halfBrick: false);
                Main.tile[x - 1, y + 1].slope(slope: 0);
                Framing.GetTileSafely(x + 2, y + 1).active(active: true);
                Main.tile[x + 2, y + 1].type = TileID.GrayBrick;
                Main.tile[x + 2, y + 1].halfBrick(halfBrick: false);
                Main.tile[x + 2, y + 1].slope(slope: 0);
                Framing.GetTileSafely(x - 2, y + 1).active(active: true);
                Main.tile[x - 2, y + 1].type = TileID.GrayBrick;
                Main.tile[x - 2, y + 1].halfBrick(halfBrick: false);
                Main.tile[x - 2, y + 1].slope(slope: 0);
                int height = WorldGen.genRand.Next(3) + 5;
                for (int i = 0; i < height; i++)
                {
                    Framing.GetTileSafely(x + 1, y - i).active(active: false);
                    Main.tile[x + 1, y - i].wall = WallID.Stone;
                    Framing.GetTileSafely(x, y - i).active(active: false);
                    Main.tile[x, y - i].wall = WallID.Stone;
                    Framing.GetTileSafely(x - 1, y - i).active(active: false);
                    Main.tile[x - 1, y - i].wall = WallID.Stone;
                    Framing.GetTileSafely(x - 2, y - i).active(active: true);
                    Main.tile[x - 2, y - i].type = TileID.WoodenBeam;
                    Framing.GetTileSafely(x + 2, y - i).active(active: true);
                    Main.tile[x + 2, y - i].type = TileID.WoodenBeam;
                }
                for (int i = 0; i < 5; i++)
                {
                    Framing.GetTileSafely(x - 2 + i, y + 2).active(active: true);
                    Main.tile[x - 2 + i, y + 2].type = TileID.GrayBrick;
                    Framing.GetTileSafely(x - 2 + i, y - height).active(active: true);
                    Main.tile[x - 2 + i, y - height].type = TileID.GrayBrick;
                }
                for (int i = 0; i < 3; i++)
                {
                    Framing.GetTileSafely(x - 1 + i, y - height - 1).active(active: true);
                    Main.tile[x - 1 + i, y - height - 1].type = TileID.GrayBrick;
                }
                WorldGen.PlaceTile(x, y, TileID.Tables);
                Globe.PlaceUndisoveredGlobe(x + WorldGen.genRand.Next(2), y - 2);
                return true;
            }
            return false;
        }

        public static bool PlaceUndisoveredGlobe(int x, int y)
        {
            return PlaceUndiscoveredGlobe(new Point(x, y));
        }

        public static bool PlaceUndiscoveredGlobe(Point p)
        {
            WorldGen.PlaceTile(p.X, p.Y, ModContent.TileType<Globe>());
            Tile tile = Main.tile[p.X, p.Y];
            if (tile.type == ModContent.TileType<Globe>())
            {
                var objectData = TileObjectData.GetTileData(tile);
                int x = p.X - tile.frameX % 36 / 18;
                int y = p.Y - tile.frameY / 18;
                objectData.HookPostPlaceMyPlayer.hook(x, y, tile.type, objectData.Style, 0);
                int index = ModContent.GetInstance<TEGlobe>().Find(x, y);
                if (index == -1)
                {
                    return false;
                }
                TEGlobe globe = (TEGlobe)TileEntity.ByID[index];
                globe.Discovered = false;
                return true;
            }
            return false;
        }

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorInvalidTiles = new[] { (int)TileID.MagicalIceBlock, };
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TEGlobe>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);
            dustType = DustID.Stone;
            disableSmartCursor = true;
            AddMapEntry(new Color(180, 180, 180), Lang.GetItemName(ModContent.ItemType<GlobeItem>()));
        }

        public override bool HasSmartInteract()
        {
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int x = i - (tile.frameX % 36 / 18);
            int y = j - (tile.frameY / 18);
            int index = ModContent.GetInstance<TEGlobe>().Find(x, y);
            var plr = Main.LocalPlayer;
            if (index == -1)
            {
                plr.noThrow = 2;
                plr.showItemIcon = true;
                plr.showItemIcon2 = ModContent.ItemType<GlobeItem>();
                return;
            }
            TEGlobe globe = (TEGlobe)TileEntity.ByID[index];
            var item = AQMod.MapMarkers.FindMarker(Main.player[Main.myPlayer].inventory);
            if (item.Item1 != null)
            {
                plr.noThrow = 2;
                plr.showItemIcon = true;
                plr.showItemIcon2 = item.Item1.type;
            }
        }

        public override bool NewRightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int x = i - tile.frameX % 36 / 18;
            int y = j - tile.frameY / 18;
            int index = ModContent.GetInstance<TEGlobe>().Find(x, y);
            if (index == -1)
            {
                return false;
            }
            TEGlobe globe = (TEGlobe)TileEntity.ByID[index];
            var item = AQMod.MapMarkers.FindMarker(Main.player[Main.myPlayer].inventory);
            if (item.Item1 != null && !globe.HasMarker(item.Item2))
            {
                Main.PlaySound(SoundID.Grab);
                item.Item2.OnAddToGlobe(Main.LocalPlayer, Main.LocalPlayer.GetModPlayer<AQPlayer>(), globe);
                globe.AddMarker(item.Item2);
                Main.LocalPlayer.ConsumeItem(item.Item1.type);
            }
            //else
            //{
            //    foreach (var marker in globe.Markers)
            //    {
            //        Main.NewText(marker.Name);
            //    }
            //}
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var plr = Main.LocalPlayer;
            if (Vector2.Distance(new Vector2(i * 16f, j * 16f), plr.Center) < 200f)
            {
                Tile tile = Main.tile[i, j];
                int x = i - (tile.frameX % 36 / 18);
                int y = j - (tile.frameY / 18);
                int index = ModContent.GetInstance<TEGlobe>().Find(x, y);
                if (index == -1)
                {
                    return;
                }
                TEGlobe globe = (TEGlobe)TileEntity.ByID[index];
                globe.Discover();
                var aQPlayer = Main.LocalPlayer.GetModPlayer<AQPlayer>();
                aQPlayer.nearGlobe = 32;
                aQPlayer.globeX = (ushort)globe.Position.X;
                aQPlayer.globeY = (ushort)globe.Position.Y;
                foreach (var m in globe.Markers)
                {
                    m.NearbyEffects(globe);
                }
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 32, 32, ModContent.ItemType<GlobeItem>());
            ModContent.GetInstance<TEGlobe>().Kill(i, j);
        }
    }
}