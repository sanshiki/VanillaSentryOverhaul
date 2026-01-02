using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class SandustSentrySandnadoFriendly : ModProjectile
    {
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.SandnadoFriendly;

        Projectile vanillaSandnado;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Projectile.type] = true;
            ProjectileID.Sets.SummonTagDamageMultiplier[Projectile.type] = 0.5f;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SandnadoFriendly);
            Projectile.height = 28*16;
            Projectile.width = 3*16;
            Projectile.aiStyle = 0;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3*60;
            Projectile.DamageType = DamageClass.Summon;
			Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ArmorPenetration = 20;
            // Projectile.usesIDStaticNPCImmunity = true;
            // Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // genereate a vanilla sandnado only for visual effect
            vanillaSandnado = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileID.SandnadoFriendly, 0, 0, Projectile.owner);
            vanillaSandnado.timeLeft = 3*60;
            vanillaSandnado.friendly = false;
            vanillaSandnado.hostile = false;
        }

        public override void Kill(int timeLeft)
        {
            vanillaSandnado.Kill();
        }
        
    }
}