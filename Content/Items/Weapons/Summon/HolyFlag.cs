using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Initialization;
namespace SummonerExpansionMod.Content.Items.Weapons.Summon
{
    
    public class HolyFlag : FlagWeapon<HolyFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/HolyFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.HolyFlagProjectile;
        protected override int POLE_LENGTH => 280;
        protected override int RAISE_USE_TIME => 42;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 55;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(gold: 4, silver: 60);
            Item.rare = ItemRarityID.Pink;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HallowedBar, 12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

    }
}
