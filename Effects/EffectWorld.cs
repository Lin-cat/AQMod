﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Effects
{
    internal sealed class EffectWorld : ModWorld
    {
        public override void Initialize()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Particle.PreDrawProjectiles.Initialize();
                Particle.PostDrawPlayers.Initialize();

                Trail.PreDrawProjectiles.Initialize();
            }
        }

        public override void PostUpdate()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Particle.PreDrawProjectiles.UpdateParticles();
                Particle.PostDrawPlayers.UpdateParticles();

                Trail.PreDrawProjectiles.UpdateTrails();
            }
        }
    }
}