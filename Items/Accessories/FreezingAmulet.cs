﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AQMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class FreezingAmulet : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.accessory = true;
            item.defense = 2;
            item.rare = AQItem.RarityGaleStreams;
            item.value = AQItem.Prices.GaleStreamsWeaponValue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<AQPlayer>().hotAmulet = true;
            player.fireWalk = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[ModContent.BuffType<Buffs.Debuffs.BlueFire>()] = true;
            player.buffImmune[ModContent.BuffType<Buffs.Debuffs.Sparkling>()] = true;
            player.buffImmune[ModContent.BuffType<Buffs.Debuffs.CorruptionHellfire>()] = true;
            player.buffImmune[ModContent.BuffType<Buffs.Debuffs.CrimsonHellfire>()] = true;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ModContent.ItemType<Amulet>());
            r.AddIngredient(ModContent.ItemType<DegenerationRing>());
            r.AddIngredient(ModContent.ItemType<Materials.Energies.AtmosphericEnergy>());
            r.AddIngredient(ModContent.ItemType<Materials.SiphonTentacle>(), 20);
            r.AddIngredient(ItemID.SoulofFlight, 8);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}