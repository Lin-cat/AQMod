﻿using AQMod.Common;
using AQMod.Content.World.Events.GlimmerEvent;
using AQMod.Items.Materials.Energies;
using AQMod.Localization;
using AQMod.NPCs.Boss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.BossItems
{
    public class MythicStarfruit : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[item.type] = Constants.BossSpawnItemSortOrder.WormFood;
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
            item.maxStack = 999;
            item.value = Item.buyPrice(gold: 10);
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.dayTime && !GlimmerEvent.IsGlimmerEventCurrentlyActive() && !NPC.AnyNPCs(ModContent.NPCType<OmegaStarite>());
        }

        public override bool UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                GlimmerEvent.Activate();
                NetHelper.ActivateGlimmerEvent();
            }
            AQMod.BroadcastMessage(AQText.Key + "Common.GlimmerEventWarning", GlimmerEvent.TextColor);
            Main.PlaySound(SoundID.Roar, player.position, 0);
            return true;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddRecipeGroup(AQRecipes.RecipeGroups.DemoniteBarOrCrimtaneBar, 5);
            r.AddIngredient(ItemID.FallenStar);
            r.AddTile(TileID.DemonAltar);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}