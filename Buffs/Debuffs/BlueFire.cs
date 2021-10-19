﻿using AQMod.Common;
using Terraria;
using Terraria.ModLoader;

namespace AQMod.Buffs.Debuffs
{
    public class BlueFire : ModBuff
    {
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            longerExpertDebuff = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<AQPlayer>().notFrostburn = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<AQNPC>().notFrostburn = true;
        }
    }
}