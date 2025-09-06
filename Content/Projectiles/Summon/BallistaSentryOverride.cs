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
using SummonerExpansionMod.Utils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class BallistaSentryOverride : IProjectileOverride
    {
        // constants
        protected const float MAX_TURN_SPEED = 0.2f;
        protected const float CONTROL_P = 0.15f;
        protected const float MAX_RANGE = 800f;
        protected const float BULLET_SPEED = 20f;
        protected const float PRED_BULLET_SPEED = 15f;
        protected const bool USE_PREDICTION = true;
        protected const int SHOOT_INTERVAL_LV1 = (int)(60 * 2.67f);
        protected const int SHOOT_INTERVAL_LV2 = (int)(60 * 1.67f);
        protected const int SHOOT_INTERVAL_LV3 = (int)(60 * 1f);
        protected const int SHOOT_INTERVAL_LV4 = (int)(60 * 0.5f);
        protected const int SHOOT_ANIMATION_SPEED = 5;
        protected const int FRAME_COUNT = 6;

        protected Vector2 TargetDirection = new Vector2(1f, 0f);
        protected int shootTimer = SHOOT_INTERVAL_LV1 / 2;
        protected int TimerDuringShoot = -1;

        protected bool SquireArmorSet = false;
        protected bool SquireAltArmorSet = false;

        public void SetDefaults(Projectile projectile)
        {
            projectile.aiStyle = -1;
            projectile.timeLeft = Projectile.SentryLifeTime;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.manualDirectionChange = true;
            projectile.sentry = true;
            projectile.netImportant = true;
            projectile.DamageType = DamageClass.Summon;
        }

        public void AI(Projectile projectile) {}

        public virtual bool PreAI(Projectile projectile) => true;

        public bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) => true;

        public void Kill(Projectile projectile, int timeLeft) {}

        protected void CheckArmorSet(Player player)
        {
            SquireArmorSet = player.armor[0].type == 3800 &&
                   player.armor[1].type == 3801 &&
                   player.armor[2].type == 3802;
            SquireAltArmorSet = player.armor[0].type == 3871 &&
                   player.armor[1].type == 3872 &&
                   player.armor[2].type == 3873;

            // Main.NewText("player armor: " + player.armor[0].type + " " + player.armor[1].type + " " + player.armor[2].type);
        }

        protected int GetShootInterval(Player player)
        {
            CheckArmorSet(player);
            int interval = SHOOT_INTERVAL_LV1;
            if(SquireArmorSet)
            {
                if(player.HasBuff(BuffID.BallistaPanic))
                {
                    interval = SHOOT_INTERVAL_LV3;
                }
            }
            else if(SquireAltArmorSet)
            {
                if(player.HasBuff(BuffID.BallistaPanic))
                {
                    interval = SHOOT_INTERVAL_LV4;
                }
                else
                {
                    interval = SHOOT_INTERVAL_LV2;
                }
            }

            return interval + FRAME_COUNT * SHOOT_ANIMATION_SPEED;
            
        }

        protected void UpdateAnimation(Projectile projectile)
        {
            // calculate turn speed using PID control
            float TargetAngle = TargetDirection.ToRotation();
            float CurrentAngle = projectile.rotation;
            float DeltaAngle = TargetAngle - CurrentAngle;
            DeltaAngle = (DeltaAngle + ModGlobal.PI_FLOAT) % ModGlobal.TWO_PI_FLOAT - ModGlobal.PI_FLOAT;
            float TurnSpeed = DeltaAngle * CONTROL_P;
            TurnSpeed = MathHelper.Clamp(TurnSpeed, -MAX_TURN_SPEED, MAX_TURN_SPEED);
            projectile.rotation = CurrentAngle + TurnSpeed;

            // face towards target
            projectile.spriteDirection = Math.Cos(CurrentAngle) > 0 ? 1 : -1;

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
    public class BallistaTowerT1Override : BallistaSentryOverride
    {

        public void SetDefaults(Projectile projectile)
        {
            base.SetDefaults(projectile);
            projectile.width = 26;
            projectile.height = 54;
        }

        public override bool PreAI(Projectile projectile)
        {
            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // targeting
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                MAX_RANGE, 
                true, 
                n => (n.Center - projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/4f || (n.Center - projectile.Center).ToRotation() >= 3f*ModGlobal.PI_FLOAT/4f).TargetNPC;


            int shootInterval = GetShootInterval(Main.player[projectile.owner]);


            // shooting
            if(target != null)
            {
                // calculate target direction
                Vector2 TargetPredictedPos = MinionAIHelper.PredictTargetPosition(projectile, target, PRED_BULLET_SPEED);
                TargetDirection = TargetPredictedPos - projectile.Center;

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
                    Vector2 BulletVelocity = projectile.rotation.ToRotationVector2() * BULLET_SPEED;
                    Vector2 bulletOffset = new Vector2(10f, 0).RotatedBy(projectile.rotation);

                    Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity, 
                                            ProjectileID.DD2BallistraProj, 
                                            projectile.damage, 
                                            projectile.knockBack, 
                                            projectile.owner);
                }
            }
            else if (target == null )
            {
                TargetDirection = new Vector2(1f, 0);
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
    public class BallistaTowerT2Override : BallistaSentryOverride
    {
        private Vector2 TargetPredictedDirection = new Vector2(1f, 0);
        private Vector2 TargetOriginDirection = new Vector2(1f, 0);
        // private const float BULLET_SPEED = 25f;
        // private const float PRED_BULLET_SPEED = 25f;

        private const float MAX_RANGE = 1200f;

        public void SetDefaults(Projectile projectile)
        {
            base.SetDefaults(projectile);
            projectile.width = 26;
            projectile.height = 54;
        }

        public override bool PreAI(Projectile projectile)
        {
            int shootInterval = GetShootInterval(Main.player[projectile.owner]);

            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // targeting
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                MAX_RANGE, 
                true, 
                n => (n.Center - projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/4f || (n.Center - projectile.Center).ToRotation() >= 3f*ModGlobal.PI_FLOAT/4f).TargetNPC;


            // shooting
            if(target != null)
            {
                // calculate target direction
                Vector2 TargetPredictedPos = MinionAIHelper.PredictTargetPosition(projectile, target, PRED_BULLET_SPEED);
                TargetPredictedDirection = TargetPredictedPos - projectile.Center;
                TargetOriginDirection = target.Center - projectile.Center;
                TargetDirection = TargetPredictedDirection + TargetOriginDirection;

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
                    float ProjectileOffsetAngle = (TargetPredictedDirection.ToRotation() - TargetOriginDirection.ToRotation()) / 2f;
                    ProjectileOffsetAngle = (ProjectileOffsetAngle + ModGlobal.PI_FLOAT) % ModGlobal.TWO_PI_FLOAT - ModGlobal.PI_FLOAT;
                    ProjectileOffsetAngle = (float) MathHelper.Clamp(ProjectileOffsetAngle, 0.02f, ModGlobal.PI_FLOAT/4f);
                    Vector2 BulletVelocity_1 = new Vector2(BULLET_SPEED, 0).RotatedBy(projectile.rotation + ProjectileOffsetAngle);
                    Vector2 BulletVelocity_2 = new Vector2(BULLET_SPEED, 0).RotatedBy(projectile.rotation - ProjectileOffsetAngle);
                    Vector2 bulletOffset = new Vector2(10f, 0).RotatedBy(projectile.rotation);

                    Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity_1, 
                                            ProjectileID.DD2BallistraProj, 
                                            projectile.damage / 2, 
                                            projectile.knockBack, 
                                            projectile.owner);
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity_2, 
                                            ProjectileID.DD2BallistraProj, 
                                            projectile.damage / 2, 
                                            projectile.knockBack, 
                                            projectile.owner);
                }
            }
            else if (target == null )
            {
                TargetDirection = new Vector2(1f, 0);
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
    public class BallistaTowerT3Override : BallistaSentryOverride
    {
        private Vector2 TargetPredictedDirection = new Vector2(1f, 0);
        private Vector2 TargetOriginDirection = new Vector2(1f, 0);

        private const float MAX_RANGE = 1500f;
        // private const float BULLET_SPEED = 50f;
        // private const float PRED_BULLET_SPEED = 50f;
        public void SetDefaults(Projectile projectile)
        {
            base.SetDefaults(projectile);
            projectile.width = 26;
            projectile.height = 54;
        }

        public override bool PreAI(Projectile projectile)
        {
            int shootInterval = GetShootInterval(Main.player[projectile.owner]);

            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // targeting
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                MAX_RANGE, 
                true, 
                n => (n.Center - projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/4f || (n.Center - projectile.Center).ToRotation() >= 3f*ModGlobal.PI_FLOAT/4f).TargetNPC;

            


            // shooting
            if(target != null)
            {
                Main.NewText("Target speed: " + target.velocity.Length());
                // calculate target direction
                Vector2 TargetPredictedPos = MinionAIHelper.PredictTargetPosition(projectile, target, PRED_BULLET_SPEED);
                TargetPredictedDirection = TargetPredictedPos - projectile.Center;
                TargetOriginDirection = target.Center - projectile.Center;
                TargetDirection = TargetPredictedDirection + TargetOriginDirection;

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
                    float ProjectileOffsetAngle = (TargetPredictedDirection.ToRotation() - TargetOriginDirection.ToRotation()) / 2f;
                    ProjectileOffsetAngle = (ProjectileOffsetAngle + ModGlobal.PI_FLOAT) % ModGlobal.TWO_PI_FLOAT - ModGlobal.PI_FLOAT;
                    ProjectileOffsetAngle = (float) MathHelper.Clamp(ProjectileOffsetAngle, 0.02f, ModGlobal.PI_FLOAT/4f);
                    Vector2 BulletVelocity_1 = new Vector2(BULLET_SPEED, 0).RotatedBy(projectile.rotation + ProjectileOffsetAngle);
                    Vector2 BulletVelocity_2 = new Vector2(BULLET_SPEED, 0).RotatedBy(projectile.rotation - ProjectileOffsetAngle);
                    Vector2 BulletVelocity_3 = new Vector2(BULLET_SPEED, 0).RotatedBy(projectile.rotation);
                    Vector2 bulletOffset = new Vector2(10f, 0).RotatedBy(projectile.rotation);

                    Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity_1, 
                                            ProjectileID.DD2BallistraProj, 
                                            projectile.damage / 3, 
                                            projectile.knockBack, 
                                            projectile.owner);
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity_2, 
                                            ProjectileID.DD2BallistraProj, 
                                            projectile.damage / 3, 
                                            projectile.knockBack, 
                                            projectile.owner);
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                            projectile.Center + bulletOffset, 
                                            BulletVelocity_3, 
                                            ProjectileID.DD2BallistraProj, 
                                            projectile.damage / 3, 
                                            projectile.knockBack, 
                                            projectile.owner);
                }
            }
            else if (target == null )
            {
                TargetDirection = new Vector2(1f, 0);
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