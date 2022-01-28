﻿using Terraria;
using Terraria.ModLoader;

namespace AQMod.Buffs.Debuffs
{
    public class UmystickDelay : ModBuff
    {
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<AQPlayer>().cantUseMenaceUmbrellaJump = true;
            player.manaRegen = 0;
            player.manaCost += 1f;
            player.manaRegenDelay = (int)player.maxRegenDelay;
        }
    }
}
