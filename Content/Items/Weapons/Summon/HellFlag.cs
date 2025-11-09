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

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 29;
            Item.knockBack = 1;
        }

    }
}
