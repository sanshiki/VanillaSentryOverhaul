using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using SummonerExpansionMod.Content.Buffs.Summon;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{

    public class TowerOfDryadsBlessing : ModProjectile
    {

        // leaf orbit parameters
        private const int LEAF_ORBIT_RADIUS_OUTER = 300;
        private const int LEAF_ORBIT_RADIUS_INNER = 100;
        private const int LEAF_NUM = 10;
        private const int LEAF_RESPAWN_INTERVAL = 40*17;
        private const int LIVE_TIME = 40*10;

        // buff constants
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;

        private int LeafTimer = LEAF_RESPAWN_INTERVAL - 40*3;

        public static float Gravity = 0.5f;
        public static float MaxGravity = 20f;

        Vector2 CenterOffset = new Vector2(10, -10);

        private List<int> LeafProjectileIndex = new List<int>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = false;
            Projectile.sentry = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            
            BUFF_ID = ModBuffID.SentryEnhancement;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // apply gravity
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxGravity)
            {
                Projectile.velocity.Y = MaxGravity;
            }


            if (LeafTimer <= LIVE_TIME)
            {

                foreach(int index in LeafProjectileIndex)
                {
                    Projectile proj = Main.projectile[index];
                    if(proj.ModProjectile is TowerOfDryadsBlessingProjectile leafProj)
                    {
                        leafProj.RotateCenter = Projectile.Center + CenterOffset;
                    }
                }

                Projectile leaf = Main.projectile[LeafProjectileIndex[0]];
                int radius = (int)Vector2.Distance(leaf.Center, Projectile.Center+CenterOffset);

                // 给范围内玩家加树妖祝福
                if (owner.active && !owner.dead && Vector2.Distance(owner.Center, Projectile.Center) < radius)
                {
                    owner.AddBuff(BuffID.DryadsWard, 30);
                }

                // 给范围内敌人加树妖祸害
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage &&
                        Vector2.Distance(npc.Center, Projectile.Center) < radius && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        npc.AddBuff(BuffID.DryadsWardDebuff, 30);
                    }
                }

                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DryadsWard, 0f, 0f);
                Dust dust = Main.dust[dustIndex];
                Random ran = new Random();
                float rate = ran.Next(100) / 100.0f;
                float velFactor = rate > 0.2 ? 0.2f : -0.6f;
                dust.velocity = (Projectile.Center + CenterOffset - dust.position) * velFactor;
                dust.noGravity = true;

            }

            // 持续生成环绕的叶子
            int leafRespawnInterval = LEAF_RESPAWN_INTERVAL;
            if(owner.HasBuff(BUFF_ID))
            {
                leafRespawnInterval = (int)(leafRespawnInterval * ENHANCEMENT_FACTOR);
            }

            if (LeafTimer >= leafRespawnInterval)
            {
                SpawnOrbitingLeaves();
                LeafTimer = 0;
            }
            LeafTimer++;
        }

        private void SpawnOrbitingLeaves()
        {
            // 先清理旧叶子，避免生成过多
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModProjectileID.TowerOfDryadsBlessingProjectile && proj.ai[0] == Projectile.whoAmI)
                {
                    proj.Kill();
                }
            }

            // 清理旧叶子索引
            LeafProjectileIndex.Clear();

            

            // 按当前旋转角度生成外环叶子
            for (int i = 0; i < LEAF_NUM; i++)
            {
                float angle = MathHelper.TwoPi / LEAF_NUM * i;
                Vector2 AngleVecTemp = new Vector2(0, angle);

                // 接口重映射
                int LeaftOuter = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + CenterOffset, // 投射物坐标->叶圆环中心
                    AngleVecTemp, // 投射物速度->叶片角度
                    ModProjectileID.TowerOfDryadsBlessingProjectile,
                    LEAF_ORBIT_RADIUS_OUTER, // 投射物伤害->叶圆环半径
                    2, // 投射物击退->叶片角速度
                    Projectile.owner
                );
                LeafProjectileIndex.Add(LeaftOuter);

                int LeafInner = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + CenterOffset, // 投射物坐标->叶圆环中心
                    AngleVecTemp, // 投射物速度->叶片角度
                    ModProjectileID.TowerOfDryadsBlessingProjectile,
                    LEAF_ORBIT_RADIUS_INNER, // 投射物伤害->叶圆环半径
                    -1, // 投射物击退->叶片角速度
                    Projectile.owner
                );
                LeafProjectileIndex.Add(LeafInner);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
    }
}
