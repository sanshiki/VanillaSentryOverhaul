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

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 19;
            Item.knockBack = 1;
        }

    }
}
