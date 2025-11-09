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
    
    public class SantaFlag : FlagWeapon<SantaFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/SantaFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.SantaFlagProjectile;
        protected override int POLE_LENGTH => 280;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 165;
            Item.knockBack = 1;
        }

    }
}
