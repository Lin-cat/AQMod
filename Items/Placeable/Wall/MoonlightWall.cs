﻿using AQMod.Tiles.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Placeable.Wall
{
    public class MoonlightWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.Glowmask(() => MoonlightWallWall.GetColor(0f));
        }

        public override void SetDefaults()
        {
            item.width = 12;
            item.height = 12;
            item.maxStack = 999;
            item.useTime = 7;
            item.useAnimation = 15;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.createWall = ModContent.WallType<MoonlightWallWall>();
            item.consumable = true;
            item.autoReuse = true;
            item.useTurn = true;
        }

        public override void CaughtFishStack(ref int stack)
        {
            stack = Main.rand.Next(80, stack + 120);
        }
    }

}