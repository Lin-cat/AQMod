﻿using AQMod.Dusts.NobleMushrooms;
using AQMod.Items.Materials.NobleMushrooms;
using AQMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AQMod.Tiles.Nature
{
    public class NobleMushrooms : ModTile
    {
        public static readonly int[] generationTiles = new int[]
        {
            TileID.Sand,
            TileID.Stone,
            TileID.Ebonstone,
            TileID.Crimstone,
            TileID.Pearlstone,
            TileID.BlueMoss,
            TileID.BrownMoss,
            TileID.GreenMoss,
            TileID.LavaMoss,
            TileID.PurpleMoss,
            TileID.RedMoss,
        };

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18, };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleWrapLimit = 3;
            TileObjectData.addTile(Type);
            var name = CreateMapEntryName("NobleMushrooms");
            AddMapEntry(new Color(208, 0, 126), name);
            AddMapEntry(new Color(144, 254, 2), name);
            AddMapEntry(new Color(0, 197, 208), name);
            disableSmartCursor = true;
        }

        public override bool KillSound(int i, int j)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                AQSound.LegacyPlay(SoundType.Custom, AQSound.Paths.NobleMushroomHit, new Vector2(i * 16, j * 16f), 0.5f, Main.rand.NextFloat(0.9f, 1.1f));
            }
            return false;
        }

        public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].frameX / 36 % 3);

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            switch (Main.tile[i, j].frameX / 36)
            {
                default:
                {
                    r = 0.52f;
                    g = 0f;
                    b = 0.31f;
                }
                break;

                case 1:
                {
                    r = 0.36f;
                    g = 0.7f;
                    b = 0f;
                }
                break;

                case 2:
                {
                    r = 0f;
                    g = 0.49f;
                    b = 0.52f;
                }
                break;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            switch (Main.tile[i, j].frameX / 36)
            {
                default:
                {
                    type = ModContent.DustType<ArgonDust>();
                }
                break;

                case 1:
                {
                    type = ModContent.DustType<KryptonDust>();
                }
                break;

                case 2:
                {
                    type = ModContent.DustType<XenonDust>();
                }
                break;
            }
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                AQSound.LegacyPlay(SoundType.Custom, AQSound.Paths.NobleMushroomDestroy, new Vector2(i * 16, j * 16f), 0.5f, Main.rand.NextFloat(-0.1f, 0.1f));
            }
            switch (frameX / 36)
            {
                default:
                {
                    Item.NewItem(i * 16, j * 16, 32, 48, ModContent.ItemType<ArgonMushroom>());
                }
                break;

                case 1:
                {
                    Item.NewItem(i * 16, j * 16, 32, 48, ModContent.ItemType<KryptonMushroom>());
                }
                break;

                case 2:
                {
                    Item.NewItem(i * 16, j * 16, 32, 48, ModContent.ItemType<XenonMushroom>());
                }
                break;
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.gamePaused || !Main.instance.IsActive)
                return;
            if (Main.tile[i, j].frameY < 18)
            {
                int chance = 60 - AQSystem.NobleMushroomsCount * 2;
                if (chance < 0)
                    chance = 0;
                if (Main.rand.NextBool(chance + 8))
                {
                    switch (Main.tile[i, j].frameX / 36)
                    {
                        default:
                        {
                            Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 2, ModContent.DustType<ArgonMist>());
                        }
                        break;

                        case 1:
                        {
                            Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 2, ModContent.DustType<KryptonMist>());
                        }
                        break;

                        case 2:
                        {
                            Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 2, ModContent.DustType<XenonMist>());
                        }
                        break;
                    }
                }
            }
        }
    }
}