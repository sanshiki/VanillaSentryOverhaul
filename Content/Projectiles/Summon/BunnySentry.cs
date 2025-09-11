using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class BunnySentry : ModProjectile
    {
        private int shootTimer;

        private const int NORMAL_FRAME_SPEED = 20;
        private const int SHOOT_FRAME_SPEED = 5;

        private const int SHOOT_INTERVAL = 50;
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;

        public static float Gravity = ModGlobal.SENTRY_GRAVITY;
        public static float MaxGravity = 20f;

        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/BunnySentry";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 26;
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
                    true, 
                    null).TargetNPC;

            if (target != null)
            {
                shootTimer++;
                if (shootTimer >= 1 && shootTimer <= 4)
                {
                    // Shooting animation
                    Projectile.frame = 1; // Frame 3
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
                    // Fire!
                    Vector2 direction = target.Center - Projectile.Center;
                    float distance = direction.Length();
                    direction.Normalize();
                    direction *= 10f; // Bullet speed
                    direction.Y -= distance * 0.002f;

                    Vector2 bulletOffset = new Vector2(-18f * Projectile.spriteDirection, 7f);

                    Projectile seed = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + bulletOffset,
                        direction,
                        ProjectileID.Seed,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    seed.DamageType = DamageClass.Summon;
                    // Main.NewText("Damage: " + Projectile.damage + "Seed Damage: " + seed.damage);

                    shootTimer = 0; // Reset shoot animation
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
        //         if (npc.CanBeChasedBy(this) && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
        //         {
        //             float distance = Vector2.Distance(Projectile.Center, npc.Center);
        //             if (distance < closestDist)
        //             {
        //                 closestDist = distance;
        //                 closest = npc;
        //             }
        //         }
        //     }

        //     return closest;
        // }

        private void UpdateAnimation(NPC target, int shootTimer)
        {
            // Projectile.frameCounter++;
            if (target != null)
            {
                // if (shootTimer < SHOOT_FRAME_SPEED)
                // {
                //     Projectile.frame = 1;
                // }
                // else
                // {
                //     Projectile.frame = 0;
                // }
                // Projectile.frameCounter = 0;
                // face towards target
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                Projectile.spriteDirection = direction.X > 0 ? -1 : 1;
            }
            // else
            // {
            //     if (Projectile.frameCounter > NORMAL_FRAME_SPEED)
            //     {
            //         Projectile.frameCounter = 0;
            //         Projectile.frame = Projectile.frame == 0 ? 1 : 0;
            //     }
            // }
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

        public override bool MinionContactDamage()
		{
			return false;
		}


    }
}