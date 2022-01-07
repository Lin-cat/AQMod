﻿using AQMod.NPCs.Monsters.DemonSiege;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace AQMod.Effects
{
    public static class CustomRenderTrapperChains
    {
        internal static void RenderTrapperChains()
        {
            int trapper = ModContent.NPCType<Trapper>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == trapper)
                {
                    var chainTexture = ModContent.GetTexture(AQUtils.GetPath<Trapper>("_Chain"));
                    int npcOwner = (int)Main.npc[i].ai[1] - 1;
                    int height = chainTexture.Height - 2;
                    var npcCenter = Main.npc[i].Center;
                    var trapImpCenter = Main.npc[npcOwner].Center;
                    var velocity = npcCenter - trapImpCenter;
                    int length = (int)(velocity.Length() / height);
                    velocity.Normalize();
                    velocity *= height;
                    float rotation = velocity.ToRotation() + MathHelper.PiOver2;
                    var origin = new Vector2(chainTexture.Width / 2f, chainTexture.Height / 2f);
                    for (int j = 1; j < length; j++)
                    {
                        var position = trapImpCenter + velocity * j;
                        var color = Lighting.GetColor((int)(position.X / 16), (int)(position.Y / 16f));
                        if (j < 6)
                            color *= 1f / 6f * j;
                        Main.spriteBatch.Draw(chainTexture, position - Main.screenPosition, null, color, rotation, origin, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
        }
    }
}