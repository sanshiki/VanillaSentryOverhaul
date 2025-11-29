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
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    // sentry info storage
    public class SentryRecallInfo
    {
        public int ID;
        public bool TileCollide;
        public bool IsRecalled;
        public Vector2 TargetPos;
        public float Seed;
        public int Anchor_ID;
        public bool AnchorInited;
    }

    public class FlagProjectile : ModProjectile
    {
        /* ------------------------- Predraw Constants ------------------------- */
        // basic predraw
        protected const int BASE_HEIGHT = 9;  // height of the pole base
        protected const int MIN_POLE_PENGTH = 64; // minimum length of the pole
        protected const int TIP_HEIGHT = 30;  // height of the pole tip
        protected const int REPEAT_SLICE_HEIGHT = 2; // height of each repeat slice
        protected virtual string FLAGPOLE_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/FlagProjectile";
        public override string Texture => FLAGPOLE_TEXTURE_PATH;

        // waving
        protected virtual float ROT_ANGLE => 240f * ModGlobal.PI_FLOAT / 180f;   // waving range angle (150 degrees)
        protected virtual int TIME_LEFT_WAVE => 24; // waving duration
        protected virtual int HANDHELD_POINT_OFFSET => 60;   // offset of the point on the pole when held
        protected virtual bool TILE_CUT_RANGE_DEBUG => false;

        // raising
        protected float RAISE_MAX_HEIGHT => 16f * 4f + HANDHELD_POINT_OFFSET / 2f; // max height of the pole when raised
        // protected int TIME_LEFT_RAISE = 60; // raising duration
        // protected float RAISE_MAX_SPEED => 2f * RAISE_MAX_HEIGHT / (float)TIME_LEFT_RAISE;   // max speed of the pole when raised
        // protected float RAISE_ACC => RAISE_MAX_SPEED / (float)TIME_LEFT_RAISE * 2f; // acceleration of the pole when raised
        protected virtual int FULLY_CHARGED_DUST => DustID.CrimsonSpray;

        // planting and recalling sentries
        protected virtual int PLANT_EXIST_DURATION => 60*60*10; // 10 min
        protected virtual float GRAVITY => 0.8f;
        protected virtual float MAX_FALL_SPEED => 16f;
        protected virtual float SENTRY_RECALL_SPEED => 50f;
        protected virtual float SENTRY_RECALL_THRESHOLD => 40f;
        protected virtual float SENTRY_RECALL_DECAY_DIST => 1000f;
        protected virtual float SENTRY_RECALL_MAX_DIST => 4000f;
        protected virtual float SENTRY_RECALL_TARGET_OFFSET => 70f;
        protected virtual float SENTRY_RANDOM_OFFSET => 20f;
        protected virtual bool USE_CUSTOM_SENTRY_RECALL => false;

        // recalling flag
        protected virtual int TIME_LEFT_RECALL => 60*60; // 1 min
        protected virtual float RECALL_SPEED => 30f;
        protected virtual float RECALL_ROTATE_SPEED => 0.3f;
        protected virtual int RECALL_SOUND_INTERVAL => 10;

        /* ------------------------- State Constants ------------------------- */
        public const int WAVE_STATE = 0; // left-click: wave
        public const int RAISE_STATE = 1; // right-short-press: raise
        public const int PLANT_STATE = 2; // right-long-press: plant
        public const int RECALL_STATE = 3; // right-click after plant: recall


        /* ------------------------- Flag Cloth Constants ------------------------- */
        protected virtual int FLAG_WIDTH => 128;
        protected virtual int FLAG_HEIGHT => 80;
        protected virtual float ROT_DISPLACEMENT => 2.05f + 2.05f;  //  1.275f + 2.05f
        protected virtual float SLOW_WAVE_AMPLITUDE => 10f;
        protected virtual float SLOW_WAVE_SPEED_FACTOR => 7f;

        protected virtual float FAST_WAVE_AMPLITUDE => 10f;
        protected virtual float FAST_WAVE_SPEED_FACTOR => 1.5f;
        protected virtual int WAVE_SLICE_WIDTH => 4;
        protected virtual string FLAG_CLOTH_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/TestFlag";

        /* ------------------------- Tail Constants ------------------------- */
        public bool TailEnabled = true;
        protected virtual bool TAIL_ENABLE_GLOBAL => true;
        protected virtual int TAIL_LENGTH => 6;
        protected virtual float TAIL_OFFSET_X_1 => -100f;  // -123
        protected virtual float TAIL_OFFSET_Y_1 => -135f;  // -213
        protected virtual float TAIL_OFFSET_X_2 => -100f;  // -123
        protected virtual float TAIL_OFFSET_Y_2 => -85f;   // -72
        protected virtual float TAIL_OFFSET_ROT_1 => 0f;
        protected virtual float TAIL_OFFSET_ROT_2 => 0f;
        protected virtual Vector2 SPIN_CENTER_OFFSET => new Vector2(65f, 225f);
        protected virtual float SPIN_CENTER_OFFSET_ROT => 0f;
        protected virtual Color TAIL_COLOR => new Color(98, 0, 0, 95);
        protected virtual bool TAIL_DYNAMIC_DEBUG => false;
        protected virtual string FLAG_TAIL_TEXTURE_PATH => ModGlobal.MOD_TEXTURE_PATH + "Vertexes/SwordTail4";

        /* ------------------------- Buff Constants ------------------------- */
        protected virtual int ENHANCE_BUFF_ID => ModBuffID.SentryEnhancement;
        protected virtual int NPC_DEBUFF_ID => BuffID.SwordWhipNPCDebuff;
        protected virtual int BUFF_START_TIME => 25;
        protected virtual int ENHANCE_BUFF_DURATION => 60*3; // 3s
        protected virtual int NPC_DEBUFF_DURATION => 60*7; // 7s
        protected virtual float DAMAGE_DECAY_FACTOR => 0.8f;

        /* ------------------------- Public Attributes ------------------------- */
        public float WaveDirection = 1;
        public int State = WAVE_STATE;
        public bool SwitchFlag = false;
        public int OnGroundCnt = 0;
        public int PoleLength = MIN_POLE_PENGTH;    // real pole length, can be modified in realtime
        public int TimeLeftRaise = 60;
        public float AttackSpeed = 1f;

        /* ------------------------- Private Variables ------------------------- */
        protected int FixedDirection = 1;   // record the fixed direction of the pole when raised
        protected float AimAngle = 0f;   // record the aim angle of the pole when waving
        protected bool Initialized = false;     // init flag
        protected bool SentryRecallInitialized = false;     // sentry recall init flag
        protected int RaiseTime = 0;     // raise time counter
        protected int RecallTime = 0;     // recall time counter
        protected float ItemRot = -2.05f;
        protected float RotAcc = 0f;
        protected float RotSpd = 0f;
        protected bool HasPlayedOnGroundSound = false;
        protected int hitCount = 0;
        protected List<SentryRecallInfo> SentryRecallInfos = new List<SentryRecallInfo>();
        protected Vector2 STICK_OFFSET = new Vector2(0f, -MIN_POLE_PENGTH / 2f + 20f);
        protected List<float> StickOffsetList = new List<float>();
        protected float FlagClothAmplitude = 0f;
        protected float FlagClothWaveSpeed = 0f;
        protected bool UseFastAnimation = false;

        public override void SetStaticDefaults()//以下照抄
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
            ProjectileID.Sets.TrailCacheLength[Type] = 50;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = PoleLength;
            Projectile.friendly = true;
            Projectile.ownerHitCheck = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = (int)(TIME_LEFT_WAVE / AttackSpeed);
            Projectile.localNPCHitCooldown = Projectile.timeLeft;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            if (TAIL_DYNAMIC_DEBUG)
            {
                DynamicParamManager.Register("PoleLength", 280, 80, 1000);
                DynamicParamManager.Register("TailLength", (float)TAIL_LENGTH, 3, 50);
                DynamicParamManager.Register("Offset_X.1", TAIL_OFFSET_X_1, -300, 300);
                DynamicParamManager.Register("Offset_Y.1", TAIL_OFFSET_Y_1, -300, 300);
                DynamicParamManager.Register("Offset_X.2", TAIL_OFFSET_X_2, -300, 300);
                DynamicParamManager.Register("Offset_Y.2", TAIL_OFFSET_Y_2, -300, 300);
                DynamicParamManager.Register("Offset_rot.1", TAIL_OFFSET_ROT_1, -3.14f, 3.14f);
                DynamicParamManager.Register("Offset_rot.2", TAIL_OFFSET_ROT_2, -3.14f, 3.14f);
                DynamicParamManager.Register("SpinCenterOffset.X", SPIN_CENTER_OFFSET.X, -300, 300);
                DynamicParamManager.Register("SpinCenterOffset.Y", SPIN_CENTER_OFFSET.Y, -300, 300);
                DynamicParamManager.Register("SpinCenterOffset.Rot", SPIN_CENTER_OFFSET_ROT, -3.14f, 3.14f);
                DynamicParamManager.Register("TailColor.R", TAIL_COLOR.R, 0, 255);
                DynamicParamManager.Register("TailColor.G", TAIL_COLOR.G, 0, 255);
                DynamicParamManager.Register("TailColor.B", TAIL_COLOR.B, 0, 255);
                DynamicParamManager.Register("TailColor.A", TAIL_COLOR.A, 0, 255);
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if(TAIL_DYNAMIC_DEBUG)
                PoleLength = (int)DynamicParamManager.Get("PoleLength").value;
            STICK_OFFSET = new Vector2(0f, -PoleLength/2f);     // player handheld point: avoid holding the tip
            Projectile.height = PoleLength;
            AttackSpeed = player.GetAttackSpeed(DamageClass.Melee);            

            // basic flag state machine
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

            DamageGrassAlongBlade(player);

            CreateDustEffect(player);
        }

        protected void WaveAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                FixedDirection = player.direction;
                // get mouse dir
                Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                AimAngle = aimDir.ToRotation() + ModGlobal.PI_FLOAT / 2f;
                // Main.NewText("WaveAI:"+Projectile.identity);
                // SoundEngine.PlaySound(SoundID.Item32, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item102, Projectile.Center);
                Initialized = true;

                RotSpd = 0f;

                Projectile.timeLeft = (int)(TIME_LEFT_WAVE / AttackSpeed);
            }

            float dir = WaveDirection/*  * FixedDirection */;
            
            // float ItemRot = player.itemRotation;
            float RotRate = (ItemRot/*  * FixedDirection */ + ROT_DISPLACEMENT/2f) / ROT_DISPLACEMENT; // 0 to 1
            float WaveUseTime = TIME_LEFT_WAVE / AttackSpeed;

            // rotate speed: accelerate then deaccelerate
            // RotAcc = -2 * ROT_DISPLACEMENT / WaveUseTime / WaveUseTime;
            if(RotRate < 0.333f)
            {
                RotAcc = 2*ROT_DISPLACEMENT / (float)WaveUseTime / (float)(WaveUseTime * 0.333f);
            }
            else
            {
                RotAcc = -2*ROT_DISPLACEMENT / (float)WaveUseTime / (float)(WaveUseTime * (1f-0.333f));
            }
            RotSpd += RotAcc;

            float offset_delta = 0f;
            if (RotRate < 0.5f)
            {
                offset_delta = MathHelper.Lerp(0.5f, -0.1f, RotRate * 2f) * PoleLength;
            }
            else
            {
                offset_delta = MathHelper.Lerp(-0.1f, 0.5f, (RotRate - 0.5f) * 2f) * PoleLength;
            }
            STICK_OFFSET = new Vector2(0f, -PoleLength / 2f + offset_delta);
            Vector2 StickOffset = new Vector2(STICK_OFFSET.X * dir, STICK_OFFSET.Y);
            StickOffsetList.Add(-PoleLength / 2f + offset_delta);
            
            // ItemRot += (1.275f + 2.05f) / (float)WaveUseTime;
            ItemRot += RotSpd;

            // Main.NewText("RotRate: "+RotRate + " RotAcc: "+RotAcc + " RotSpd: "+RotSpd + " ItemRot: "+ItemRot);

            float Rot = AimAngle + (ROT_ANGLE * RotRate - ROT_ANGLE / 2f) * dir;

            Projectile.Center = CenterMapping(player.Center, StickOffset, Rot);
            Projectile.rotation = Rot;
            Projectile.spriteDirection = (int)dir;

            // Main.NewText(Projectile.Center);

        }

        protected void RaiseAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                Projectile.timeLeft = TimeLeftRaise;
                FixedDirection = player.direction;
                Projectile.friendly = false;
                // Main.NewText("RaiseAI:"+Projectile.identity);
                Initialized = true;
            }
            float RaiseMaxHeight = 16f * 4f + HANDHELD_POINT_OFFSET / 2f;
            float RaiseMaxSpeed = 2f * RaiseMaxHeight / (float)TimeLeftRaise;
            float RaiseAcc = RaiseMaxSpeed / (float)TimeLeftRaise * 2f;

            float RaiseHeight = 0f;
            if(RaiseTime < TimeLeftRaise / 2)
            {
                RaiseHeight = RaiseAcc * (float)RaiseTime * (float)RaiseTime / 2f;
            }
            else
            {
                RaiseHeight = RaiseMaxHeight - RaiseAcc * (float)(TimeLeftRaise - RaiseTime) * (float)(TimeLeftRaise - RaiseTime) / 2f;
            }
            RaiseTime++;
 
            if(RaiseTime >= TimeLeftRaise * 0.42f)
            {
                player.AddBuff(ENHANCE_BUFF_ID, ENHANCE_BUFF_DURATION);
            }

            // Vector2 StickOffset = new Vector2(STICK_OFFSET.X * FixedDirection, STICK_OFFSET.Y);
            Vector2 StickOffset = new Vector2(0f, -PoleLength/2f+40f);
            Projectile.Center = CenterMapping(player.MountedCenter, StickOffset, 0) + new Vector2(0, RaiseMaxHeight/2f-RaiseHeight);
            Projectile.spriteDirection = FixedDirection;
            // Projectile.velocity = player.velocity;
            
            if(SwitchFlag)
            {
                float DustSpd = 3f;
                for (float ang = -ModGlobal.PI_FLOAT; ang <= ModGlobal.PI_FLOAT; ang += ModGlobal.PI_FLOAT / 8f)
                {
                    Dust dust;
                    Vector2 position = player.Center;
                    Vector2 DustVel = new Vector2(1, 0).RotatedBy(ang) * DustSpd;
                    dust = Terraria.Dust.NewDustPerfect(position, FULLY_CHARGED_DUST, DustVel, 0, new Color(255,255,255), 1.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                }
                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                State = PLANT_STATE;
                SwitchFlag = false;
                Initialized = false;
            }
        }

        protected void PlantAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                Projectile.timeLeft = PLANT_EXIST_DURATION;
                FixedDirection = player.direction;
                Projectile.tileCollide = true;
                Projectile.friendly = false;

                // reset buff
                player.AddBuff(ENHANCE_BUFF_ID, ENHANCE_BUFF_DURATION);

                // Main.NewText("PlantAI:"+Projectile.identity);
                Initialized = true;

                HasPlayedOnGroundSound = false;
            }

            // apply gravity
            // long timestamp = Main.GameUpdateCount;
            // Main.NewText("[+"+timestamp+"]"+"Add gravity");
            MinionAIHelper.ApplyGravity(Projectile, GRAVITY, MAX_FALL_SPEED);

            if (OnGroundCnt == 1 && !HasPlayedOnGroundSound)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                HasPlayedOnGroundSound = true;
            }

            if (OnGroundCnt >= 10)
            {
                if (!SentryRecallInitialized)
                {
                    // find affected sentries
                    SentryRecallInfos.Clear();
                    foreach (var proj in Main.projectile)
                    {
                        if (proj.active && proj.owner == Projectile.owner && proj.sentry && (proj.Center - Projectile.Center).Length() <= SENTRY_RECALL_MAX_DIST)
                        {
                            SentryRecallInfo info = new SentryRecallInfo()
                            {
                                ID = proj.identity,
                                TileCollide = proj.tileCollide,
                                TargetPos = proj.Center,
                                Seed = Main.rand.NextFloat(-SENTRY_RANDOM_OFFSET, SENTRY_RANDOM_OFFSET)
                            };
                            SentryRecallInfos.Add(info);
                            // make sentry can go through tile
                            if(!USE_CUSTOM_SENTRY_RECALL) proj.tileCollide = false;
                        }
                    }
                    // set target pos
                    int SentryCount = SentryRecallInfos.Count;
                    float TotalLength = SentryCount == 0 ? 0f : (float)Math.Sqrt(SentryCount - 1) * 200f;
                    Random random = new Random();
                    int ranDir = random.Next(2) == 0 ? 1 : -1;
                    for (int i = 0; i < SentryCount; i++)
                    {
                        SentryRecallInfo info = SentryRecallInfos[i];
                        float LocalX = SentryCount == 1 ? 0f : (float)i / (float)(SentryCount - 1) * TotalLength - TotalLength / 2f;
                        float PreciseX = Projectile.Center.X + LocalX * ranDir;
                        float PreciseY = Projectile.Center.Y - SENTRY_RECALL_TARGET_OFFSET;
                        float X = PreciseX + info.Seed;
                        float Y = PreciseY + info.Seed;
                        int SentryWidth = Main.projectile[info.ID].width;
                        int SentryHeight = Main.projectile[info.ID].height;
                        Vector2 PossiblePos = MinionAIHelper.SearchForValidPosition(new Vector2(X, Y), (int)(SentryWidth * 1.5), (int)(SentryHeight * 1.5), 10);
                        SentryRecallInfos[i].TargetPos = PossiblePos;

                        // Main.NewText("Sentry "+i+" target pos: "+info.TargetPos);
                    }

                    // reset buff
                    player.AddBuff(ENHANCE_BUFF_ID, ENHANCE_BUFF_DURATION);

                    SentryRecallInitialized = true;
                }
                foreach (var info in SentryRecallInfos)
                {
                    if (USE_CUSTOM_SENTRY_RECALL)
                    {
                        CustomSentryRecall(info);
                    }
                    else
                    {
                        // move sentries
                        var sentry = Main.projectile[info.ID];
                        Vector2 ToTargetDist = info.TargetPos - sentry.Center;
                        if (ToTargetDist.Length() >= SENTRY_RECALL_THRESHOLD && !info.IsRecalled)
                        {
                            Vector2 ToTargetDir = ToTargetDist.SafeNormalize(Vector2.UnitX);
                            float DecayFactor = MathHelper.Clamp(ToTargetDist.Length() / SENTRY_RECALL_DECAY_DIST, 0.1f, 1f);
                            // float DecayFactor = MathHelper.Clamp(2f-SENTRY_RECALL_DECAY_DIST/(ToTargetDist.Length()+0.0001f), 0f, 1f);
                            sentry.velocity = ToTargetDir * SENTRY_RECALL_SPEED * DecayFactor;
                            // sentry.velocity = Vector2.Zero;
                            // sentry.Center += ToTargetDir * SENTRY_RECALL_SPEED * DecayFactor;
                        }
                        else
                        {
                            // reset sentry tile collide
                            sentry.tileCollide = info.TileCollide;
                            info.IsRecalled = true;
                        }
                    }
                }
            }

            // if the planted flag is too far away from player, kill self
            Vector2 ToOwnerDist = player.Center - Projectile.Center;
            if (ToOwnerDist.Length() >= 4000f)
            {
                Projectile.Kill();
            }

            if (SwitchFlag)
            {
                State = RECALL_STATE;
                SwitchFlag = false;
                Initialized = false;
                HasPlayedOnGroundSound = false;

                foreach (var info in SentryRecallInfos)
                {
                    Projectile sentry = Main.projectile[info.ID];
                    sentry.velocity /= 2f;
                }
            }
        }
        
        protected virtual void CustomSentryRecall(SentryRecallInfo info)
        {
            // raise error if not implemented
            throw new NotImplementedException("CustomSentryRecall is not implemented");
        }

        protected void RecallAI(Player player)
        {
            if (!Initialized)
            {
                // initialize
                Projectile.friendly = true;
                Projectile.tileCollide = false;
                Projectile.timeLeft = TIME_LEFT_RECALL;
                Projectile.localNPCHitCooldown = Projectile.timeLeft;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.ownerHitCheck = false;

                Initialized = true;
            }

            Vector2 RecallDist = player.Center - Projectile.Center;
            Vector2 RecallDirection = RecallDist.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = RecallDirection * RECALL_SPEED;
            // Projectile.Center += RecallDirection * RECALL_SPEED;
            Projectile.rotation += RECALL_ROTATE_SPEED * Projectile.spriteDirection;

            RecallTime++;
            if(RecallTime >= RECALL_SOUND_INTERVAL)
            {
                RecallTime = 0;
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
            }

            if(RecallDist.Length() <= 100f)
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D flagTexture = ModContent.Request<Texture2D>(FLAGPOLE_TEXTURE_PATH).Value;
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

            // draw flag cloth
            Vector2 FlagOffset = new Vector2(-FLAG_WIDTH / 2f * Projectile.spriteDirection, (FLAG_HEIGHT - Projectile.height) / 2f);
            Vector2 FlagOffsetEx = new Vector2(-2f + 1f * Projectile.spriteDirection, 0f);
            FlagOffset += FlagOffsetEx;
            Vector2 ClothCenter = Projectile.Center + FlagOffset.RotatedBy(Projectile.rotation);
            PreDrawFlagCloth(ref lightColor, ClothCenter);

            return false;
        }

        protected void PreDrawFlagCloth(ref Color lightColor, Vector2 ClothCenter)
        {
            Player player = Main.player[Projectile.owner];
            if(State == WAVE_STATE || State == RECALL_STATE)
            {
                UseFastAnimation = true;
            }
            else
            {
                UseFastAnimation = false;
            }

            if(State == RAISE_STATE || State == PLANT_STATE)
            {
                // do not draw tail when raise or plant
                TailEnabled = false;
            }
            else
            {
                TailEnabled = true;
            }
            if(TailEnabled && TAIL_ENABLE_GLOBAL)
            {
                SpriteBatch sb = Main.spriteBatch;
                GraphicsDevice gd = Main.graphics.GraphicsDevice;
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, 
                                        BlendState.AlphaBlend, 
                                        SamplerState.AnisotropicClamp, 
                                        DepthStencilState.None, 
                                        RasterizerState.CullNone, 
                                        null, 
                                        Main.GameViewMatrix.
                                        TransformationMatrix);

                List<Vertex> ve = new List<Vertex>();
                int tailLength;
                float offset_X_1, offset_Y_1, offset_X_2, offset_Y_2, offset_rot_1, offset_rot_2;
                Color tailColor;
                Vector2 SpinCenter;
                if(TAIL_DYNAMIC_DEBUG)
                {
                    tailLength = (int)DynamicParamManager.Get("TailLength").value;
                    offset_X_1 = (float)DynamicParamManager.Get("Offset_X.1").value;
                    offset_Y_1 = (float)DynamicParamManager.Get("Offset_Y.1").value;
                    offset_X_2 = (float)DynamicParamManager.Get("Offset_X.2").value;
                    offset_Y_2 = (float)DynamicParamManager.Get("Offset_Y.2").value;
                    offset_rot_1 = (float)DynamicParamManager.Get("Offset_rot.1").value;
                    offset_rot_2 = (float)DynamicParamManager.Get("Offset_rot.2").value;
                    tailColor = new Color((int)DynamicParamManager.Get("TailColor.R").value, (int)DynamicParamManager.Get("TailColor.G").value, (int)DynamicParamManager.Get("TailColor.B").value, (int)DynamicParamManager.Get("TailColor.A").value);
                    SpinCenter = ClothCenter + new Vector2(DynamicParamManager.Get("SpinCenterOffset.X").value*Projectile.spriteDirection, DynamicParamManager.Get("SpinCenterOffset.Y").value).RotatedBy(Projectile.rotation + DynamicParamManager.Get("SpinCenterOffset.Rot").value);
                }
                else
                {
                    tailLength = TAIL_LENGTH;
                    offset_X_1 = TAIL_OFFSET_X_1;
                    offset_Y_1 = TAIL_OFFSET_Y_1;
                    offset_X_2 = TAIL_OFFSET_X_2;
                    offset_Y_2 = TAIL_OFFSET_Y_2;
                    offset_rot_1 = TAIL_OFFSET_ROT_1;
                    offset_rot_2 = TAIL_OFFSET_ROT_2;
                    tailColor = TAIL_COLOR;
                    SpinCenter = ClothCenter + SPIN_CENTER_OFFSET.RotatedBy(Projectile.rotation + SPIN_CENTER_OFFSET_ROT);
                }

                offset_X_1 *= Projectile.spriteDirection;
                offset_X_2 *= Projectile.spriteDirection;

                SpinCenter = player.Center;


                int CurrentTime = (int)(TIME_LEFT_WAVE / AttackSpeed) - Projectile.timeLeft;
                if(State == RECALL_STATE) CurrentTime = TIME_LEFT_RECALL - Projectile.timeLeft;
                int OldPosSize = (int)Math.Min(CurrentTime, tailLength);
                for(int i = 0; i < OldPosSize;i++)
                {
                    float ratio = i / (float)tailLength;
                    float color_rate = MathHelper.Clamp(ratio*3, 0, 1);
                    Color b = tailColor;

                    // SpinCenter = Projectile.oldPos[i] + new Vector2(0, PoleLength/2);
                    Vector2 UpperVertexPffset, LowerVertexPffset;
                    if (State == WAVE_STATE)
                    {
                        Vector2 StickOffset = new Vector2(0, StickOffsetList[Math.Max(0,StickOffsetList.Count - i - 5)]);
                        SpinCenter = CenterMapping(Projectile.Center, StickOffset, Projectile.rotation + ModGlobal.PI_FLOAT);
                        UpperVertexPffset = new Vector2(-FLAG_WIDTH * 0.9f * Projectile.spriteDirection, -(PoleLength / 2f) * 0.95f) + StickOffset;
                        LowerVertexPffset = new Vector2(-FLAG_WIDTH * 0.9f * Projectile.spriteDirection, (-PoleLength / 2f + FLAG_HEIGHT) * 0.95f) + StickOffset;
                    }
                    else
                    {
                        SpinCenter = Projectile.Center;
                        UpperVertexPffset = new Vector2(-FLAG_WIDTH * 0.9f * Projectile.spriteDirection, -PoleLength / 2f * 0.95f);
                        LowerVertexPffset = new Vector2(-FLAG_WIDTH * 0.9f * Projectile.spriteDirection, (-PoleLength / 2f + FLAG_HEIGHT) * 0.95f);
                    }

                    ve.Add(new Vertex(SpinCenter - Main.screenPosition + UpperVertexPffset.RotatedBy(Projectile.oldRot[i]+offset_rot_1),
                            new Vector3(ratio, 1, 1),
                            b));
                    ve.Add(new Vertex(SpinCenter - Main.screenPosition + LowerVertexPffset.RotatedBy(Projectile.oldRot[i]+offset_rot_2),
                            new Vector3(ratio, 0, 1),
                            b));

                }

        
                if(ve.Count >= 3)//因为顶点需要围成一个三角形才能画出来 所以需要判顶点数>=3 否则报错
                {
                    gd.Textures[0] = ModContent.Request<Texture2D>(FLAG_TAIL_TEXTURE_PATH).Value;//获取刀光的拖尾贴图
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, ve.ToArray(), 0, ve.Count - 2);//画
                }

                //结束顶点绘制
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // draw flag
            Texture2D flagTexture = ModContent.Request<Texture2D>(FLAG_CLOTH_TEXTURE_PATH).Value;
            int width = flagTexture.Width;
            int height = flagTexture.Height;
            Vector2 origin = new Vector2(width / 2 * Projectile.spriteDirection, height / 2);
            Vector2 drawPos = ClothCenter - Main.screenPosition;
            FlagClothAmplitude = UseFastAnimation ? FAST_WAVE_AMPLITUDE : SLOW_WAVE_AMPLITUDE;
            FlagClothWaveSpeed = UseFastAnimation ? FAST_WAVE_SPEED_FACTOR : SLOW_WAVE_SPEED_FACTOR;

            float time = Main.GameUpdateCount / FlagClothWaveSpeed;
            if (State == WAVE_STATE || State == RECALL_STATE)
                time = 1.85f;
            int sliceWidth = WAVE_SLICE_WIDTH;
            int sliceCount = width / sliceWidth;

            for (int i = 0; i < sliceCount; i++)
            {
                int sliceX = i * sliceWidth;
                Rectangle sliceRect = new Rectangle(sliceX, 0, sliceWidth, height);
                float RealAmplitude = FlagClothAmplitude * ((float)sliceCount - (float)(i)) / (float)sliceCount;
                float wave = sliceX < width ? (float)Math.Sin((i / 6f) + time) * RealAmplitude : 0;

                Vector2 LocalPos = new Vector2(sliceX * Projectile.spriteDirection, wave);
                Vector2 WorldPos = ClothCenter + LocalPos.RotatedBy(Projectile.rotation) - Main.screenPosition;

                Main.spriteBatch.Draw(
                    flagTexture,
                    WorldPos,
                    sliceRect,
                    lightColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    0f
                );
            }
        }

        protected void DrawPart(Texture2D texture, Vector2 worldPos, Rectangle rect, Color color, Vector2 origin)
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
            Projectile.velocity.X = 0f;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            height = PoleLength/3;
            fallThrough = false;
            // hitboxCenterFrac = new Vector2(0.5f, -(PoleLength / height-1) / 2f);
            hitboxCenterFrac = new Vector2(0.5f, (float)((height - PoleLength/2f) / height));
            return true;
        }

        public override void Kill(int timeLeft)
        {
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
            return false;
        }

        protected void DamageGrassAlongBlade(Player player)
        {
            if ((State == RAISE_STATE || State == PLANT_STATE) ||
                (State == WAVE_STATE && Projectile.timeLeft >= (int)(TIME_LEFT_WAVE / AttackSpeed) - 1) ||
                (State == RECALL_STATE && Projectile.timeLeft >= TIME_LEFT_RECALL - 1)) return;
            Vector2 CurrentPoleTip = Projectile.Center + new Vector2(0, PoleLength / 2f).RotatedBy(Projectile.rotation + Math.PI);
            Vector2 OldPoleTip = Projectile.oldPos[1] + new Vector2(0, PoleLength / 2f).RotatedBy(Projectile.oldRot[0] + Math.PI) + new Vector2(0, PoleLength / 2f);
            float x1 = CurrentPoleTip.X, y1 = CurrentPoleTip.Y;
            float x2 = OldPoleTip.X, y2 = OldPoleTip.Y;
            float x3 = player.Center.X, y3 = player.Center.Y;
            float SearchMaxX = (x1 > x2 && x3 > x2) ? x1 : (x2 > x3) ? x2 : x3;
            float SearchMinX = (x1 < x2 && x3 < x2) ? x1 : (x2 < x3) ? x2 : x3;
            float SearchMaxY = (y1 > y2 && y3 > y2) ? y1 : (y2 > y3) ? y2 : y3;
            float SearchMinY = (y1 < y2 && y3 < y2) ? y1 : (y2 < y3) ? y2 : y3;

            if (TILE_CUT_RANGE_DEBUG)
            {
                Dust.QuickDustLine(CurrentPoleTip, OldPoleTip, 10f, Color.Red);
                Dust.QuickDustLine(player.Center, CurrentPoleTip, 10f, Color.Green);
                Dust.QuickDustLine(player.Center, OldPoleTip, 10f, Color.Blue);
                Dust.QuickDust(Projectile.oldPos[1] + new Vector2(0, PoleLength / 2f), Color.Yellow);

                // Main.NewText("SearchMinX: " + SearchMinX + " SearchMaxX: " + SearchMaxX + " SearchMinY: " + SearchMinY + " SearchMaxY: " + SearchMaxY + " CurrentPoleTip: " + CurrentPoleTip + " OldPoleTip: " + OldPoleTip);
            }

            int cnt = 0;
            for (float x = SearchMinX; x <= SearchMaxX; x += 16f)
            {
                for (float y = SearchMinY; y <= SearchMaxY; y += 16f)
                {
                    int tileX = (int)(x / 16f);
                    int tileY = (int)(y / 16f);

                    // 检查 tile 是否存在且可被破坏
                    if (WorldGen.InWorld(tileX, tileY))
                    {
                        Tile tile = Framing.GetTileSafely(tileX, tileY);

                        cnt++;

                        // 只破坏草、瓶子、罐子之类（frame重要）
                        if (tile.HasTile && Main.tileCut[tile.TileType])
                        {
                            WorldGen.KillTile(tileX, tileY, false, false, true);
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileX, tileY);
                            }
                        }
                    }
                }
            }
            // Main.NewText("DamageGrassAlongBlade: " + cnt);
        }

        protected virtual void CreateDustEffect(Player player)
        {
            // create dust when using imbue flasks
            if (State == WAVE_STATE || State == RECALL_STATE)
            {
                int dustType = -1;
                if (MinionAIHelper.IsPartyImbue(player))
                {
                    dustType = Main.rand.Next(4) + 139;
                }
                else
                {
                    dustType = MinionAIHelper.GetImbueDust(player);
                }
                if (dustType != -1 && Main.rand.NextFloat() < 0.75f)
                {
                    Vector2 DustCenter = Projectile.Center + new Vector2(0, -(PoleLength - FLAG_HEIGHT) / 2f).RotatedBy(Projectile.rotation);
                    Dust dust = Dust.NewDustDirect(DustCenter - new Vector2(FLAG_HEIGHT / 2f, FLAG_HEIGHT / 2f), FLAG_HEIGHT, FLAG_HEIGHT, dustType, 0f, 0f, 0, default, 2f);
                    dust.noGravity = true;
                    dust.velocity = (DustCenter - Projectile.Center).SafeNormalize(Vector2.Zero) * MinionAIHelper.RandomFloat(0f, 3f);
                    dust.fadeIn = 1f;

                }
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(NPC_DEBUFF_ID, NPC_DEBUFF_DURATION);

            Player player = Main.player[Projectile.owner];
            player.MinionAttackTargetNPC = target.whoAmI;
            // player.HasMinionAttackTargetNPC = true;

            int ImbueDeBuffID = MinionAIHelper.GetImbueDebuff(player);
            if (ImbueDeBuffID != -1) target.AddBuff(ImbueDeBuffID, 3 * 60);
            if(MinionAIHelper.IsPartyImbue(player))
            {
                Projectile.NewProjectile(new EntitySource_Misc("WeaponEnchantment_Confetti"), target.Center.X, target.Center.Y, target.velocity.X, target.velocity.Y, 289, 0, 0f, player.whoAmI);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = (target.Center - player.Center).X > 0 ? 1 : -1;

            float multiplier = (float)Math.Pow(DAMAGE_DECAY_FACTOR, hitCount);

            modifiers.FinalDamage *= multiplier;

            hitCount++;
        }

        protected Vector2 ConvertToWorldPos(Vector2 localPos)
        {
            return Projectile.Center + localPos.RotatedBy(Projectile.rotation) - Main.screenPosition;
        }

        protected Vector2 CenterMapping(Vector2 center, Vector2 offset, float rotation)
        {
            return center + offset.RotatedBy(rotation);
        }
    }
}