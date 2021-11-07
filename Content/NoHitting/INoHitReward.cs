﻿using Terraria;

namespace AQMod.Content.NoHitting
{
    public interface INoHitReward
    {
        bool OnEffect(NPC npc, int hitDirection, double damage, NoHitManager noHitManager);
        void NPCLoot(NPC npc, NoHitManager noHitManager);
    }
}