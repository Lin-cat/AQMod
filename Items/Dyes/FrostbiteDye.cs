﻿using AQMod.Effects.Dyes;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace AQMod.Items.Dyes
{
    public class FrostbiteDye : DyeItem
    {
        public override string Pass => "HoriztonalWavePass";
        public override int Rarity => AQItem.Rarities.GaleStreamsRare - 1;

        public override ArmorShaderData CreateShaderData()
        {
            return new ArmorShaderDataModifyLightColor(Effect, Pass, (v) =>
            {
                return v * new Vector3(0.05f, 0.2f, 0.9f);
            }).UseColor(new Vector3(0.05f, 0.2f, 0.9f)).UseSecondaryColor(new Vector3(0.1f, 0.4f, 2f));
        }
    }
}