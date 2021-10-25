﻿using AQMod.Common.Utilities;
using AQMod.Items.Materials;
using AQMod.Localization;
using AQMod.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace AQMod.Common.WorldGeneration
{
    public class AQWorldGen : ModWorld
    {
        private static GenPass getPass(string name, WorldGenLegacyMethod method)
        {
            return new PassLegacy(name, method);
        }

        internal static void GenerateGlimmeringStatues(GenerationProgress progress)
        {
            if (progress != null)
                progress.Message = Language.GetTextValue(AQText.Key + "Common.WorldGen_GlimmeringStatues");
            int glimmerCount = 0;
            int glimmerMax = Main.maxTilesX / 1500;
            var rectLeft = new Rectangle(80, 40, 600, (int)Main.worldSurface - 100);
            var rectRight = new Rectangle(Main.maxTilesX - rectLeft.Width - rectLeft.X, rectLeft.Y, rectLeft.Width, rectLeft.Height);
            var rectSpace = new Rectangle(80, 40, Main.maxTilesX - 160, 180);
            for (int i = 0; i < 10000; i++)
            {
                int r = WorldGen.genRand.Next(3);
                Rectangle rect;
                switch (r)
                {
                    default:
                    rect = rectSpace;
                    break;

                    case 1:
                    rect = rectLeft;
                    break;

                    case 2:
                    rect = rectRight;
                    break;
                }
                int x = WorldGen.genRand.Next(rect.X, rect.X + rect.Width);
                int y = WorldGen.genRand.Next(rect.Y, rect.Y + rect.Height);
                if (GlimmeringStatue.TryGenGlimmeringStatue(x, y))
                {
                    glimmerCount++;
                    if (glimmerCount >= glimmerMax)
                        break;
                }
            }
        }

        internal static void GenerateGlobeTemples(GenerationProgress progress)
        {
            if (progress != null)
                progress.Message = Language.GetTextValue(AQText.Key + "Common.WorldGen_Globes");
            int templeCount = 0;
            int templeMax = Main.maxTilesX / 400;
            for (int i = 0; i < 10000; i++)
            {
                int x = WorldGen.genRand.Next(80, Main.maxTilesX - 80);
                int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, Main.maxTilesY - 300);
                if (Globe.GenGlobeTemple(x, y))
                {
                    templeCount++;
                    if (templeCount >= templeMax)
                        break;
                }
            }
        }

        internal static void GenerateGoreNests(GenerationProgress progress)
        {
            if (progress != null)
                progress.Message = Language.GetTextValue(AQText.Key + "Common.WorldGen_GoreNests");
            int goreNestCount = 0;
            for (int i = 0; i < 10000; i++)
            {
                int x = WorldGen.genRand.Next(80, Main.maxTilesX - 80);
                int y = WorldGen.genRand.Next(Main.maxTilesY - 300, Main.maxTilesY - 80);
                if (GoreNest.GrowGoreNest(x, y, true, true))
                {
                    goreNestCount++;
                    if (goreNestCount > 4)
                        break;
                }
            }
        }

        internal static void GenerateTikiChests(GenerationProgress progress)
        {
            progress.Message = Language.GetTextValue(AQText.Key + "Common.RandomStructures_TikiChest");
            int halfX = Main.maxTilesX / 2;
            bool flag = WorldGen.genRand.NextBool();
            int tikiChests = Main.maxTilesX / 900;
            for (int i = 0; i < 100000; i++)
            {
                int xOffset = WorldGen.genRand.Next(80, 200);
                int x = flag ? Main.maxTilesX - xOffset : xOffset;
                int y = WorldGen.genRand.Next(200, 500);
                if (TryPlaceTikiChest(x, y, out int chest))
                {
                    Chest c = Main.chest[chest];
                    int index = 0;
                    c.item[index].SetDefaults(ModContent.ItemType<CrabShell>());
                    c.item[index].stack = WorldGen.genRand.Next(18, 25) + 1;
                    index++;
                    tikiChests--;
                    if (tikiChests <= 0)
                        break;
                }
            }
            for (int i = 0; i < 100000; i++)
            {
                int _ = WorldGen.genRand.Next(80, 200);
                int x = flag ? Main.maxTilesX - _ : _;
                int y = WorldGen.genRand.Next(200, 500);
                if (TryPlaceFakeTikiChest(x, y))
                    break;
            }
        }

        public static bool TryPlaceTikiChest(int x, int y, out int chest)
        {
            if (check2x2_Liquid(x, y) && check1x3_Liquid(x - 1, y) && check1x3_Liquid(x + 2, y))
            {
                chest = WorldGen.PlaceChest(x, y, 21, false, 0);
                WorldGen.PlaceTile(x - 1, y, TileID.Lamps);
                WorldGen.PlaceTile(x + 2, y, TileID.Lamps);
                return true;
            }
            chest = -1;
            return false;
        }

        public static bool TryPlaceFakeTikiChest(int x, int y)
        {
            if (check2x2_Liquid(x, y) && check1x3_Liquid(x - 1, y) && check1x3_Liquid(x + 2, y))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (ActiveAndUncuttable(x, y + 4 + i) && NotSloped(x, y + 4 + i))
                    {
                        WorldGen.KillTile(x, y + 3 + i);
                        WorldGen.PlaceTile(x, y + 3 + i, TileID.Explosives);
                        AQUtils.TileABLine(x, y, x, y + 3 + i, delegate (int x2, int y2)
                        {
                            WorldGen.PlaceWire(x2, y2);
                            return true;
                        });
                        AQUtils.TileABLine(x - 1, y, x + 2, y, delegate (int x2, int y2)
                        {
                            WorldGen.PlaceWire(x2, y2);
                            return true;
                        });
                        WorldGen.Place2x2(x + 1, y, TileID.FakeContainers, 0);
                        WorldGen.PlaceTile(x - 1, y, TileID.Lamps);
                        WorldGen.PlaceTile(x + 2, y, TileID.Lamps);
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void GenerateNobleMushrooms(GenerationProgress progress)
        {
            progress.Message = Language.GetTextValue(AQText.Key + "Common.RandomStructures_NobleMushrooms");
            for (int i = 0; i < 5000; i++)
            {
                int x = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
                int y = WorldGen.genRand.Next((int)WorldGen.rockLayer, Main.maxTilesY - 240);
                int style = WorldGen.genRand.Next(3);
                int size = WorldGen.genRand.Next(30, 75);
                if (TryPlaceNobleGroup(x, y, style, size))
                {
                    TryPlaceNobleGroup(x, y, style, size + 10);
                    TryPlaceNobleGroup(x, y, style, size + 10);
                    i += 450;
                }
            }
        }

        public static bool TryPlaceNobleGroup(int x, int y, int style, int size)
        {
            var halfSize = size / 2;
            var area = new Rectangle(x - halfSize, y - halfSize, size, size);
            int requiredMushrooms = size / 15;
            var validSpots = new List<Point>();
            var invalidX = new List<int>();
            var tileType = ModContent.TileType<NobleMushrooms>();
            for (int j = area.Y; j < area.Y + size; j++)
            {
                for (int i = area.X; i < area.X + size; i++)
                {
                    var invalidXIndex = invalidX.FindIndex((x2) => x2.Equals(i));
                    if (invalidXIndex != -1)
                    {
                        i++;
                        continue;
                    }
                    if (check2x2(i, j))
                    {
                        var type = Main.tile[i, j + 1].type;
                        bool canPlace = false;
                        for (int k = 0; k < NobleMushrooms.generationTiles.Length; k++)
                        {
                            if (NobleMushrooms.generationTiles[k] == type)
                            {
                                canPlace = true;
                                break;
                            }
                        }
                        type = Main.tile[i + 1, j + 1].type;
                        bool canPlace2 = false;
                        for (int k = 0; k < NobleMushrooms.generationTiles.Length; k++)
                        {
                            if (NobleMushrooms.generationTiles[k] == type)
                            {
                                canPlace2 = true;
                                break;
                            }
                        }
                        if (canPlace && canPlace2)
                        {
                            validSpots.Add(new Point(i, j));
                            invalidX.Add(i);
                            i++;
                        }
                    }
                }
            }
            if (validSpots.Count < requiredMushrooms)
                return false;
            for (int i = 0; i < requiredMushrooms; i++)
            {
                int index = WorldGen.genRand.Next(validSpots.Count);
                WorldGen.Place2x2Horizontal(validSpots[index].X, validSpots[index].Y, (ushort)tileType, style);
                validSpots.RemoveAt(index);
            }
            return true;
        }

        internal static void GenerateCandelabraTraps(GenerationProgress progress)
        {
            progress.Message = Language.GetTextValue(AQText.Key + "Common.RandomStructures_CandelabraTraps");
            int halfX = Main.maxTilesX / 2;
            int candelabraTrapCount = Main.expertMode ? 7 : 3;
            int candelabras = 0;
            for (int i = 0; i < 100000; i++)
            {
                int x = WorldGen.genRand.Next(halfX - 350, halfX + 350);
                int y = WorldGen.genRand.Next(500, Main.maxTilesY - 300);
                if (TryPlaceCandelabraTrap(x, y))
                {
                    candelabras++;
                    if (candelabras >= candelabraTrapCount)
                        break;
                }
            }
        }

        public static bool TryPlaceCandelabraTrap(int x, int y)
        {
            if (check2x2(x, y))
            {
                var candelabra = new Rectangle(x, y - 1, 2, 2);
                var pressurePlate = new Point();
                var search = new Rectangle(x - 20, y - 20, 40, 30);
                for (int j = 0; j < 150; j++)
                {
                    int x2 = WorldGen.genRand.Next(search.X, search.X + search.Width + 1);
                    int y2 = WorldGen.genRand.Next(search.Y, search.Y + search.Height + 1);
                    if (!candelabra.Contains(x2, y2) && pressurePlateCheck(x2, y2))
                        pressurePlate = new Point(x2, y2);
                }
                if (pressurePlate != new Point()) // I always want to have a pressure plate that toggles the candelabra 
                {
                    var pressurePlate2 = new Point();
                    var invalidRect = new Rectangle(pressurePlate.X - 2, pressurePlate.Y - 2, 4, 4);
                    for (int j = 0; j < 100; j++)
                    {
                        int x2 = WorldGen.genRand.Next(search.X, search.X + search.Width + 1);
                        int y2 = WorldGen.genRand.Next(search.Y, search.Y + search.Height + 1);
                        if (!candelabra.Contains(x2, y2) && !invalidRect.Contains(x2, y2) && pressurePlateCheck(x2, y2))
                            pressurePlate2 = new Point(x2, y2);
                    }
                    WorldGen.KillTile(pressurePlate.X, pressurePlate.Y);
                    Main.tile[pressurePlate.X, pressurePlate.Y + 1].active(active: true);
                    WorldGen.PlaceTile(pressurePlate.X, pressurePlate.Y, TileID.PressurePlates, true, true);
                    AQUtils.TileABLine(pressurePlate.X, pressurePlate.Y, x, y, delegate (int x3, int y3)
                    {
                        WorldGen.PlaceWire(x3, y3);
                        return true;
                    });
                    AQUtils.RectangleMethod(candelabra, delegate (int x3, int y3)
                    {
                        if (Main.tileCut[Main.tile[x3, y3].type])
                            WorldGen.KillTile(x3, y3);
                        return true;
                    });
                    WorldGen.PlaceTile(x + 1, y, TileID.PlatinumCandelabra, true, true);
                    if (pressurePlate2 != new Point())
                    {
                        var dynamite = new Point();
                        for (int j = 0; j < 5; j++)
                        {
                            if (Framing.GetTileSafely(x, y + 4 + j).active() &&
                                ActiveAndSolid(x, y + 5 + j) && NotSloped(x, y + 5 + j))
                            {
                                dynamite = new Point(x, y + 5 + j);
                                break;
                            }
                        }
                        if (dynamite != new Point())
                        {
                            WorldGen.PlaceTile(pressurePlate2.X, pressurePlate2.Y, TileID.PressurePlates, true, true);
                            AQUtils.TileABLine(pressurePlate2.X, pressurePlate2.Y, dynamite.X, dynamite.Y, delegate (int x3, int y3)
                            {
                                WorldGen.PlaceWire2(x3, y3);
                                return true;
                            });
                            WorldGen.KillTile(dynamite.X, dynamite.Y);
                            WorldGen.PlaceTile(dynamite.X, dynamite.Y, TileID.Explosives, true, true);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public static void GenerateExoticBlotches(GenerationProgress progress)
        {
            progress.Message = Language.GetTextValue(AQText.Key + "Common.RandomStructures_ExoticCoralBlotches");
            for (int i = 0; i < 15000; i++)
            {
                int x = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
                int y = WorldGen.genRand.Next(200, Main.maxTilesY - 240);
                int style = WorldGen.genRand.Next(3);
                int size = WorldGen.genRand.Next(50, 150);
                if (ExoticCoral.TryPlaceExoticBlotch(x, y, style, size))
                    i += 150;
            }
        }

        internal static void GenerateOceanRavines(GenerationProgress progress)
        {
            progress.Message = Language.GetTextValue(AQText.Key + "Common.OceanRavines");
            for (int i = 0; i < 5000; i++)
            {
                int x = WorldGen.genRand.Next(90, 200);
                if (WorldGen.genRand.NextBool())
                    x = Main.maxTilesX - x;
                for (int j = 200; j < Main.worldSurface; j++)
                {
                    if (CanPlaceOceanRavine(x, j))
                    {
                        int style = WorldGen.genRand.Next(3) + 3;
                        PlaceOceanRavine(x, j, style);
                        i += 1000;
                        break;
                    }
                }
            }
        }

        public static bool CanPlaceOceanRavine(int x, int y)
        {
            return !Framing.GetTileSafely(x, y).active() && Main.tile[x, y].liquid > 0 && Framing.GetTileSafely(x, y + 1).active() && Main.tileSand[Main.tile[x, y + 1].type];
        }

        public static void PlaceOceanRavine(int x, int y, int torchStyle = -1, int tileType = TileID.Sandstone, int tileType2 = TileID.HardenedSand, int wallType = WallID.Sandstone)
        {
            int height = WorldGen.genRand.Next(40, 120);
            int digDir = x < Main.maxTilesX / 2 ? 1 : -1;
            int[] xAdds = new int[height];
            int x3 = x;
            int x2 = x;
            for (int i = 0; i < height; i++)
            {
                if (WorldGen.genRand.NextBool())
                {
                    xAdds[i] = digDir;
                    x += xAdds[i];
                }
                for (int j = 0; j < 20; j++)
                {
                    WorldGen.PlaceTile(x - 10 + j, y + i, tileType, true, true);
                    Framing.GetTileSafely(x - 10 + j, y + i).wall = (ushort)wallType;
                }
                WorldGen.TileRunner(x, y + i, WorldGen.genRand.Next(15, 25), 5, tileType, true);
                WorldGen.TileRunner(x, y + i, WorldGen.genRand.Next(20, 28), 5, tileType2, true);
            }
            for (int i = 0; i < 10; i++)
            {
                WorldGen.digTunnel(x2, y - i, digDir, 1f, 2, 6, true);
                for (int j = 0; j < 20; j++)
                {
                    Framing.GetTileSafely(x - 10 + j, y + height + i).wall = (ushort)wallType;
                }
            }
            for (int i = 0; i < height; i++)
            {
                x2 += xAdds[i];
                WorldGen.digTunnel(x2, y + i, digDir, 1f, 2, 6, true);
            }
            WorldGen.TileRunner(x, y + height + 4, WorldGen.genRand.Next(20, 28), 6, tileType, true);
            WorldGen.TileRunner(x, y + height + 4, WorldGen.genRand.Next(20, 33), 6, tileType2, true);
            WorldGen.digTunnel(x - digDir, y + height - 6, digDir, 1f, 5, 6, true);
            WorldGen.digTunnel(x - digDir, y + height - 5, digDir, 1f, 5, 6, true);
            WorldGen.digTunnel(x - digDir, y + height - 4, digDir, 1f, 5, 6, true);
            WorldGen.digTunnel(x, y + height - 3, digDir, 1f, 5, 6, true);
            WorldGen.digTunnel(x, y + height - 2, digDir, 1f, 5, 6, true);
            WorldGen.digTunnel(x, y + height - 1, digDir, 1f, 5, 6, true);
            WorldGen.digTunnel(x, y + height, digDir, 1f, 5, 6, true);
            if (torchStyle != -1)
            {
                for (int i = 0; i < height; i++)
                {
                    x3 += xAdds[i];
                    if (WorldGen.genRand.NextBool(5))
                    {
                        for (int j = 0; j < 15; j++)
                        {
                            if (WorldGen.genRand.NextBool())
                            {
                                if (!Framing.GetTileSafely(x3 + j, y + i).active() && Main.tile[x3 + j, y + i].liquid > 0)
                                {
                                    WorldGen.PlaceTile(x3 + j, y + i, ModContent.TileType<Torches>(), true, false, -1, torchStyle);
                                    if (Framing.GetTileSafely(x3 + j, y + i).active() && Main.tile[x3 + j, y + i].type == ModContent.TileType<Torches>())
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool PlaceRobsterQuestTile(int style = 6)
        {
            int x = WorldGen.genRand.Next(90, 200);
            if (WorldGen.genRand.NextBool())
                x = Main.maxTilesX - x;
            for (int j = 200; j < Main.worldSurface; j++)
            {
                if (!Framing.GetTileSafely(x, j).active() && Main.tile[x, j].liquid > 0 && Framing.GetTileSafely(x, j + 1).active())
                {
                    WorldGen.PlaceTile(x, j, ModContent.TileType<ExoticCoral>(), true, false, -1, style);
                    if (Framing.GetTileSafely(x, j).active() && Main.tile[x, j].type == ModContent.TileType<ExoticCoral>())
                        return true;
                }
            }
            return false;
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int i;
            i = tasks.FindIndex((t) => t.Name.Equals("Beaches"));
            if (i != -1)
            {
                i++;
                tasks.Insert(i, getPass("Ocean Ravines", GenerateOceanRavines));
            }
            i = tasks.FindIndex((t) => t.Name.Equals("Hellforge"));
            if (i != -1)
            {
                i++;
                tasks.Insert(i, getPass("Gore Nests", GenerateGoreNests));
                tasks.Insert(i, getPass("Globe Temples", GenerateGlobeTemples));
                tasks.Insert(i, getPass("Glimmering Statues", GenerateGlimmeringStatues));
            }
            i = tasks.FindIndex((t) => t.Name.Equals("Micro Biomes"));
            if (i != -1)
            {
                i++;
                tasks.Insert(i, getPass("Noble Mushrooms", GenerateNobleMushrooms));
                tasks.Insert(i, getPass("Tiki Chests", GenerateTikiChests));
                tasks.Insert(i, getPass("Candelabra Traps", GenerateCandelabraTraps));
                tasks.Insert(i, getPass("Exotic Coral", GenerateExoticBlotches));
            }
        }

        internal static void KillRectangle(Rectangle rect)
        {
            for (int i = rect.X; i < rect.X + rect.Width; i++)
            {
                for (int j = rect.Y; j < rect.Y + rect.Height; j++)
                {
                    WorldGen.KillTile(i, j);
                }
            }
        }

        internal static void KillCuttable(Rectangle rect)
        {
            for (int i = rect.X; i <= rect.X + rect.Width; i++)
            {
                for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
                {
                    if (Framing.GetTileSafely(i, j).active() && Main.tileCut[Main.tile[i, j].type])
                        WorldGen.KillTile(i, j);
                }
            }
        }

        internal static bool pressurePlateCheck(int x, int y)
        {
            return !ActiveAndUncuttable(x, y) &&
            !ActiveAndUncuttable(x + 1, y) &&
            !ActiveAndUncuttable(x, y - 1) &&
            !ActiveAndUncuttable(x + 1, y - 1) &&
            !ActiveAndUncuttable(x, y - 2) &&
            !ActiveAndUncuttable(x + 1, y - 2) &&
            ActiveAndSolid(x, y + 1) && NotSloped(x, y + 1);
        }

        internal static bool NotSloped(int x, int y)
        {
            return Main.tile[x, y].slope() == 0 && !Main.tile[x, y].halfBrick();
        }

        internal static bool ActiveAndSolid(int x, int y)
        {
            return Framing.GetTileSafely(x, y).active() && Main.tileSolid[Main.tile[x, y].type] && !Main.tileCut[Main.tile[x, y].type];
        }

        internal static bool ActiveAndFullySolid(int x, int y)
        {
            return Framing.GetTileSafely(x, y).active() && Main.tileSolid[Main.tile[x, y].type] && !Main.tileSolidTop[Main.tile[x, y].type] && !Main.tileCut[Main.tile[x, y].type];
        }

        internal static bool ActiveAndUncuttable(int x, int y)
        {
            return Framing.GetTileSafely(x, y).active() && !Main.tileCut[Main.tile[x, y].type];
        }

        internal static bool check1x3(int x, int y)
        {
            return !ActiveAndUncuttable(x, y) &&
                   !ActiveAndUncuttable(x, y - 1) &&
                   !ActiveAndUncuttable(x, y - 2) &&
                   ActiveAndUncuttable(x, y + 1) && NotSloped(x, y + 1);
        }

        internal static bool check1x3_Liquid(int x, int y)
        {
            return !ActiveAndUncuttable(x, y) && Main.tile[x, y].liquid == 0 &&
                   !ActiveAndUncuttable(x, y - 1) && Main.tile[x, y - 1].liquid == 0 &&
                   !ActiveAndUncuttable(x, y - 2) && Main.tile[x, y - 2].liquid == 0 &&
                   ActiveAndUncuttable(x, y + 1) && NotSloped(x, y + 1);
        }

        internal static bool check2x2(int x, int y)
        {
            return !ActiveAndUncuttable(x, y) &&
                   !ActiveAndUncuttable(x + 1, y) &&
                   !ActiveAndUncuttable(x, y - 1) &&
                   !ActiveAndUncuttable(x + 1, y - 1) &&
                   ActiveAndSolid(x, y + 1) && NotSloped(x, y + 1) &&
                   ActiveAndSolid(x + 1, y + 1) && NotSloped(x + 1, y + 1);
        }

        internal static bool check2x2_Liquid(int x, int y)
        {
            return !ActiveAndUncuttable(x, y) && Main.tile[x, y].liquid == 0 &&
                   !ActiveAndUncuttable(x + 1, y) && Main.tile[x + 1, y].liquid == 0 &&
                   !ActiveAndUncuttable(x, y - 1) && Main.tile[x, y - 1].liquid == 0 &&
                   !ActiveAndUncuttable(x + 1, y - 1) && Main.tile[x + 1, y - 1].liquid == 0 &&
                   ActiveAndSolid(x, y + 1) && NotSloped(x, y + 1) &&
                   ActiveAndSolid(x + 1, y + 1) && NotSloped(x + 1, y + 1);
        }
    }
}