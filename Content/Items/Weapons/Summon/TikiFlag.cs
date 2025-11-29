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
    
    public class TikiFlag : FlagWeapon<TikiFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/TikiFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.TikiFlagProjectile;
        protected override int POLE_LENGTH => 280;
        protected override int RAISE_USE_TIME => 40;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 100;
            Item.knockBack = 1;
        }

    }
}
