using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Graphics.Shaders;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class StardustSentry : ModProjectile
    {
        // animation
        private const int FRAME_COUNT = 5;
        private const int MAX_FRAME_SPEED = 12;
        private const int MIN_FRAME_SPEED = 3;
        // private int fireCooldown = 30;
        private const int FIRE_INTERVAL = 120;
        private const int SIGNAL_TIME = 50;
        private const int BULLET_NUM = 3;
        private const int BULLET_INTERVAL = 5;
        private float currentFrameSpeed = (float)MAX_FRAME_SPEED;
        private int fireTimer = 0;
        private int signalTimer = 0;
        private int bulletTimer = 0;
        private int bulletCnt = 0;
        private long floatCnt = 0;
        private long floatingDeckCnt = 0;
        private bool canShoot = false;
        private Vector2 targetCenter = Vector2.Zero;

        private const float REAL_BULLET_SPEED = 15f;
        private const float PRED_BULLET_SPEED = 15f;
        private const float DEACCELERATION = 0.5f;
        private const bool USE_PREDICTION = true;
        private int SignalID = -1;
        private string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/StardustSentry";
        public override string Texture => TEXTURE_PATH;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = FRAME_COUNT;
        }

        public override void SetDefaults()
        {
            Projectile.width = 68;
            Projectile.height = 98;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.sentry = true;
            Projectile.netImportant = true;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            // Float in the air
            Vector2 vel = Projectile.velocity;
            Vector2 vel_dir = vel.SafeNormalize(Vector2.Zero);
            if(vel.Length() > DEACCELERATION)
            {
                Projectile.velocity -= vel_dir * DEACCELERATION;
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }

            float FloatAmplitude = 0.5f;
            FloatAmplitude = Math.Min(FloatAmplitude, 2f / vel.Length());
            
            float FloatOffset = (float)(Math.Sin(floatCnt * 0.05f) * FloatAmplitude);
            floatCnt++;
            Projectile.Center += new Vector2(0, FloatOffset);

            // teleport to owner if needed
            float maxDistance = 3000;
            Player owner = Main.player[Projectile.owner];
            if (Vector2.Distance(Projectile.Center, owner.Center) > maxDistance)
            {
                TryTeleportNearPlayer(owner);
            }

            // search for targets and emit signal
            int fireInterval = FIRE_INTERVAL;
            NPC target = MinionAIHelper.SearchForTargets(
                owner, 
                Projectile, 
                1700f, 
                false, 
                null).TargetNPC;
            if (target != null)
            {
                targetCenter = target.Center;
                if (fireTimer >= fireInterval)
                {
                    EmitSignal(target);
                    fireTimer = 0;
                    signalTimer = 0;
                }
                currentFrameSpeed -= 0.1f;
                if (currentFrameSpeed < MIN_FRAME_SPEED) currentFrameSpeed = MIN_FRAME_SPEED;
            }
            else
            {
                currentFrameSpeed += 0.1f;
                if (currentFrameSpeed > MAX_FRAME_SPEED) currentFrameSpeed = MAX_FRAME_SPEED;
            }

            // check if signal available
            if (SignalID >= 0)
            {
                Projectile signalProj = Main.projectile[SignalID];
                if (!signalProj.active || signalProj.type != ModProjectileID.StardustSentrySignal)   // not available, set to -1
                {
                    SignalID = -1;
                }
                else // available
                {
                    signalProj.Center = Projectile.Center + new Vector2(0, -Projectile.height / 2f - 1000f / 2f);
                    signalProj.velocity = Projectile.velocity;

                    signalTimer++;
                    if (signalTimer >= SIGNAL_TIME && target != null)
                    {
                        signalTimer = 0;
                        canShoot = true;
                    }
                }
            }
            
            if(canShoot)
            {
                bulletTimer++;
                if(bulletTimer >= BULLET_INTERVAL)
                {
                    bulletCnt++;
                    bulletTimer = 0;
                    Vector2 ProjOffset = new Vector2(MinionAIHelper.RandomFloat(-1000f, 1000f), -1000f);
                    Vector2 ProjSpawnPos = targetCenter + ProjOffset;
                    Vector2 PredictedPos = targetCenter; 
                    if(target != null)
                        PredictedPos = MinionAIHelper.PredictTargetPosition(ProjSpawnPos, targetCenter, target.velocity, 50f);
                    Vector2 direction = (PredictedPos - ProjSpawnPos).SafeNormalize(Vector2.Zero);
                    Projectile proj = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        ProjSpawnPos,
                        direction * 50f,
                        ModProjectileID.StardustSentryBullet,
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    ProjectileID.Sets.SentryShot[proj.type] = true;
                    if(target != null)
                        proj.ai[0] = (float)(target.whoAmI);

                    if(bulletCnt >= BULLET_NUM)
                    {
                        bulletCnt = 0;
                        canShoot = false;
                    }
                }
            }

            fireTimer++;
            if(fireTimer >= fireInterval)
                fireTimer = fireInterval;

            UpdateAnimation(target);
        }

        private void EmitSignal(NPC target)
        {
            Vector2 SignalOffset = new Vector2(0, -Projectile.height/2f-1000f/2f);
            Projectile proj = Projectile.NewProjectileDirect(
                Projectile.GetSource_FromAI(),
                Projectile.Center + SignalOffset,
                new Vector2(0, 0),
                ModProjectileID.StardustSentrySignal,
                0,
                0,
                Projectile.owner
            );
            
            SignalID = proj.whoAmI;
        }

        private void TryTeleportNearPlayer(Player player)
        {
            Vector2 bestPosition = Projectile.Center;
            int bestTileCount = int.MaxValue;

            for (int xOffset = -5; xOffset <= 5; xOffset++)
            {
                for (int yOffset = -5; yOffset <= 5; yOffset++)
                {
                    Vector2 checkPos = player.Center + new Vector2(xOffset * 16, yOffset * 16);
                    if (!Collision.SolidCollision(checkPos, Projectile.width, Projectile.height))
                    {
                        int tileCount = CountNearbySolidTiles(checkPos);
                        if (tileCount < bestTileCount)
                        {
                            bestTileCount = tileCount;
                            bestPosition = checkPos;
                        }
                    }
                }
            }

            // add random offset to best position
            bestPosition += new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10));

            // Teleport if we found a better position
            Projectile.position = bestPosition;
            Projectile.netUpdate = true;

            // Optional: Visual or sound effect
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(bestPosition, Projectile.width, Projectile.height, DustID.MagicMirror, Scale: 1.5f);
                }
                SoundEngine.PlaySound(SoundID.Item8, bestPosition);
            }
        }

        private int CountNearbySolidTiles(Vector2 center)
        {
            int tileCount = 0;
            Point tileCenter = center.ToTileCoordinates();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Tile tile = Framing.GetTileSafely(tileCenter.X + x, tileCenter.Y + y);
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        tileCount++;
                    }
                }
            }

            return tileCount;
        }

        public override bool MinionContactDamage()
		{
			return false;
		}

        private void UpdateAnimation(NPC target)
        {
            Projectile.frameCounter++;
            floatingDeckCnt++;
            if (Projectile.frameCounter >= currentFrameSpeed)
            {
                Projectile.frame = (Projectile.frame + 1) % (FRAME_COUNT - 1);
                Projectile.frameCounter = 0;
            }

            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                Projectile.spriteDirection = direction.X > 0 ? -1 : 1;
            }

            if (Main.rand.NextFloat() < 0.05f)
            {
                Dust dust;
                Vector2 position = Projectile.Center - Projectile.Size / 2f;
                dust = Dust.NewDustDirect(position, Projectile.width, Projectile.height, 111, 0f, 0f, 0, new Color(0,67,255), 1f);
                dust.velocity = new Vector2(0f, MinionAIHelper.RandomFloat(-0.3f, 0f));
                // dust.shader = GameShaders.Armor.GetSecondaryShader(30, Main.LocalPlayer);
            }
        }

        public override void Kill(int timeLeft)
        {
            if (SignalID >= 0)
            {
                Projectile signalProj = Main.projectile[SignalID];
                if (!signalProj.active || signalProj.type != ModProjectileID.StardustSentrySignal)   // not available, set to -1
                {
                    SignalID = -1;
                    signalProj.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(TEXTURE_PATH).Value;
            int width = texture.Width;
            int FrameHeight = Projectile.height;
            int CurrentFrameHeight = texture.Height / FRAME_COUNT * (Projectile.frame+1);

            // draw the turret part
            Rectangle TurretRect = new Rectangle(0, 0, width, FrameHeight);
            Vector2 TurretWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0f, 0f));
            Vector2 TurretOrigin = new Vector2(width / 2f, FrameHeight / 2f);
            MinionAIHelper.DrawPart(
                Projectile,
                texture,
                TurretWorldPos,
                TurretRect,
                lightColor,
                Projectile.rotation,
                TurretOrigin
            );

            // draw the floating deck part
            float FloatAmplitude = 5f;
            float FloatOffset = (float)(Math.Cos(floatingDeckCnt * 0.03f) * FloatAmplitude);
            // Main.NewText("FloatOffset:" + FloatOffset);
            Rectangle DeckRect = new Rectangle(0, CurrentFrameHeight, width, FrameHeight);
            Vector2 DeckWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2(0f, CurrentFrameHeight+FloatOffset));
            Vector2 DeckOrigin = new Vector2(width / 2f, CurrentFrameHeight + FrameHeight / 2f);
            MinionAIHelper.DrawPart(
                Projectile,
                texture,
                DeckWorldPos,
                DeckRect,
                lightColor,
                Projectile.rotation,
                DeckOrigin
            );

            return false;
        }
    }
}