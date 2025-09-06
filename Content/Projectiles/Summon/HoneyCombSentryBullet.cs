using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class HoneyCombSentryBullet : ModProjectile
    {

        private const int FRAME_SPEED = 10;
        private const int FRAME_NUM = 4;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.Bee];
            ProjectileID.Sets.TrailingMode[Projectile.type] = ProjectileID.Sets.TrailingMode[ProjectileID.Bee];
            Main.projFrames[Projectile.type] = FRAME_NUM;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Bee);

            // 关键：改成召唤伤害
            Projectile.DamageType = DamageClass.Summon;

            // 保留原版蜜蜂的免疫设置
            // Projectile.usesLocalNPCImmunity = true;
            // Projectile.localNPCHitCooldown = 10;
        }


        public override void AI()
        {
            Projectile.frameCounter++;
            if(Projectile.frameCounter >= FRAME_SPEED)
            {
                Projectile.frame = (Projectile.frame + 1) % FRAME_NUM;
                Projectile.frameCounter = 0;
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 150, Projectile.velocity.X, Projectile.velocity.Y, 50);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].scale = 1f;
			}
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X) {
                Projectile.velocity.X = -oldVelocity.X; // 水平方向反弹
            }
            if (Projectile.velocity.Y != oldVelocity.Y) {
                Projectile.velocity.Y = -oldVelocity.Y; // 垂直方向反弹
            }
            Projectile.penetrate--;
            return false;
        }

        // // 关键：直接引用原版火枪子弹贴图
        // public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        // public override void SetDefaults()
        // {
        //     Projectile.width = 8;
        //     Projectile.height = 8;

        //     Projectile.aiStyle = 36;
        //     AIType = ProjectileID.Bee;

        //     Projectile.friendly = true;
        //     Projectile.hostile = false;
        //     Projectile.penetrate = 1;
        //     Projectile.timeLeft = 600;

        //     // Projectile.ignoreWater = true;
        //     // Projectile.tileCollide = true;

        //     // 关键：召唤伤害
        //     Projectile.DamageType = DamageClass.Summon;

        //     // Projectile.usesLocalNPCImmunity = true;
        //     // Projectile.localNPCHitCooldown = 4;
        // }
    }
}
