using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Back)]
    public class SuperEarthCloakLv1 : ModItem
    {
        public override void SetDefaults()
        {
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 5);
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<HD2SentryDmgReductionPlayer>().hasLv1Accessory = true;

            player.statDefense += 4;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.TissueSample, 5)
                .AddTile(TileID.Anvils)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.ShadowScale, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Back)]
    public class SuperEarthCloakLv2 : ModItem
    {
        public override void SetDefaults()
        {
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 5);
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<HD2SentryDmgReductionPlayer>().hasLv2Accessory = true;

            player.statDefense += 6;
            player.GetDamage(DamageClass.Summon) += 0.01f;
            player.maxTurrets += 1;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SuperEarthCloakLv1>(), 1)
                .AddIngredient(ItemID.MartianConduitPlating, 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class HD2SentryDmgReductionPlayer : ModPlayer
    {
        public bool hasLv1Accessory = false;
        public bool hasLv2Accessory = false;

        public override void ResetEffects() {
            hasLv1Accessory = false;
            hasLv2Accessory = false;
        }
    }
}