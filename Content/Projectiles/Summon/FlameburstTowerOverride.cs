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
    public class FlameburstTowerOverride : IProjectileOverride
    {
        // constants
        protected const float BULLET_SPEED_1 = 10f;
        protected const float BULLET_SPEED_2 = 15f;
        protected const float BULLET_SPEED_3 = 20f;
        protected const float SPEED_FACTOR = 1.4f;
        protected const float PRED_BULLET_SPEED = 15f;
        protected const bool USE_PREDICTION = true;
        protected const float MAX_RANGE_1 = 50f*16f;
        protected const float MAX_RANGE_2 = 60f*16f;
        protected const float MAX_RANGE_3 = 60f*16f;
        protected const float RANGE_FACTOR = 1.5f;
        protected const int SHOOT_ANIMATION_SPEED = 5;
        protected const int SHOOT_INTERVAL = (int)(60 * 0.5f);
        protected virtual int FRAME_COUNT => 9;

        protected Vector2 direction = new Vector2(1f, 0f);
        protected int shootTimer = SHOOT_INTERVAL / 2;
        protected int TimerDuringShoot = -1;

        protected bool SquireArmorSet = false;
        protected bool SquireAltArmorSet = false;

        public FlameburstTowerOverride()
        {
            RegisterFlags["SetDefaults"] = true;
            RegisterFlags["PreAI"] = true;
        }

        public override void SetDefaults(Projectile projectile)
        {
            projectile.width = 30;
			projectile.height = 54;
            projectile.aiStyle = -1;
            projectile.timeLeft = Projectile.SentryLifeTime;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.manualDirectionChange = true;
            projectile.sentry = true;
            projectile.netImportant = true;
            projectile.DamageType = DamageClass.Summon;
        }

        protected void CheckArmorSet(Player player)
        {
            SquireArmorSet = player.armor[0].type == 3797 &&
                   player.armor[1].type == 3798 &&
                   player.armor[2].type == 3799;
            SquireAltArmorSet = player.armor[0].type == 3874 &&
                   player.armor[1].type == 3875 &&
                   player.armor[2].type == 3876;
        }

        protected bool GetEnhanced(Player player)
        {
            CheckArmorSet(player);
            if(SquireArmorSet || SquireAltArmorSet)
            {
                return true;
            }
            return false;
        }

        protected void UpdateAnimation(Projectile projectile)
        {
            // face towards target
            projectile.spriteDirection = direction.X > 0 ? 1 : -1;

            // play shoot animation
            if (TimerDuringShoot >= 0 || projectile.frame > FRAME_COUNT - 1)
            {
                projectile.frameCounter++;
                if(projectile.frameCounter >= SHOOT_ANIMATION_SPEED)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                }
            }
            else
            {
                projectile.frame = 0;
                projectile.frameCounter = 0;
            }
        }
    }

    public class FlameburstShotOverride : IProjectileOverride
    {
        protected virtual float HOMING_RANGE => 500f;
        protected virtual float CONTROL_P => 0.15f;
        protected virtual float CONTROL_D => 0.01f;
        protected virtual float MAX_TURN_SPEED => 0.02f;

        protected float lastError = 0f;

        public FlameburstShotOverride()
        {
            RegisterFlags["AI"] = true;
        }

        public override void AI(Projectile projectile)
        {
            // get target
            int targetId = (int)projectile.ai[0];
            NPC target = targetId != -1 ? Main.npc[targetId] : null;

            if(target == null || !target.active || (target.Center - projectile.Center).Length() > HOMING_RANGE)
            {
                return;
            }

            float direction = (target.Center - projectile.Center).ToRotation();
            float dir_err = direction - projectile.velocity.ToRotation();
            dir_err = (dir_err + ModGlobal.PI_FLOAT) % ModGlobal.TWO_PI_FLOAT - ModGlobal.PI_FLOAT;
            float turn_speed = dir_err * CONTROL_P + (dir_err - lastError) * CONTROL_D;
            lastError = dir_err;
            turn_speed = MathHelper.Clamp(turn_speed, -MAX_TURN_SPEED, MAX_TURN_SPEED);
            Vector2 velocity = projectile.velocity;
            projectile.velocity = velocity.RotatedBy(turn_speed);
        }
    }

    public class FlameburstShotT1Override : FlameburstShotOverride
    {
        protected override float HOMING_RANGE => 500f;
        protected override float CONTROL_P => 0.15f;
        protected override float CONTROL_D => 0.01f;
        protected override float MAX_TURN_SPEED => 0.02f;
    }

    public class FlameburstTowerT1Override : FlameburstTowerOverride
    {
        protected override int FRAME_COUNT => 7;
        public override bool PreAI(Projectile projectile)
        {
            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // calculate range
            bool Enhanced = GetEnhanced(Main.player[projectile.owner]);
            float maxRange = MAX_RANGE_1 * (Enhanced ? RANGE_FACTOR : 1f);

            // targeting
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                maxRange, 
                true, 
                n => (n.Center - projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/4f || (n.Center - projectile.Center).ToRotation() >= 3f*ModGlobal.PI_FLOAT/4f).TargetNPC;


            int shootInterval = SHOOT_INTERVAL + FRAME_COUNT * SHOOT_ANIMATION_SPEED;


            // shooting
            if(target != null)
            {
                // calculate target direction
                Vector2 TargetPredictedPos = MinionAIHelper.PredictTargetPosition(projectile, target, BULLET_SPEED_1 * (Enhanced ? SPEED_FACTOR : 1f), 60, 3);
                direction = TargetPredictedPos - projectile.Center;
                direction.Normalize();

                // fire!
                if(shootTimer >= shootInterval)
                {
                    shootTimer = 0;
                }
                if(shootTimer == 0)
                {
                    TimerDuringShoot = 0;
                }
                
            }

            // keep shooting even lose target
            if(TimerDuringShoot >= 0)
            {
                TimerDuringShoot++;
                if(TimerDuringShoot >= SHOOT_ANIMATION_SPEED * FRAME_COUNT)
                {
                    TimerDuringShoot = -1;
                }
                else if (TimerDuringShoot == SHOOT_ANIMATION_SPEED * FRAME_COUNT / 2)
                {
                    Vector2 BulletVelocity = direction * BULLET_SPEED_1 * (Enhanced ? SPEED_FACTOR : 1f);
                    Vector2 bulletOffset = new Vector2(0, -20f);

                    Projectile proj = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity, 
                                            ProjectileID.DD2FlameBurstTowerT1Shot, 
                                            projectile.damage, 
                                            projectile.knockBack, 
                                            projectile.owner);

                    // transfer target to the new projectile
                    if(target != null)
                    {
                        proj.ai[0] = target.whoAmI;
                    }
                    else
                    {
                        proj.ai[0] = -1;
                    }
                }
            }
            else if (target == null)
            {
                direction = new Vector2(1f, 0);
            }

            shootTimer++;
            if(shootTimer >= shootInterval)
            {
                shootTimer = shootInterval;
            }

            UpdateAnimation(projectile);

            return false;
        }
    }

    public class FlameburstShotT2Override : FlameburstShotOverride
    {
        protected override float HOMING_RANGE => 750f;
        protected override float CONTROL_P => 0.15f;
        protected override float CONTROL_D => 0.02f;
        protected override float MAX_TURN_SPEED => 0.08f;
    }

    public class FlameburstTowerT2Override : FlameburstTowerOverride
    {
        public override bool PreAI(Projectile projectile)
        {
            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // calculate range
            bool Enhanced = GetEnhanced(Main.player[projectile.owner]);
            float maxRange = MAX_RANGE_2 * (Enhanced ? RANGE_FACTOR : 1f);

            // targeting
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                maxRange, 
                true, 
                n => (n.Center - projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/4f || (n.Center - projectile.Center).ToRotation() >= 3f*ModGlobal.PI_FLOAT/4f).TargetNPC;


            int shootInterval = SHOOT_INTERVAL + FRAME_COUNT * SHOOT_ANIMATION_SPEED;


            // shooting
            if(target != null)
            {
                // calculate target direction
                Vector2 TargetPredictedPos = MinionAIHelper.PredictTargetPosition(projectile, target, BULLET_SPEED_2 * (Enhanced ? SPEED_FACTOR : 1f), 60, 3);
                direction = TargetPredictedPos - projectile.Center;
                direction.Normalize();

                // fire!
                if(shootTimer >= shootInterval)
                {
                    shootTimer = 0;
                }
                if(shootTimer == 0)
                {
                    TimerDuringShoot = 0;
                }
                
            }

            // keep shooting even lose target
            if(TimerDuringShoot >= 0)
            {
                TimerDuringShoot++;
                if(TimerDuringShoot >= SHOOT_ANIMATION_SPEED * FRAME_COUNT)
                {
                    TimerDuringShoot = -1;
                }
                else if (TimerDuringShoot == SHOOT_ANIMATION_SPEED * FRAME_COUNT / 2)
                {
                    Vector2 BulletVelocity = direction * BULLET_SPEED_2 * (Enhanced ? SPEED_FACTOR : 1f);
                    Vector2 bulletOffset = new Vector2(0, -20f);

                    Projectile proj = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity, 
                                            ProjectileID.DD2FlameBurstTowerT2Shot, 
                                            projectile.damage, 
                                            projectile.knockBack, 
                                            projectile.owner);

                    // transfer target to the new projectile
                    if(target != null)
                    {
                        proj.ai[0] = target.whoAmI;
                    }
                    else
                    {
                        proj.ai[0] = -1;
                    }
                }
            }
            else if (target == null)
            {
                direction = new Vector2(1f, 0);
            }

            shootTimer++;
            if(shootTimer >= shootInterval)
            {
                shootTimer = shootInterval;
            }

            UpdateAnimation(projectile);

            return false;
        }
    }

    public class FlameburstShotT3Override : FlameburstShotOverride
    {
        protected override float HOMING_RANGE => 1000f;
        protected override float CONTROL_P => 2f;
        protected override float CONTROL_D => 0.05f;
        protected override float MAX_TURN_SPEED => 1.0f;
    }

    public class FlameburstTowerT3Override : FlameburstTowerOverride
    {
        public override bool PreAI(Projectile projectile)
        {
            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // calculate range
            bool Enhanced = GetEnhanced(Main.player[projectile.owner]);
            float maxRange = MAX_RANGE_3 * (Enhanced ? RANGE_FACTOR : 1f);

            // targeting
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                maxRange, 
                true, 
                n => (n.Center - projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/4f || (n.Center - projectile.Center).ToRotation() >= 3f*ModGlobal.PI_FLOAT/4f).TargetNPC;


            int shootInterval = SHOOT_INTERVAL + FRAME_COUNT * SHOOT_ANIMATION_SPEED;


            // shooting
            if(target != null)
            {
                // calculate target direction
                // Vector2 TargetPredictedPos = MinionAIHelper.PredictTargetPosition(projectile, target, BULLET_SPEED_3 * (Enhanced ? SPEED_FACTOR : 1f), 60, 3);
                direction = target.Center - projectile.Center;
                direction.Normalize();

                // fire!
                if(shootTimer >= shootInterval)
                {
                    shootTimer = 0;
                }
                if(shootTimer == 0)
                {
                    TimerDuringShoot = 0;
                }
                
            }

            // keep shooting even lose target
            if(TimerDuringShoot >= 0)
            {
                TimerDuringShoot++;
                if(TimerDuringShoot >= SHOOT_ANIMATION_SPEED * FRAME_COUNT)
                {
                    TimerDuringShoot = -1;
                }
                else if (TimerDuringShoot == SHOOT_ANIMATION_SPEED * FRAME_COUNT / 2)
                {
                    Vector2 BulletVelocity = direction * BULLET_SPEED_3 * (Enhanced ? SPEED_FACTOR : 1f);
                    Vector2 bulletOffset = new Vector2(0, -20f);

                    Projectile proj = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity, 
                                            ProjectileID.DD2FlameBurstTowerT2Shot, 
                                            projectile.damage, 
                                            projectile.knockBack, 
                                            projectile.owner);

                    // transfer target to the new projectile
                    if(target != null)
                    {
                        proj.ai[0] = target.whoAmI;
                    }
                    else
                    {
                        proj.ai[0] = -1;
                    }
                }
            }
            else if (target == null)
            {
                direction = new Vector2(1f, 0);
            }

            shootTimer++;
            if(shootTimer >= shootInterval)
            {
                shootTimer = shootInterval;
            }

            UpdateAnimation(projectile);

            return false;
        }
    }
}