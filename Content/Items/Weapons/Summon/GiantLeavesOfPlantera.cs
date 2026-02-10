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
    
    public class GiantLeavesOfPlantera : FlagWeapon<GiantLeavesOfPlanteraProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/GiantLeavesOfPlanteraItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.GiantLeavesOfPlanteraProjectile;
        protected override int POLE_LENGTH => 260;
        protected override int RAISE_USE_TIME => 40;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 85;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.Lime;
        }

    }
}
