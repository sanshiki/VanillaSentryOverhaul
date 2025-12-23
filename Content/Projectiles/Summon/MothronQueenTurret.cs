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
    public class MothronQueenTurret : ModProjectile
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
        private Vector2 direction = new Vector2(0, -1);
        private const float COMPENSATE_ANGLE = 15f * ModGlobal.DEG_TO_RAD_FLOAT;

        private Dictionary<int, Projectile> eggDict = new Dictionary<int, Projectile>();

        public static float Gravity = ModGlobal.SENTRY_GRAVITY;
        public static float MaxGravity = 20f;
        private const float BULLET_SPEED = 25f;
        private const float BULLET_GRAVITY = 1.0f;
        private const int FRAME_COUNT = 9;

        private const string BASE_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/MothronQueenTurretBase";
        private const string GUN_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/MothronQueenTurretGun";
        private const string HOLDER_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/MothronQueenTurretHolder";

        public override string Texture => BASE_TEXTURE_PATH;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = FRAME_COUNT;
        }

        public override void SetDefaults()
        {
            Projectile.width = 78;
            Projectile.height = 52;
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
                1500f, 
                false, 
                n => (n.Center - Projectile.Center).ToRotation() <= ModGlobal.PI_FLOAT/6f || (n.Center - Projectile.Center).ToRotation() >= 5f*ModGlobal.PI_FLOAT/6f).TargetNPC;

            

            if (target != null)
            {
                shootInterval = isOnSand ? SHOOT_INTERVAL_FAST : SHOOT_INTERVAL;

                direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                if(direction.ToRotation() <= -ModGlobal.PI_FLOAT/2f - COMPENSATE_ANGLE || direction.ToRotation() >= -ModGlobal.PI_FLOAT/2f + COMPENSATE_ANGLE)
                {
                    direction = direction.RotatedBy(COMPENSATE_ANGLE * (direction.X > 0f ? -1f : 1f));
                }

                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0;
                }
                if (shootTimer == 0)
                {
                    // Fire!
                    Vector2 ShootOffset = new Vector2(0f, -15f);
                    Vector2 ShootCenter = Projectile.Center + ShootOffset;

                    Projectile egg = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        ShootCenter,
                        direction * BULLET_SPEED,
                        ModProjectileID.MothronQueenTurretBullet,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    if(!eggDict.ContainsKey(egg.whoAmI))
                    {
                        eggDict.Add(egg.whoAmI, egg);
                    }
                    
                    shootTimer = 0; // Reset shoot animation

                    SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
                }

                // update egg queue
                foreach(Projectile egg in eggDict.Values)
                {
                    if((target.Center - egg.Center).Length() < 200f)
                    {
                        // Vector2 BabySpiderDir = (target.Center - egg.Center).SafeNormalize(Vector2.Zero);
                        // egg.velocity = BabySpiderDir * egg.velocity.Length();
                        eggDict.Remove(egg.whoAmI);
                        egg.Kill();
                    }
                    if(egg.timeLeft <= 0 || !egg.active)
                    {
                        eggDict.Remove(egg.whoAmI);
                    }
                }
            }


            shootTimer++;
            if(shootTimer >= shootInterval)
                shootTimer = shootInterval;

        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // set velocity to 0
            // Projectile.velocity = Vector2.Zero;
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

        public override bool PreDraw(ref Color lightColor)
        {
            // draw base
            Texture2D baseTexture = ModContent.Request<Texture2D>(BASE_TEXTURE_PATH).Value;
            int width = baseTexture.Width;
            int height = baseTexture.Height;
            Rectangle baseRect = new Rectangle(0, 0, width, height);
            Vector2 baseWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0, 0));
            Vector2 baseOrigin = new Vector2(width / 2, height / 2);
            MinionAIHelper.DrawPart(Projectile, baseTexture, baseWorldPos, baseRect, lightColor, Projectile.rotation, baseOrigin);

            // draw gun (spin with direction)
            Texture2D gunTexture = ModContent.Request<Texture2D>(GUN_TEXTURE_PATH).Value;
            Rectangle gunRect = new Rectangle(0, 0, width, height);
            Vector2 gunWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0, 0));
            Vector2 gunOrigin = new Vector2(40, 36);
            MinionAIHelper.DrawPart(Projectile, gunTexture, gunWorldPos, gunRect, lightColor, direction.ToRotation() + ModGlobal.PI_FLOAT/2f, gunOrigin);

            // draw holder
            Texture2D holderTexture = ModContent.Request<Texture2D>(HOLDER_TEXTURE_PATH).Value;
            Rectangle holderRect = new Rectangle(0, 0, width, height);
            Vector2 holderWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0, 0));
            Vector2 holderOrigin = new Vector2(width / 2, height / 2);
            MinionAIHelper.DrawPart(Projectile, holderTexture, holderWorldPos, holderRect, lightColor, Projectile.rotation, holderOrigin);

            return false;
        }
    }
}