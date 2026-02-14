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
    
    public class HellFlag : FlagWeapon<HellFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/HellFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.HellFlagProjectile;
        protected override int POLE_LENGTH => 240;
        protected override int RAISE_USE_TIME => 50;
        protected override int WAVE_USE_TIME => 25+4;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 28;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(silver: 54);
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 18);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

    }
}
