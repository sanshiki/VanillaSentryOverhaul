using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework.Graphics;

using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Utils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class HoneyCombSentry : ModProjectile
    {
        private int shootTimer;

        private const int NORMAL_FRAME_SPEED = 20;
        private const int SHOOT_FRAME_SPEED = 5;

        private const int SHOOT_INTERVAL = 60*2;
        private const int MAX_BEES_PER_SHOT = 10;
        private const int MIN_BEES_PER_SHOT = 5;
        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;

        public static float Gravity = ModGlobal.SENTRY_GRAVITY;
        public static float MaxGravity = 20f;

        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/HoneyCombSentry";
        public override string Texture => TEXTURE_PATH;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 43;
            Projectile.height = 31;
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
                    // Fire!
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    // direction *= 10f;
                    float dir = direction.ToRotation();
                    Vector2 BaseVel = new Vector2(3f, 0f);
                    
                    Random beeNumRandom = new Random();
                    int bees_per_shot = beeNumRandom.Next(MIN_BEES_PER_SHOT, MAX_BEES_PER_SHOT + 1);
                    for (int i = 0; i < bees_per_shot; i++)
                    {
                        Random random = new Random();
                        float random_seed_dir = (float)random.NextDouble();
                        float random_seed_vel = (float)random.NextDouble();
                        float dir_offset = (random_seed_dir*2-1) * ModGlobal.PI_FLOAT / 8f;
                        float vel_offset = (random_seed_vel*2-1) * 0.5f + 1f;
                        // Main.NewText("random_seed: " + random_seed + " dir_offset: " + dir_offset);
                        Vector2 Velocity = (BaseVel * vel_offset).RotatedBy(dir + dir_offset);

                        int bee = Projectile.NewProjectile(
                            Projectile.GetSource_FromAI(),
                            Projectile.Center,
                            Velocity,
                            // ModProjectileID.HoneyCombSentryBullet,
                            ProjectileID.Bee,
                            Projectile.damage,
                            0,
                            Projectile.owner);

                        if(bee != Main.maxProjectiles)
                        {
                            Main.projectile[bee].DamageType = DamageClass.Summon;
                            // Main.NewText(bee + " " + Main.projectile[bee].DamageType);
                            // Main.NewText("Damage: " + Projectile.damage + "Bee Damage: " + Main.projectile[bee].damage);
                        }
                    }
                    for(int i = 0;i < 5;i++)
                    {
                        Random random = new Random();
                        float random_seed = (float)random.NextDouble();
                        float scale = random_seed * 3f + 0.5f;
                        int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 50, default, 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].scale = scale;
                    }
                    shootTimer = 0;
                }
            }
            else
            {
                shootTimer = 0; // Reset if no target
            }

            // Animation
            // UpdateAnimation(target, shootTimer);
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