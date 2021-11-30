﻿using AQMod.Common.PlayerData.Layers;
using Terraria.ModLoader;

namespace AQMod.Assets.Graphics.PlayerLayers
{
    public static class PlayerLayersCache
    {
        public static readonly PlayerLayer postDrawHead = new PostDrawHead().GetLayer();
        public static readonly PlayerLayer postDrawBody = new PostDrawBody().GetLayer();
        public static readonly PlayerLayer preDraw = new PreDraw().GetLayer();
        public static readonly PlayerLayer postDraw = new PostDraw().GetLayer();
        public static readonly PlayerLayer postDrawWings = new PostDrawWings().GetLayer();

        public static readonly PlayerHeadLayer postDrawHeadHead = new HeadLayerPostDraw().GetLayer();
    }
}