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
    public class RocketSentry : ModProjectile
    {
        // timers
        private int shootTimer;
        private int spawnTimer;

        // sentry state
        private bool onGround;
        private const bool USE_PREDICTION = true;
        private int rocketCount = 0;

        // sentry parameters
        private const float REAL_BULLET_SPEED = 15f;
        private const float PRED_BULLET_SPEED = 22f;

        // buff constants
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;

        // animation frame parameters
        private const int FRAME_COUNT = 37;
        private const int NORMAL_FRAME_SPEED = 20;
        private const int SHOOT_FRAME_SPEED = 5;

        // private const int SHOOT_INTERVAL = 6;
        private const int TOTAL_SHOOT_INTERVAL = 60;
        private const int INTERVAL_BETWEEN_ROCKETS = 20;
        private const int ROCKET_NUM = 2;
        private const int SPAWN_TIME = 2*26;

        // gravity
        public static float Gravity = 0.5f;
        public static float MaxGravity = 20f;

        // direction
        private Vector2 direction = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = FRAME_COUNT;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 53;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.sentry = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            
            BUFF_ID = ModBuffID.SentryEnhancement;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // if on ground, spawnTimer++
            if(onGround)
            {
                spawnTimer++;
            }

            // apply gravity
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxGravity)
            {
                Projectile.velocity.Y = MaxGravity;
            }

            // Targeting
            NPC target = FindTarget(1500f); // Range: 1500 pixels

            if (target != null && spawnTimer > SPAWN_TIME)
            {
                shootTimer++;
                if (shootTimer == 1)
                {
                    // Shooting animation
                    Projectile.frame = 2; // Frame 3
                }
                else
                {
                    Projectile.frame = 0;
                }

                // calculate direction
                Vector2 PredictedPos = target.Center;

                if(USE_PREDICTION)
                {
                    for(int tick = 0; tick < 60; tick+=3)
                    {
                        Vector2 TargetPredictedPos = target.Center + target.velocity * tick;
                        PredictedPos = TargetPredictedPos;

                        float bulletFlyTime = Vector2.Distance(Projectile.Center, TargetPredictedPos) / PRED_BULLET_SPEED;

                        if (bulletFlyTime < tick)
                        {
                            // Main.NewText("bulletFlyTime: " + bulletFlyTime + " tick: " + tick);
                            break;
                        }

                    }
                }
                direction = PredictedPos - Projectile.Center;
                direction.Normalize();

                if ((shootTimer >= INTERVAL_BETWEEN_ROCKETS && rocketCount == 0) ||
                    (shootTimer >= INTERVAL_BETWEEN_ROCKETS*2 && rocketCount == 1))
                {
                    

                    // Fire!
                    Vector2 bulletVelocity = direction * REAL_BULLET_SPEED;

                    Vector2 bulletOffset = new Vector2(15f, -5f) + direction * 27f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + bulletOffset,
                        bulletVelocity,
                        ProjectileID.RocketI,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    rocketCount++;

                    SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
                }

                int shootInterval = TOTAL_SHOOT_INTERVAL;
                if(owner.HasBuff(BUFF_ID))
                {
                    shootInterval = (int)(shootInterval * ENHANCEMENT_FACTOR);
                }

                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0;
                    rocketCount = 0;
                }
            }
            else
            {
                shootTimer = 0; // Reset if no target
                rocketCount = 0;
            }

            // Animation
            UpdateAnimation(target);
        }

        private NPC FindTarget(float range)
        {
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this) && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        private void UpdateAnimation(NPC target)
        {
            Projectile.frameCounter++;
            
            if(spawnTimer < SPAWN_TIME)
            {
                Projectile.frame = FRAME_COUNT - spawnTimer / 2 - 1;
            }
            else if (target != null)
            {
                // face towards target
                float angle = (float)Math.Atan2(direction.Y, Math.Abs(direction.X));    // -PI/2 to PI/2
                int frame = 0;

                float step = (float)(Math.PI / 36f);

                if (Math.Abs(angle) < step)
                {
                    frame = 0;
                }
                else if (angle > step)
                {
                    frame = (int)(Math.Abs(angle) / step);
                    if (frame > 8)
                    {
                        frame = 8;
                    }
                }
                else if (angle < -step)
                {
                    frame = (int)(Math.Abs(angle) / step) + 9;
                    if (frame >= 25)
                    {
                        frame = 25;
                    }
                }
                Projectile.frame = frame;

                // face towards target
                Projectile.spriteDirection = direction.X > 0 ? 1 : -1;
            }

            // Main.NewText("frame: " + Projectile.frame + " spawnTimer: " + spawnTimer);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            onGround = true;
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public void SetAttached(bool attached)
        {
            onGround = attached;
            // Projectile.tileCollide = !attached;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool MinionContactDamage()
		{
			return false;
		}
    }
}