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
    
    public class OneTrueFlag : FlagWeapon<OneTrueFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/OneTrueFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.OneTrueFlagProjectile;
        protected override int POLE_LENGTH => 280;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 180;
            Item.knockBack = 1;
        }

    }
}
