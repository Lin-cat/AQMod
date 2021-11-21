﻿using AQMod.NPCs.Boss.Starite;
using Terraria;

namespace AQMod.Content.NoHitting
{
    public class NoHitOmegaStarite : INoHitReward
    {
        public readonly int Type;
        public readonly int Stack;

        public NoHitOmegaStarite(int type)
        {
            Type = type;
            Stack = 1;
        }

        public NoHitOmegaStarite(int type, int stack)
        {
            Type = type;
            Stack = stack;
        }

        public void NPCLoot(NPC npc, NoHitNPC noHitManager)
        {
            Item.NewItem(npc.getRect(), Type, Stack);
        }

        public bool OnEffect(NPC npc, int hitDirection, double damage, NoHitNPC noHitManager)
        {
            return (npc.modNPC as OmegaStarite).skipDeathTimer > 0 || npc.ai[0] == OmegaStarite.PHASE_DEAD;
        }
    }
}