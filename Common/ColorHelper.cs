﻿using AQMod.Common.Utilities.Colors;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;

namespace AQMod.Common
{
    public class ColorHelper
    {
        internal static Color BossMessage => new Color(175, 75, 255, 255);
        internal static Color EventMessage => new Color(50, 255, 130, 255);
        internal static Color Furniture => new Color(191, 142, 111);

        public readonly byte[] Paints;
        public readonly Dictionary<short, byte> DyeToPaint;
        public readonly Dictionary<byte, short> PaintToDye;
        public readonly Dictionary<byte, short> PaintToYoyoString;

        public IColorGradient AquaticGrad;
        public IColorGradient AtmosphericGrad;
        public IColorGradient CosmicGrad;
        public IColorGradient DemonicGrad;
        public IColorGradient OrganicGrad;
        public IColorGradient UltimateGrad;

        public ColorHelper()
        {
            AquaticGrad = new ColorWaveGradient(4f, new Color(111, 111, 190, 0), new Color(144, 144, 255, 0));
            AtmosphericGrad = new ColorWaveGradient(4f, new Color(200, 150, 10, 0) * 0.8f, new Color(255, 230, 70, 0) * 0.8f);
            CosmicGrad = new ColorWaveGradient(4f, new Color(90, 30, 200, 0), new Color(190, 120, 255, 0));
            DemonicGrad = new ColorWaveGradient(4f, new Color(222, 100, 10, 0) * 0.8f, new Color(255, 255, 120, 0) * 0.8f);
            OrganicGrad = new ColorWaveGradient(4f, new Color(120, 255, 60, 0), new Color(180, 250, 90, 0));
            UltimateGrad = new ColorWaveGradient(8f, new Color(150, 255, 255, 0), new Color(255, 150, 255, 0));

            Paints = new byte[]
            {
                PaintID.Red,
                PaintID.Orange,
                PaintID.Yellow,
                PaintID.Lime,
                PaintID.Green,
                PaintID.Teal,
                PaintID.Cyan,
                PaintID.SkyBlue,
                PaintID.Blue,
                PaintID.Purple,
                PaintID.Violet,
                PaintID.Pink,
                PaintID.Black,
                PaintID.White,
                PaintID.Brown,
                255,
            };
            DyeToPaint = new Dictionary<short, byte>
            {
                [ItemID.RedDye] = Paints[0],
                [ItemID.OrangeDye] = Paints[1],
                [ItemID.YellowDye] = Paints[2],
                [ItemID.LimeDye] = Paints[3],
                [ItemID.GreenDye] = Paints[4],
                [ItemID.TealDye] = Paints[5],
                [ItemID.CyanDye] = Paints[6],
                [ItemID.SkyBlueDye] = Paints[7],
                [ItemID.BlueDye] = Paints[8],
                [ItemID.PurpleDye] = Paints[9],
                [ItemID.VioletDye] = Paints[10],
                [ItemID.PinkDye] = Paints[11],
                [ItemID.BlackDye] = Paints[12],
                [ItemID.SilverDye] = Paints[13],
                [ItemID.BrownDye] = Paints[14],
                [ItemID.RainbowDye] = Paints[15],
            };
            PaintToDye = new Dictionary<byte, short>
            {
                [Paints[0]] = ItemID.RedDye,
                [Paints[1]] = ItemID.OrangeDye,
                [Paints[2]] = ItemID.YellowDye,
                [Paints[3]] = ItemID.LimeDye,
                [Paints[4]] = ItemID.GreenDye,
                [Paints[5]] = ItemID.TealDye,
                [Paints[6]] = ItemID.CyanDye,
                [Paints[7]] = ItemID.SkyBlueDye,
                [Paints[8]] = ItemID.BlueDye,
                [Paints[9]] = ItemID.PurpleDye,
                [Paints[10]] = ItemID.VioletDye,
                [Paints[11]] = ItemID.PinkDye,
                [Paints[12]] = ItemID.BlackDye,
                [Paints[13]] = ItemID.SilverDye,
                [Paints[14]] = ItemID.BrownDye,
                [Paints[15]] = ItemID.RainbowDye,
            };
            PaintToYoyoString = new Dictionary<byte, short>
            {
                [0] = ItemID.WhiteString,
                [Paints[0]] = ItemID.RedString,
                [Paints[1]] = ItemID.OrangeString,
                [Paints[2]] = ItemID.YellowString,
                [Paints[3]] = ItemID.LimeString,
                [Paints[4]] = ItemID.GreenString,
                [Paints[5]] = ItemID.TealString,
                [Paints[6]] = ItemID.CyanString,
                [Paints[7]] = ItemID.SkyBlueString,
                [Paints[8]] = ItemID.BlueString,
                [Paints[9]] = ItemID.PurpleString,
                [Paints[10]] = ItemID.VioletString,
                [Paints[11]] = ItemID.PinkString,
                [Paints[12]] = ItemID.BlackString,
                [Paints[13]] = ItemID.WhiteString,
                [Paints[14]] = ItemID.BrownString,
                [Paints[15]] = ItemID.RainbowString,
            };
        }
    }
}