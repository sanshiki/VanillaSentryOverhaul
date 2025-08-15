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
    public class AntlionSentry : ModProjectile
    {
        private int shootTimer;

        private bool isShooting = false;
        private bool isOnSand = false;

        private const int NORMAL_FRAME_SPEED = 10;
        private const int SHOOT_FRAME_SPEED = 5;

        private const int SHOOT_INTERVAL = 120;
        private const int SHOOT_INTERVAL_FAST = 90;
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;
        private int shootInterval = SHOOT_INTERVAL;

        public static float Gravity = 0.5f;
        public static float MaxGravity = 20f;
        private const float BULLET_SPEED_Y = 25f;
        private const float BULLET_GRAVITY = 1.0f;
        private const float MAX_LEGAL_HEIGHT = BULLET_SPEED_Y * BULLET_SPEED_Y / (2 * BULLET_GRAVITY)*0.8f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 11;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 55;
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
            
            // apply gravity
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxGravity)
            {
                Projectile.velocity.Y = MaxGravity;
            }

            // Targeting
            NPC target = MinionAIHelper.SearchForTargets(
                owner, 
                Projectile, 
                600f, 
                false, 
                n => n.Center.Y < Projectile.Center.Y + MAX_LEGAL_HEIGHT).TargetNPC;

            if (target != null)
            {
                shootTimer++;

                shootInterval = isOnSand ? SHOOT_INTERVAL_FAST : SHOOT_INTERVAL;

                if(owner.HasBuff(BUFF_ID))
                {
                    shootInterval = (int)(shootInterval * ENHANCEMENT_FACTOR);
                }


                if (shootTimer >= shootInterval)
                {
                    // Fire!
                    Vector2 ShootOffset = new Vector2(20f, -15f);
                    Vector2 ShootCenter = Projectile.Center + ShootOffset;
                    Vector2 direction = target.Center - ShootCenter;
                    Vector2 dir_compensation = target.velocity;
                    dir_compensation *= 40f;
                    if(dir_compensation.Length() > 200f)
                    {
                        dir_compensation.Normalize();
                        dir_compensation *= 200f;
                    }
                    // Main.NewText("dir_compensation:"+ dir_compensation.X.ToString() + " " + dir_compensation.Y.ToString());
                    direction += dir_compensation;
                    float vy = BULLET_SPEED_Y;
                    float bullet_gravity = BULLET_GRAVITY;
                    float max_vx = 8f;
                    float delta = Math.Max(0, vy * vy + 2 * bullet_gravity * direction.Y);
                    float pred_t1 = (vy + (float)Math.Sqrt(delta)) / bullet_gravity;
                    float pred_t2 = (vy - (float)Math.Sqrt(delta)) / bullet_gravity;
                    float pred_t = Math.Max(pred_t1, pred_t2);
                    float vx = direction.X / pred_t;

                    Main.NewText("pred_t:"+ pred_t.ToString() + " vx:" + vx.ToString() + "isOnSand:" + isOnSand.ToString());    

                    if(vx > max_vx)
                    {
                        vx = max_vx;
                    }
                    if(vx < -max_vx)
                    {
                        vx = -max_vx;
                    }

                    

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        ShootCenter,
                        new Vector2(vx, -vy),
                        ModProjectileID.AntlionSentryBullet,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    shootTimer = 0; // Reset shoot animation
                    isOnSand = false;


                    SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
                }
            }
            else
            {
                shootTimer = 0; // Reset if no target
            }

            // Animation
            UpdateAnimation(target, shootTimer);
        }

        // private NPC FindTarget(float range)
        // {
        //     NPC closest = null;
        //     float closestDist = range;

        //     for (int i = 0; i < Main.maxNPCs; i++)
        //     {
        //         NPC npc = Main.npc[i];
        //         if (npc.CanBeChasedBy(this)/*  && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) */)
        //         {
        //             float distance = Vector2.Distance(Projectile.Center, npc.Center);
        //             // check if the target is legal
        //             float MaxHeight = BULLET_SPEED_Y * BULLET_SPEED_Y / (2 * BULLET_GRAVITY)*0.8f;
        //             if(npc.Center.Y > Projectile.Center.Y + MaxHeight)
        //             {
        //                 continue;
        //             }
        //             if (distance < closestDist)
        //             {
        //                 closestDist = distance;
        //                 closest = npc;
        //             }
        //         }
        //     }

        //     // check if the target is legal
        //     // float MaxHeight = BULLET_SPEED_Y * BULLET_SPEED_Y / (2 * Gravity);
        //     // if(closest.Center.Y > Projectile.Center.Y + MaxHeight)
        //     // {
        //     //     return null;
        //     // }

        //     return closest;
        // }

        private void UpdateAnimation(NPC target, int shootTimer)
        {
            Projectile.frameCounter++;
            if (target != null)
            {
                if (shootTimer == shootInterval - (int)(SHOOT_FRAME_SPEED * 1.5f))
                {
                    isShooting = true;
                    Projectile.frameCounter = 0;
                }
            }

            if (isShooting)
            {
                if (Projectile.frameCounter > SHOOT_FRAME_SPEED)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= 11)
                    {
                        isShooting = false;
                        Projectile.frame = 0;
                    }
                }
            }
            else
            {
                if (Projectile.frameCounter > NORMAL_FRAME_SPEED)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = Projectile.frame == 0 ? 1 : 0;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // if the sentry is on sand, set the shoot interval to fast
            int tileX = (int)(Projectile.Center.X / 16f)-1;
            int tileY = (int)(Projectile.Bottom.Y / 16f);
            bool onSand = false;
            for(int i=0;i<4;i++)
            {
                for(int j=0;j<2;j++)
                {
                    Tile tileBelow = Framing.GetTileSafely(tileX+i, tileY+j);
                    if(tileBelow.HasTile && tileBelow.TileType == TileID.Sand)
                    {
                        onSand = true;
                        break;
                    }
                }
            }
            if(onSand)
            {
                isOnSand = true;
            }
            else
            {
                isOnSand = false;
            }

            // set velocity to 0
            Projectile.velocity = Vector2.Zero;

            return false;
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