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
    public class SandustSentry : ModProjectile
    {
        private int shootTimer;

        private bool isShooting = false;
        private bool isOnSand = false;

        private const int NORMAL_FRAME_SPEED = 15;
        private const int SHOOT_FRAME_SPEED = 5;

        private const int SHOOT_INTERVAL = 120;
        private const int SHOOT_INTERVAL_FAST = 90;
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;
        private int shootInterval = SHOOT_INTERVAL;

        public static float Gravity = ModGlobal.SENTRY_GRAVITY;
        public static float MaxGravity = 20f;
        private const float BULLET_SPEED_Y = 55f;
        private const float BULLET_SPEED_X = 20f;
        private const float BULLET_SPEED = 60f;
        private const float BULLET_GRAVITY = 2.0f;
        private const float MAX_LEGAL_HEIGHT = BULLET_SPEED_Y * BULLET_SPEED_Y / (2 * BULLET_GRAVITY)*0.8f;
        private const int FRAME_COUNT = 4;


        public override string Texture => "SummonerExpansionMod/Assets/Textures/Projectiles/SandustSentry";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = FRAME_COUNT;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 104;
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
                1000f, 
                false, 
                n => n.Center.Y < Projectile.Center.Y + MAX_LEGAL_HEIGHT).TargetNPC;

            if (target != null)
            {
                shootInterval = isOnSand ? SHOOT_INTERVAL_FAST : SHOOT_INTERVAL;

                // if(owner.HasBuff(BUFF_ID))
                // {
                //     shootInterval = (int)(shootInterval * ENHANCEMENT_FACTOR);
                // }


                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0;
                }
                if (shootTimer == 0)
                {
                    // Fire!
                    Vector2 ShootOffset = new Vector2(0f, -15f);
                    Vector2 ShootCenter = Projectile.Center + ShootOffset;
                    
                    Vector2 velocity = MinionAIHelper.PredictVelocityWithGravity(Projectile.Center, target.Center, target.velocity, BULLET_SPEED_Y, BULLET_GRAVITY, 60, 1);

                    // Main.NewText("pred_t:"+ pred_t.ToString() + " vx:" + vx.ToString() + "isOnSand:" + isOnSand.ToString());    

                    if(velocity.X > BULLET_SPEED_X)
                    {
                        velocity.X = BULLET_SPEED_X;
                    }
                    if(velocity.X < -BULLET_SPEED_X)
                    {
                        velocity.X = -BULLET_SPEED_X;
                    }

                    Projectile proj = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        ShootCenter,
                        velocity,
                        ModProjectileID.SandustSentryBullet,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    ProjectileID.Sets.SentryShot[proj.type] = true;

                    shootTimer = 0; // Reset shoot animation
                    isOnSand = false;


                    SoundEngine.PlaySound(SoundID.Item95, Projectile.position);
                }
            }

            shootTimer++;
            if(shootTimer >= shootInterval)
                shootTimer = shootInterval;

            // Animation
            UpdateAnimation(target, shootTimer);
        }

        private void UpdateAnimation(NPC target, int shootTimer)
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > NORMAL_FRAME_SPEED)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FRAME_COUNT)
                {
                    Projectile.frame = 0;
                }
            }

            if (Main.rand.NextFloat() < 0.1f)
            {
                Dust dust;
                int dustType = MinionAIHelper.RandomBool() ? 283 : 133;
                float offset = dustType == 283 ? Projectile.height/2f : Projectile.height/4f;
                Vector2 position = Projectile.position+new Vector2(0f, offset);
                dust = Main.dust[Terraria.Dust.NewDust(position, Projectile.width, 5, dustType, 0f, -4.6511626f, 0, new Color(255,255,255), 1f)];
            }


        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.X = 0f;

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