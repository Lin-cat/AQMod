﻿using AQMod.Items.Materials;
using AQMod.Localization;
using AQMod.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Boss.Summons
{
    public class MushroomClam : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[item.type] = ItemID.Sets.SortingPriorityBossSpawns[ItemID.SlimeCrown];
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = ItemRarityID.Blue;
            item.useAnimation = 45;
            item.useTime = 45;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.maxStack = 999;
        }

        public override bool CanUseItem(Player player)
        {
            return (player.ZoneBeach || player.Biomes().zoneCrabCrevice) && !NPC.AnyNPCs(ModContent.NPCType<JerryCrabson>());
        }

        public override bool UseItem(Player player)
        {
            NPC.NewNPC((int)player.position.X, (int)player.position.Y + 1000, ModContent.NPCType<JerryCrabson>(), 0, 0f, 0f, 0f, 0f, player.whoAmI);
            Main.NewText(AQText.ModText("Common.AwakenedCrabson"), CommonColors.BossMessage);
            Main.PlaySound(SoundID.Roar, player.position, 0);
            return true;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<CrabShell>(), 4);
            r.AddRecipeGroup(AQRecipes.RecipeGroups.AnyNobleMushroom);
            r.AddTile(TileID.DemonAltar);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}