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
    public class AntlionSentryBullet : ModProjectile
    {

        private const float GRAVITY = 1.0f;
        private const float MAX_GRAVITY = 30f;
        private const float EXPLOSION_RADIUS = 60f;

        public override void SetDefaults()
        {
            Projectile.width = 14; // 尺寸可以和沙块差不多
            Projectile.height = 14;
            Projectile.aiStyle = 0; // 自定义AI，不用原版沙块的aiStyle
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1; // 击中一次就消失
            Projectile.timeLeft = 600; // 存活时间
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            // 模拟重力：Y方向速度逐渐增加
            Projectile.velocity.Y += GRAVITY; // 比原版重力大（原版沙块大概 0.2f）

            // 限制最大下落速度，避免太快
            if (Projectile.velocity.Y > MAX_GRAVITY)
                Projectile.velocity.Y = MAX_GRAVITY;

            // 旋转，增加视觉效果
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            // 粒子效果（可选）
            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f);
            }
        }

        // 击中NPC时
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explosion(target);
        }

        // 撞击地面时
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explosion(null);
            return false; // Kill() 在 Explosion 内处理
        }

        private void Explosion(NPC target)
        {
            // 范围半径（像素）
            float radius = EXPLOSION_RADIUS; // 大约5格范围
            Vector2 center = Projectile.Center;

            // 伤害范围内的NPC
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(center) < radius && npc != target)
                {
                    // 计算伤害
                    int damage = (int)(Projectile.damage * 0.8f); // 爆炸伤害稍低
                    // Main.NewText("damage:" + damage.ToString());
                    NPC.HitInfo hitInfo = new NPC.HitInfo();
                    hitInfo.Crit = false;
                    hitInfo.DamageType = DamageClass.Summon;
                    hitInfo.HitDirection = 0;
                    hitInfo.Knockback = 0f;
                    hitInfo.Damage = damage;
                    npc.StrikeNPC(hitInfo, false, false);
                }
            }

            // 生成Dust特效
            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDustPerfect(center, DustID.Sand, dustVel).scale = 1.8f;
            }

            // 播放爆炸音效
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            Projectile.Kill();
        }

    }
}
