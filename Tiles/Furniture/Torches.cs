﻿using AQMod.Common.Graphics;
using AQMod.Dusts.NobleMushrooms;
using AQMod.Items.Placeable.Torch;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AQMod.Tiles.Furniture
{
    public class Torches : ModTile
    {
        public const int UltrabrightRedTorch = 0;
        public const int UltrabrightGreenTorch = 1;
        public const int UltrabrightBlueTorch = 2;
        public const int ExoticRedTorch = 3;
        public const int ExoticGreenTorch = 4;
        public const int ExoticBlueTorch = 5;
        public const int SparklingTorch = 6;

        public override void SetDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileWaterDeath[Type] = true;

            TileID.Sets.FramesOnKillWall[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(1);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(2);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.addAlternate(0);

            TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
            TileObjectData.newSubTile.LinkedAlternates = true;
            TileObjectData.newSubTile.WaterDeath = false;
            TileObjectData.newSubTile.LavaDeath = false;
            TileObjectData.newSubTile.WaterPlacement = LiquidPlacement.Allowed;
            TileObjectData.newSubTile.LavaPlacement = LiquidPlacement.NotAllowed;

            var waterTorch = TileObjectData.newSubTile;
            TileObjectData.addSubTile(UltrabrightRedTorch);
            TileObjectData.newSubTile.CopyFrom(waterTorch);
            TileObjectData.addSubTile(UltrabrightGreenTorch);
            TileObjectData.newSubTile.CopyFrom(waterTorch);
            TileObjectData.addSubTile(UltrabrightBlueTorch);

            TileObjectData.newSubTile.CopyFrom(waterTorch);
            TileObjectData.newSubTile.LavaPlacement = LiquidPlacement.Allowed;
            TileObjectData.addSubTile(SparklingTorch);

            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddMapEntry(new Color(0, 0, 255), Lang.GetItemName(ItemID.Torch));
            dustType = ModContent.DustType<ArgonDust>();
            drop = ModContent.ItemType<UltrabrightRedTorch>();
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Torches };
            torch = true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = Main.rand.Next(1, 3);
        }

        private const int TorchIntensityDistance = 300;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameX < 66)
            {
                switch (tile.frameY / 22)
                {
                    case ExoticRedTorch:
                        {
                            r = 1f;
                            g = 0f;
                            b = 0f;
                        }
                        break;

                    case ExoticGreenTorch:
                        {
                            r = 0f;
                            g = 1f;
                            b = 0f;
                        }
                        break;

                    case ExoticBlueTorch:
                        {
                            r = 0f;
                            g = 0f;
                            b = 1f;
                        }
                        break;

                    case UltrabrightRedTorch:
                        {
                            r = ((float)Math.Sin(Main.GlobalTime) + 1f) / 4f;
                            g = ((float)Math.Cos(Main.GlobalTime) + 1f) / 16f;
                            b = ((float)Math.Sin(Main.GlobalTime * 0.85f) + 1f) / 16f;

                            if (Main.tile[i, j].liquid > 0)
                            {
                                float intensityMult = 0.1f;
                                var screenPosition = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
                                var distance = (AQGraphics.ScreenCenter - screenPosition).Length();
                                if (distance < TorchIntensityDistance)
                                {
                                    intensityMult += 1f - distance / TorchIntensityDistance;
                                }
                                r *= intensityMult;
                                g *= intensityMult;
                                b *= intensityMult;
                            }
                        }
                        break;

                    case UltrabrightGreenTorch:
                        {
                            r = ((float)Math.Sin(Main.GlobalTime) + 1f) / 16f;
                            g = ((float)Math.Cos(Main.GlobalTime) + 1f) / 4f;
                            b = ((float)Math.Sin(Main.GlobalTime * 0.85f) + 1f) / 16f;

                            if (Main.tile[i, j].liquid > 0)
                            {
                                float intensityMult = 0.1f;
                                var screenPosition = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
                                var distance = (AQGraphics.ScreenCenter - screenPosition).Length();
                                if (distance < TorchIntensityDistance)
                                {
                                    intensityMult += 1f - distance / TorchIntensityDistance;
                                }
                                r *= intensityMult;
                                g *= intensityMult;
                                b *= intensityMult;
                            }
                        }
                        break;

                    case UltrabrightBlueTorch:
                        {
                            r = ((float)Math.Sin(Main.GlobalTime) + 1f) / 16f;
                            g = ((float)Math.Cos(Main.GlobalTime) + 1f) / 16f;
                            b = ((float)Math.Sin(Main.GlobalTime * 0.85f) + 1f) / 4f;

                            if (Main.tile[i, j].liquid > 0)
                            {
                                float intensityMult = 0.1f;
                                var screenPosition = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
                                var distance = (AQGraphics.ScreenCenter - screenPosition).Length();
                                if (distance < TorchIntensityDistance)
                                {
                                    intensityMult += 1f - distance / TorchIntensityDistance;
                                }
                                r *= intensityMult;
                                g *= intensityMult;
                                b *= intensityMult;
                            }
                        }
                        break;

                    case SparklingTorch:
                        {
                            r = 0.9f;
                            g = 0.9f;
                            b = 1f;
                        }
                        break;
                }
            }
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
        {
            offsetY = 0;
            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;
                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }
        }

        public override bool Drop(int i, int j)
        {
            switch (Main.tile[i, j].frameY / 22)
            {
                case UltrabrightRedTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<UltrabrightRedTorch>());
                    }
                    break;

                case UltrabrightGreenTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<UltrabrightGreenTorch>());
                    }
                    break;

                case UltrabrightBlueTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<UltrabrightBlueTorch>());
                    }
                    break;

                case ExoticRedTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<ExoticRedTorch>());
                    }
                    break;

                case ExoticGreenTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<ExoticGreenTorch>());
                    }
                    break;

                case ExoticBlueTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<ExoticBlueTorch>());
                    }
                    break;

                case SparklingTorch:
                    {
                        Item.NewItem(i * 16, j * 16, 16, 16, ModContent.ItemType<SparklingTorch>());
                    }
                    break;
            }
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            int torchFrameY = Main.tile[i, j].frameY / 22;
            switch (torchFrameY)
            {
                case ExoticRedTorch:
                case ExoticGreenTorch:
                case ExoticBlueTorch:
                    {
                        ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
                        Color color = new Color(100, 100, 100, 0);
                        int frameX = Main.tile[i, j].frameX;
                        int frameY = Main.tile[i, j].frameY;
                        int width = 20;
                        int offsetY = 0;
                        int height = 20;
                        if (WorldGen.SolidTile(i, j - 1))
                        {
                            offsetY = 2;
                            if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                            {
                                offsetY = 4;
                            }
                        }
                        Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                        if (Main.drawToScreen)
                        {
                            zero = Vector2.Zero;
                        }
                        for (int k = 0; k < 7; k++)
                        {
                            float x = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                            float y = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
                            Main.spriteBatch.Draw(ModContent.GetTexture(this.GetPath("_Flames")), new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + x, j * 16 - (int)Main.screenPosition.Y + offsetY + y) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                        }
                    }
                    break;

                case SparklingTorch:
                    {
                        ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
                        Color color = new Color(100, 100, 100, 0);
                        int frameX = Main.tile[i, j].frameX;
                        int frameY = Main.tile[i, j].frameY;
                        int width = 20;
                        int offsetY = 0;
                        int height = 20;
                        if (WorldGen.SolidTile(i, j - 1))
                        {
                            offsetY = 2;
                            if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                            {
                                offsetY = 4;
                            }
                        }
                        Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                        if (Main.drawToScreen)
                        {
                            zero = Vector2.Zero;
                        }
                        for (int k = 0; k < 7; k++)
                        {
                            float x = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                            float y = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
                            Main.spriteBatch.Draw(ModContent.GetTexture(this.GetPath("_Flames")), new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + x, j * 16 - (int)Main.screenPosition.Y + offsetY + y) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                        }
                    }
                    break;
            }
        }
    }
}