using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Utils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class MachineGunSentry : SentryWithSpawnAnime
    {
        // timers
        private int shootTimer;
        private int spawnTimer;

        // textures
        private const string BASE_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/MachineGunSentryV3Base";
        private const string GUN_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/MachineGunSentryV3Gun";
        private const string FRONT_BOARD_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/SentryFrontBoard";
        private const string BACK_BOARD_TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/SentryBackBoard";
        public override string Texture => BASE_TEXTURE_PATH;

        // sentry state
        private bool onGround;
        private const bool USE_PREDICTION = true;

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
        public static float Gravity = ModGlobal.SENTRY_GRAVITY;
        public static float MaxGravity = 20f;

        // direction
        private Vector2 direction = new Vector2(0, -1);
        // private Vector2 direction = new Vector2(1, 0);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = FRAME_COUNT;
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 48;
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

                int shootInterval = SHOOT_INTERVAL;
                if(owner.HasBuff(BUFF_ID))
                {
                    shootInterval = (int)(shootInterval * ENHANCEMENT_FACTOR);
                }

                if (shootTimer >= shootInterval)
                {
                    // calculate prediction
                    Vector2 PredictedPos = target.Center;
                    
                    if(USE_PREDICTION)
                    {
                        PredictedPos = MinionAIHelper.PredictTargetPosition(
                            Projectile, target, PRED_BULLET_SPEED);
                    }

                    // Fire!
                    direction = PredictedPos - Projectile.Center;
                    direction.Normalize();
                    Vector2 bulletVelocity = direction * REAL_BULLET_SPEED;

                    Vector2 bulletOffset = new Vector2(15f, -5f) + direction * 27f;

                    Projectile bullet = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + bulletOffset,
                        bulletVelocity,
                        // ModProjectileID.MachineGunSentryBullet,
                        ProjectileID.Bullet,
                        Projectile.damage,
                        0,
                        Projectile.owner);

                    bullet.DamageType = DamageClass.Summon;
                    // Main.NewText("Damage: " + Projectile.damage + "Bullet Damage: " + bullet.damage);

                    shootTimer = 0; // Reset shoot animation

                    SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
                }
            }
            else
            {
                shootTimer = 0; // Reset if no target
            }

            // Animation
            // UpdateAnimation(target);
            Projectile.spriteDirection = direction.X > 0 ? 1 : -1;
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


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D BaseTexture = ModContent.Request<Texture2D>(BASE_TEXTURE_PATH).Value;
            Texture2D GunTexture = ModContent.Request<Texture2D>(GUN_TEXTURE_PATH).Value;
            Texture2D FrontBoardTexture = ModContent.Request<Texture2D>(FRONT_BOARD_TEXTURE_PATH).Value;
            Texture2D BackBoardTexture = ModContent.Request<Texture2D>(BACK_BOARD_TEXTURE_PATH).Value;

            float SpawnProcess = (float)Math.Min((float)spawnTimer / SPAWN_TIME * 2, 1.0f);
            float GunAdjustProcess = (float)Math.Min(Math.Max((float)(spawnTimer - SPAWN_TIME/2) / SPAWN_TIME * 2, 0.0f), 1.0f);
            float SpawnStartY = (BaseTexture.Height + 22f) * (1 - SpawnProcess);

            if (spawnTimer >= SPAWN_TIME / 2 && spawnTimer < SPAWN_TIME)
            {
                // (0, -1) -> (-1, 0)
                float ang = GunAdjustProcess * ModGlobal.PI_FLOAT / 2;
                direction = new Vector2(-(float)Math.Sin(ang), -(float)Math.Cos(ang));
                direction.Normalize();
            }

            // always draw base
            Vector2 BaseWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0, 0));
            Vector2 BaseOrigin = new Vector2(BaseTexture.Width / 2, BaseTexture.Height / 2);
            float ClipThreshold = BaseWorldPos.Y + 19f;
            MinionAIHelper.DrawPart(
                Projectile,
                BaseTexture,
                BaseWorldPos,
                new Rectangle(0, 0, BaseTexture.Width, BaseTexture.Height),
                lightColor,
                Projectile.rotation,
                BaseOrigin
            );

            // draw back board
            float BackBoardWorldY = SpawnStartY-2f;
            Vector2 BackBoardWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0, BackBoardWorldY)); // 1 3
            Vector2 BackBoardOrigin = new Vector2(BackBoardTexture.Width / 2, BackBoardTexture.Height / 2);
            Rectangle BackBoardRect = MinionAIHelper.CalculateClipRect(new Rectangle(0, 0, BackBoardTexture.Width, BackBoardTexture.Height), BackBoardWorldPos, BackBoardOrigin, -1f, ClipThreshold);
            // Main.NewText("BackBoardRect:" + BackBoardRect + " WorldPos:" + BackBoardWorldPos + " Origin:" + BackBoardOrigin + " ClipThreshold:" + ClipThreshold);
            MinionAIHelper.DrawPart(
                Projectile,
                BackBoardTexture,
                BackBoardWorldPos,
                BackBoardRect,
                lightColor,
                Projectile.rotation,
                BackBoardOrigin
            );

            // draw gun
            float GunWorldY = SpawnStartY-14f;
            Vector2 GunWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(3 * Projectile.spriteDirection, GunWorldY));
            Vector2 GunOrigin = new Vector2(GunTexture.Width / 2 - 6 * Projectile.spriteDirection, 7);
            Vector2 GunWorldPosTemp = new Vector2(GunWorldPos.Y, GunWorldPos.X);
            Vector2 GunOriginTemp = new Vector2(GunOrigin.Y, GunOrigin.X);
            Rectangle GunRectClip = MinionAIHelper.CalculateClipRect(new Rectangle(0, 0, GunTexture.Width, GunTexture.Height), GunWorldPosTemp, GunOrigin, ClipThreshold, -1f);
            // Main.NewText("GunRectClip:" + GunRectClip + " GunWorldPos:" + GunWorldPosTemp + " GunOrigin:" + GunOriginTemp + " ClipThreshold:" + ClipThreshold);
            Rectangle GunRect = new Rectangle(GunTexture.Width - GunRectClip.Width, GunRectClip.Y, GunRectClip.Width , GunTexture.Height);
            MinionAIHelper.DrawPart(
                Projectile,
                GunTexture,
                GunWorldPos,
                GunRect,
                lightColor,
                // Projectile.rotation,
                direction.ToRotation() + (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi),
                GunOrigin
            );

            // draw front board
            float FrontBoardWorldY = SpawnStartY-8f;
            Vector2 FrontBoardWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(-6 * Projectile.spriteDirection, FrontBoardWorldY)); // 7 9
            Vector2 FrontBoardOrigin = new Vector2(FrontBoardTexture.Width / 2, FrontBoardTexture.Height / 2);
            Rectangle FrontBoardRect = MinionAIHelper.CalculateClipRect(new Rectangle(0, 0, FrontBoardTexture.Width, FrontBoardTexture.Height), FrontBoardWorldPos, FrontBoardOrigin, -1f, ClipThreshold);
            
            MinionAIHelper.DrawPart(
                Projectile,
                FrontBoardTexture,
                FrontBoardWorldPos,
                FrontBoardRect,
                lightColor,
                Projectile.rotation,
                FrontBoardOrigin
            );


            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            onGround = true;
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public override void SetAttached(bool attached)
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