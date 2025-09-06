using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Items.Weapons.Summon;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.Utils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{

    public class SentryRecallInfo
    {
        public int ID;
        public bool TileCollide;
        public bool IsRecalled;
        public Vector2 TargetPos;
        public float Seed;
    }

    public class FlagPole : ModProjectile
    {
        // predraw constants
        private const int BASE_HEIGHT = 9;
        private const int MIN_POLE_PENGTH = 64;
        private const int TIP_HEIGHT = 30;
        private const int REPEAT_SLICE_HEIGHT = 2;
        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/FlagPole";
        public override string Texture => TEXTURE_PATH;

        // waving
        private const float ROT_ANGLE = 150f * ModGlobal.PI_FLOAT / 180f;
        private const int TIME_LEFT_WAVE = 24;

        // raising
        private const float RAISE_MAX_HEIGHT = 16f * 4f; // 4 tiles
        private const int TIME_LEFT_RAISE = 60;
        private const float RAISE_MAX_SPEED = 2f * RAISE_MAX_HEIGHT / (float)TIME_LEFT_RAISE;
        private const float RAISE_ACC = RAISE_MAX_SPEED / (float)TIME_LEFT_RAISE * 2f;

        // plant and recall sentries
        private const int PLANT_EXIST_DURATION = 60*60*10; // 10 min
        private const float GRAVITY = 0.8f;
        private const float MAX_FALL_SPEED = 20f;
        private const float SENTRY_RECALL_SPEED = 50f;
        private const float SENTRY_RECALL_THRESHOLD = 200f;
        private const float SENTRY_RECALL_DECAY_DIST = 1500f;
        private const float SENTRY_RECALL_MAX_DIST = 4000f;
        private const float SENTRY_RECALL_TARGET_OFFSET = 150f;
        private const float SENTRY_RANDOM_OFFSET = 20f;

        // recall flag
        private const int RECALL_EXIST_DURATION = 60*60; // 1 min
        private const float RECALL_SPEED = 30f;
        private const float RECALL_ROTATE_SPEED = 0.3f;
        
        // state constants
        public const int WAVE_STATE = 0; // left-click: wave
        public const int RAISE_STATE = 1; // right-short-press: raise
        public const int PLANT_STATE = 2; // right-long-press: plant
        public const int RECALL_STATE = 3; // right-click after plant: recall


        // flag constants
        private const int FLAG_WIDTH = 128;
        private const int FLAG_HEIGHT = 80;
        private Projectile FlagProjectile = null;

        // buff constants
        private int BUFF_ID = -1;
        private int TAG_BUFF_ID = -1;
        private const int BUFF_START_TIME = 25;
        private const int BUFFING_INTERVAL = 60;
        private const int BUFF_DURATION = 150; // 30s
        private const int SENTRY_TARGET_DURATION = 60*60; // 1 min
        // public attributes
        public float WaveDirection = 1;
        public int State = WAVE_STATE;
        public bool SwitchFlag = false;
        public int OnGroundCnt = 0;
        public int PoleLength = MIN_POLE_PENGTH;

        // private variables
        private int FixedDirection = 1;
        private float AimAngle = 0f;
        private bool Initialized = false;
        private bool SentryRecallInitialized = false;
        private int RaiseTime = 0;
        private float ItemRot = -2.05f;
        private Dictionary<int, int> hitCountPerNPC = new Dictionary<int, int>();
        // private Dictionary<int, bool> AffectedSentryIDs = new Dictionary<int, bool>(); // record id and original tileCollide
        private List<SentryRecallInfo> SentryRecallInfos = new List<SentryRecallInfo>();
        private Vector2 STICK_OFFSET = new Vector2(0f, -MIN_POLE_PENGTH/2f+20f);

        

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = PoleLength;
            Projectile.friendly = true;
            Projectile.ownerHitCheck = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TIME_LEFT_WAVE;
            Projectile.localNPCHitCooldown = Projectile.timeLeft;
            Projectile.usesLocalNPCImmunity = true;
            
            BUFF_ID = ModBuffID.SentryEnhancement;
            TAG_BUFF_ID = ModBuffID.SentryTarget;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            STICK_OFFSET = new Vector2(0f, -PoleLength/2f+20f);
            Projectile.height = PoleLength;

            switch (State)
            {
                case WAVE_STATE:
                {
                    WaveAI(player);
                } break;
                case RAISE_STATE:
                {
                    RaiseAI(player);
                } break;
                case PLANT_STATE:
                {
                    PlantAI(player);
                } break;
                case RECALL_STATE:
                {
                    RecallAI(player);
                } break;
            }

            if(FlagProjectile == null)
            {
                FlagProjectile = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModProjectileID.FlagProjectile, 0, 0, Projectile.owner);
            }
            if(FlagProjectile != null)
            {
                Vector2 FlagOffset = new Vector2(-FLAG_WIDTH/2f * Projectile.spriteDirection, (FLAG_HEIGHT-Projectile.height)/2f);
                FlagProjectile.Center = Projectile.Center + FlagOffset.RotatedBy(Projectile.rotation);
                FlagProjectile.rotation = Projectile.rotation;
                FlagProjectile.spriteDirection = Projectile.spriteDirection;

                if(FlagProjectile.ModProjectile is FlagProjectile flagProjectile)
                {
                    if(State == WAVE_STATE || State == RECALL_STATE)
                    {
                        flagProjectile.UseFastAnimation = true;
                    }
                    else
                    {
                        flagProjectile.UseFastAnimation = false;
                    }
                }
            }
        }

        private void WaveAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                FixedDirection = player.direction;
                // get mouse dir
                Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                AimAngle = aimDir.ToRotation() + ModGlobal.PI_FLOAT / 2f;
                // Main.NewText("WaveAI:"+Projectile.identity);
                SoundEngine.PlaySound(SoundID.Item32, Projectile.Center);
                Initialized = true;
            }

            float dir = WaveDirection/*  * FixedDirection */;
            Vector2 StickOffset = new Vector2(STICK_OFFSET.X * dir, STICK_OFFSET.Y);
            // float ItemRot = player.itemRotation;
            float RotRate = (ItemRot/*  * FixedDirection */ + 2.05f) / (1.275f + 2.05f); // 0 to 1
            ItemRot += (1.275f + 2.05f) / (float)TIME_LEFT_WAVE;
            
            float Rot = AimAngle + (ROT_ANGLE * RotRate - ROT_ANGLE / 2f) * dir;

            Projectile.Center = CenterMapping(player.Center, StickOffset, Rot);
            Projectile.rotation = Rot;
            Projectile.spriteDirection = (int)dir;

            // Main.NewText(Projectile.Center);

            if (Projectile.timeLeft == TIME_LEFT_WAVE)
            {
                hitCountPerNPC.Clear();
            }

        }

        private void RaiseAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                Projectile.timeLeft = TIME_LEFT_RAISE;
                FixedDirection = player.direction;
                Projectile.friendly = false;
                // Main.NewText("RaiseAI:"+Projectile.identity);
                Initialized = true;
            }

            float RaiseHeight = 0f;
            if(RaiseTime < TIME_LEFT_RAISE / 2)
            {
                RaiseHeight = RAISE_ACC * (float)RaiseTime * (float)RaiseTime / 2f;
            }
            else
            {
                RaiseHeight = RAISE_MAX_HEIGHT - RAISE_ACC * (float)(TIME_LEFT_RAISE - RaiseTime) * (float)(TIME_LEFT_RAISE - RaiseTime) / 2f;
            }
            RaiseTime++;

            if(RaiseTime >= BUFF_START_TIME)
            {
                player.AddBuff(BUFF_ID, BUFF_DURATION);
            }

            Vector2 StickOffset = new Vector2(STICK_OFFSET.X * FixedDirection, STICK_OFFSET.Y);
            Projectile.Center = CenterMapping(player.MountedCenter, StickOffset, 0) + new Vector2(0, RAISE_MAX_HEIGHT/2f-RaiseHeight);
            Projectile.spriteDirection = FixedDirection;
            // Projectile.velocity = player.velocity;
            
            if(SwitchFlag)
            {
                State = PLANT_STATE;
                SwitchFlag = false;
                Initialized = false;
            }
        }

        private void PlantAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                Projectile.timeLeft = PLANT_EXIST_DURATION;
                FixedDirection = player.direction;
                Projectile.tileCollide = true;
                Projectile.friendly = false;

                // reset buff
                player.AddBuff(BUFF_ID, BUFF_DURATION);

                // Main.NewText("PlantAI:"+Projectile.identity);
                Initialized = true;
            }

            // apply gravity
            Projectile.velocity.X -= 0.02f;
            Projectile.velocity.Y += GRAVITY;
            if(Projectile.velocity.Y > MAX_FALL_SPEED)
            {
                Projectile.velocity.Y = MAX_FALL_SPEED;
            }

            if(OnGroundCnt >= 20)
            {
                if(!SentryRecallInitialized)
                {
                    // find affected sentries
                    SentryRecallInfos.Clear();
                    foreach(var proj in Main.projectile)
                    {
                        if(proj.active && proj.owner == Projectile.owner && proj.sentry && (proj.Center - Projectile.Center).Length() <= SENTRY_RECALL_MAX_DIST)
                        {
                            SentryRecallInfo info = new SentryRecallInfo() { 
                                ID = proj.identity, 
                                TileCollide = proj.tileCollide, 
                                TargetPos = proj.Center,
                                Seed = Main.rand.NextFloat(-SENTRY_RANDOM_OFFSET, SENTRY_RANDOM_OFFSET)
                            };
                            SentryRecallInfos.Add(info);
                            proj.tileCollide = false;
                        }
                    }
                    // set target pos
                    int SentryCount = SentryRecallInfos.Count;
                    float TotalLength = SentryCount == 0 ? 0f : (float)Math.Sqrt(SentryCount-1) * 200f;
                    Random random = new Random();
                    int ranDir = random.Next(2) == 0 ? 1 : -1;
                    for(int i = 0; i < SentryCount; i++)
                    {
                        SentryRecallInfo info = SentryRecallInfos[i];
                        float LocalX = SentryCount == 1 ? 0f : (float)i / (float)(SentryCount-1) * TotalLength - TotalLength / 2f;
                        float PreciseX = Projectile.Center.X + LocalX * ranDir;
                        float PreciseY = Projectile.Center.Y - SENTRY_RECALL_TARGET_OFFSET;
                        float X = PreciseX + info.Seed;
                        float Y = PreciseY + info.Seed;
                        SentryRecallInfos[i].TargetPos = new Vector2(X, Y);

                        // Main.NewText("Sentry "+i+" target pos: "+info.TargetPos);
                    }

                    // reset buff
                    player.AddBuff(BUFF_ID, BUFF_DURATION);

                    SentryRecallInitialized = true;
                }
                foreach(var info in SentryRecallInfos)
                {
                    var sentry = Main.projectile[info.ID];
                    Vector2 ToTargetDist = info.TargetPos - sentry.Center;
                    if(ToTargetDist.Length() >= SENTRY_RECALL_THRESHOLD && !info.IsRecalled)
                    {
                        Vector2 ToTargetDir = ToTargetDist.SafeNormalize(Vector2.UnitX);
                        float DecayFactor = Math.Min(1f, ToTargetDist.Length() / SENTRY_RECALL_DECAY_DIST);
                        sentry.velocity = ToTargetDir * SENTRY_RECALL_SPEED * DecayFactor;
                        // sentry.velocity = Vector2.Zero;
                        // sentry.Center += ToTargetDir * SENTRY_RECALL_SPEED * DecayFactor;
                    }
                    else
                    {
                        sentry.tileCollide = info.TileCollide;
                        info.IsRecalled = true;
                    }
                }
            }
            Vector2 ToOwnerDist = player.Center - Projectile.Center;
            if(ToOwnerDist.Length() >= 4000f)
            {
                Projectile.Kill();
            }

            if(SwitchFlag)
            {
                State = RECALL_STATE;
                SwitchFlag = false;
                Initialized = false;
            }
        }

        private void RecallAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                Projectile.friendly = true;
                Projectile.tileCollide = false;
                Projectile.timeLeft = RECALL_EXIST_DURATION;
                Projectile.localNPCHitCooldown = Projectile.timeLeft;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.ownerHitCheck = false;

                Initialized = true;
            }

            Vector2 RecallDist = player.Center - Projectile.Center;
            Vector2 RecallDirection = RecallDist.SafeNormalize(Vector2.UnitX);
            // Projectile.velocity = RecallDirection * RECALL_SPEED;
            Projectile.Center += RecallDirection * RECALL_SPEED;
            Projectile.rotation += RECALL_ROTATE_SPEED * Projectile.spriteDirection;

            if(RecallDist.Length() <= 100f)
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D flagTexture = ModContent.Request<Texture2D>(TEXTURE_PATH).Value;
            int width = flagTexture.Width;
            int height = Projectile.height;
            int TextureHeight = flagTexture.Height;
            Vector2 origin = new Vector2(width / 2, height / 2);

            // draw tip part
            Rectangle tipRect = new Rectangle(0, 0, width, TIP_HEIGHT);
            Vector2 tipLocalPos = new Vector2(0, 0);
            Vector2 tipWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, tipLocalPos);
            DrawPart(flagTexture, tipWorldPos, tipRect, lightColor, origin);

            // draw repeat part
            for (int i = 0; i < (PoleLength - TIP_HEIGHT - BASE_HEIGHT) / REPEAT_SLICE_HEIGHT; i++)
            {
                int repeatY = i * REPEAT_SLICE_HEIGHT + TIP_HEIGHT;
                Rectangle repeatRect = new Rectangle(0, TIP_HEIGHT, width, REPEAT_SLICE_HEIGHT);
                Vector2 repeatLocalPos = new Vector2(0, repeatY);
                Vector2 repeatWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, repeatLocalPos);
                DrawPart(flagTexture, repeatWorldPos, repeatRect, lightColor, origin);
            }

            // draw base part
            Rectangle baseRect = new Rectangle(0, TextureHeight - BASE_HEIGHT, width, BASE_HEIGHT);
            Vector2 baseLocalPos = new Vector2(0, PoleLength - BASE_HEIGHT);
            Vector2 baseWorldPos = MinionAIHelper.ConvertToWorldPos(Projectile, baseLocalPos);
            DrawPart(flagTexture, baseWorldPos, baseRect, lightColor, origin);

            return false;
        }

        private void DrawPart(Texture2D texture, Vector2 worldPos, Rectangle rect, Color color, Vector2 origin)
        {
            Main.spriteBatch.Draw(
                texture,
                worldPos,
                rect,
                color,
                Projectile.rotation,
                origin,
                Projectile.scale,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0f
            );
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnGroundCnt++;
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override void Kill(int timeLeft)
        {
            if(FlagProjectile != null)
            {
                FlagProjectile.Kill();
            }

            foreach(var info in SentryRecallInfos)
            {
                var proj = Main.projectile[info.ID];
                proj.tileCollide = info.TileCollide;
                // Main.NewText("Sentry "+entry.Key+" tileCollide: "+entry.Value);
            }
            SentryRecallInfos.Clear();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 PoleStart = Projectile.Center + new Vector2(0, PoleLength/2f).RotatedBy(Projectile.rotation);
            Vector2 PoleEnd = Projectile.Center + new Vector2(0, PoleLength/2f).RotatedBy(Projectile.rotation+Math.PI);
            float collisionPoint = 0f;
            if(Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), PoleStart, PoleEnd, 10f, ref collisionPoint))
            {
                // Main.NewText("Collision: "+collisionPoint);
                return true;
            }
            // int dust1 = Dust.NewDust(PoleStart, 10, 10, DustID.Torch, 0f, 0f, 0, default, 1f);
            // int dust2 = Dust.NewDust(PoleEnd, 10, 10, DustID.Sand, 0f, 0f, 0, default, 1f);
            // int dust3 = Dust.NewDust(Projectile.Center, 10, 10, DustID.Grass, 0f, 0f, 0, default, 1f);
            // Main.dust[dust1].velocity = Vector2.Zero;
            // Main.dust[dust1].noGravity = true;
            // Main.dust[dust2].velocity = Vector2.Zero;
            // Main.dust[dust2].noGravity = true;
            // Main.dust[dust3].velocity = Vector2.Zero;
            // Main.dust[dust3].noGravity = true;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // foreach(NPC npc in Main.npc)
            // {
            //     if(npc.active && npc.HasBuff(TAG_BUFF_ID))
            //     {
            //         npc.DelBuff(npc.FindBuffIndex(TAG_BUFF_ID));
            //     }
            // }
            target.AddBuff(TAG_BUFF_ID, SENTRY_TARGET_DURATION);

            Player player = Main.player[Projectile.owner];
            player.MinionAttackTargetNPC = target.whoAmI;
            // player.HasMinionAttackTargetNPC = true;
        }

        private Vector2 ConvertToWorldPos(Vector2 localPos)
        {
            return Projectile.Center + localPos.RotatedBy(Projectile.rotation) - Main.screenPosition;
        }

        private Vector2 CenterMapping(Vector2 center, Vector2 offset, float rotation)
        {
            return center + offset.RotatedBy(rotation);
        }
    }
}