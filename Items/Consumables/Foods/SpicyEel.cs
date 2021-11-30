﻿using AQMod.Items.Materials.Fish;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Consumables.Foods
{
    public class SpicyEel : ModItem, ISpecialFood
    {
        public override void SetDefaults()
        {
            item.width = 10;
            item.height = 10;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.useAnimation = 15;
            item.useTime = 15;
            item.useTurn = true;
            item.UseSound = SoundID.Item2;
            item.maxStack = 999;
            item.consumable = true;
            item.rare = ItemRarityID.Green;
            item.value = Item.buyPrice(silver: 50);
            item.buffType = BuffID.WellFed;
            item.buffTime = 25200;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine t = tooltips[i];
                if (t.mod == "Terraria" && t.Name == "WellFedExpert")
                {
                    tooltips.RemoveAt(i);
                    break;
                }
            }
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<LarvaEel>());
            r.AddRecipeGroup(AQMod.AnyNobleMushroom);
            r.SetResult(this);
            r.AddRecipe();
            r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<UltraEel>());
            r.AddRecipeGroup(AQMod.AnyNobleMushroom);
            r.SetResult(this);
            r.AddRecipe();
        }

        int ISpecialFood.ChangeBuff(Player player)
        {
            return ModContent.BuffType<Buffs.Foods.SpicyEel>();
        }
    }
}