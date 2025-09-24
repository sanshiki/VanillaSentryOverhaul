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
    public class AutocannonSentryBullet : ModProjectile
    {
        // 关键：直接引用原版火枪子弹贴图
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.BulletHighVelocity;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
            // Projectile.ranged = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.aiStyle = 1;
        }

        // public override void AI()
        // {
        //     float num = Projectile.light;
        //     float num2 = Projectile.light;
        //     float num3 = Projectile.light;
        //     num2 *= 0.7f;
		// 	num3 *= 0.1f;
        //     Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), num, num2, num3);
        // }
    }
}
