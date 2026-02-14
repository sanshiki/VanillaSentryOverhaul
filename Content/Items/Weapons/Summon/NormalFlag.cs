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
    
    public class NormalFlag : FlagWeapon<NormalFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/NormalFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.NormalFlagProjectile;
        protected override int POLE_LENGTH => 200;
        protected override int WAVE_USE_TIME => 25+5;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 10;
            Item.knockBack = 1;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 5);
            recipe.AddIngredient(ItemID.Wood, 15);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }

    }
}
