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
    
    public class GoblinFlag : FlagWeapon<GoblinFlagProjectile>
    {
        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Items/GoblinFlagItem";
        protected override int MOD_PROJECTILE_ID => ModProjectileID.GoblinFlagProjectile;
        protected override int POLE_LENGTH => 220;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.damage = 15;
            Item.knockBack = 1;
        }

    }
}
