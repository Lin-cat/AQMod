﻿using AQMod.Common.Utilities;
using AQMod.Content.WorldEvents.GlimmerEvent;
using AQMod.Items.Materials.Energies;
using AQMod.Localization;
using AQMod.NPCs.Boss.Starite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.BossSummons
{
    public class MythicStarfruit : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[item.type] = (int)BossSpawnItemSortOrder.WormFood;
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = ItemRarityID.Green;
            item.useAnimation = 45;
            item.useTime = 45;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.dayTime && !AQMod.glimmerEvent.IsActive && !NPC.AnyNPCs(ModContent.NPCType<OmegaStarite>());
        }

        public override bool UseItem(Player player)
        {
            AQMod.glimmerEvent.Activate(resetSpawnChance: false);
            AQMod.BroadcastMessage(AQText.Key + "Common.GlimmerEventWarning", GlimmerEvent.TextColor);
            Main.PlaySound(SoundID.Roar, player.position, 0);
            return true;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<CosmicEnergy>());
            r.AddIngredient(ItemID.DemoniteBar, 5);
            r.AddIngredient(ItemID.FallenStar);
            r.AddTile(TileID.DemonAltar);
            r.SetResult(this);
            r.AddRecipe();
            r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<CosmicEnergy>());
            r.AddIngredient(ItemID.CrimtaneBar, 5);
            r.AddIngredient(ItemID.FallenStar);
            r.AddTile(TileID.DemonAltar);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}