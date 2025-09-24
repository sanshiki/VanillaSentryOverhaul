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
    public class EyeBallTurretOverride : IProjectileOverride
    {
        private const int SHOOT_INTERVAL = 60;
        private const float REAL_BULLET_SPEED = 10f;
        private const float PRED_BULLET_SPEED = 12f;
        private int shootTimer = 0;

        private Vector2 direction = new Vector2(0, -1);

        public EyeBallTurretOverride()
        {
            RegisterFlags["SetDefaults"] = true;
            RegisterFlags["PreAI"] = true;
            RegisterFlags["OnTileCollide"] = true;
            RegisterFlags["TileCollideStyle"] = true;
        }

        public override void SetDefaults(Projectile projectile)
        {
			projectile.width = 18;
			projectile.height = 60;
			projectile.aiStyle = -1;
			projectile.timeLeft = Projectile.SentryLifeTime;
			projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.sentry = true;
            projectile.netImportant = true;
            projectile.DamageType = DamageClass.Summon;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            projectile.velocity = Vector2.Zero;
            return true;
        }

        public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool PreAI(Projectile projectile)
        {
            // apply gravity
            MinionAIHelper.ApplyGravity(projectile, ModGlobal.SENTRY_GRAVITY, ModGlobal.SENTRY_MAX_FALL_SPEED);

            // search for target
            NPC target = MinionAIHelper.SearchForTargets(
                Main.player[projectile.owner], 
                projectile, 
                800f, 
                true, 
                null).TargetNPC;

            if(target != null)
            {
                Vector2 ShootOffset = new Vector2(0, -20f);
                Vector2 ShootCenter = projectile.Center + ShootOffset;
                Vector2 PredictedPos = MinionAIHelper.PredictTargetPosition(ShootCenter, target.Center, target.velocity, PRED_BULLET_SPEED, 60, 3);
                direction = PredictedPos - projectile.Center;
                direction.Normalize();

                if(shootTimer >= SHOOT_INTERVAL)
                {
                    shootTimer = 0;
                }
                if(shootTimer == 0)
                {
                    
                    Projectile bullet = Projectile.NewProjectileDirect(
                        projectile.GetSource_FromThis(),
                        ShootCenter,
                        direction * REAL_BULLET_SPEED,
                        ProjectileID.HoundiusShootiusFireball,
                        projectile.damage,
                        projectile.knockBack, 
                        projectile.owner);
                }
            }

            shootTimer++;
            if(shootTimer >= SHOOT_INTERVAL)
            {
                shootTimer = SHOOT_INTERVAL;
            }

            return false;
        }
    }
}