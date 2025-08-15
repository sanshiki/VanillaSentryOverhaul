using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Utils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class MachineGunSentry : ModProjectile
    {
        // timers
        private int shootTimer;
        private int spawnTimer;

        // sentry state
        private bool onGround;
        private const bool USE_PREDICTION = true;
        private Vector2 LastVelocity;

        // sentry parameters
        private const float REAL_BULLET_SPEED = 20f;
        private const float PRED_BULLET_SPEED = 20f;
        private const float ACC_FACTOR = 0.05f;
        private const int FRAME_COUNT = 36;
        private const float MAX_RANGE = 1500f;

        private const int SHOOT_INTERVAL = 10;
        private const int SPAWN_TIME = 2*26;

        // buff constants
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;

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
            Projectile.DamageType = DamageClass.Summon;
            
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
            // NPC target = FindTarget(1500f); // Range: 1500 pixels
            NPC target = MinionAIHelper.SearchForTargets(
                owner, 
                Projectile, 
                MAX_RANGE, 
                true, 
                null).TargetNPC;

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

                int shootInterval = SHOOT_INTERVAL;
                if(owner.HasBuff(BUFF_ID))
                {
                    shootInterval = (int)(shootInterval * ENHANCEMENT_FACTOR);
                }

                if (shootTimer >= shootInterval)
                {
                    // calculate prediction
                    Vector2 PredictedPos = target.Center;
                    Vector2 Acc = (target.velocity - LastVelocity) * ACC_FACTOR;
                    LastVelocity = target.velocity;

                    if(USE_PREDICTION)
                    {
                        for(int tick = 0; tick < 60; tick+=3)
                        {
                            Vector2 TargetPredictedPos = target.Center + target.velocity * tick + 0.5f * Acc * tick * tick;
                            PredictedPos = TargetPredictedPos;

                            float bulletFlyTime = Vector2.Distance(Projectile.Center, TargetPredictedPos) / PRED_BULLET_SPEED;

                            if (bulletFlyTime < tick)
                            {
                                // Main.NewText("bulletFlyTime: " + bulletFlyTime + " tick: " + tick);
                                break;
                            }
                        }
                    }

                    // Fire!
                    direction = PredictedPos - Projectile.Center;
                    direction.Normalize();
                    Vector2 bulletVelocity = direction * REAL_BULLET_SPEED;

                    Vector2 bulletOffset = new Vector2(15f, -5f) + direction * 27f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + bulletOffset,
                        bulletVelocity,
                        ModProjectileID.MachineGunSentryBullet,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    shootTimer = 0; // Reset shoot animation

                    SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
                }
            }
            else
            {
                shootTimer = 0; // Reset if no target
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
                    if (frame >= 24)
                    {
                        frame = 24;
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