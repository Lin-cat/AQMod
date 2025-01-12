﻿using Terraria;
using Terraria.ModLoader;

namespace AQMod.Buffs
{
    public class PrimeTime : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 50;
            player.endurance += 0.5f;
            player.GetModPlayer<AQPlayer>().setArachnotronCooldown = true;
        }
    }
}