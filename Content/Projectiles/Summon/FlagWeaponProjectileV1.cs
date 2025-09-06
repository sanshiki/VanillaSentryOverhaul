using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;

using SummonerExpansionMod.Initialization;
using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Items.Weapons.Summon;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class FlagWeaponProjectileV1 : ModProjectile
    {
        // animation constants
        // waving
        private Vector2 STICK_OFFSET = new Vector2(-55f, -144f);
        private const float ROT_ANGLE = 150f * ModGlobal.PI_FLOAT / 180f;
        private const int TIME_LEFT_WAVE = 24;
        private const int WAVE_DEAD_ZONE = 20;
        private const int WAVE_SLICE_WIDTH = 4;
        private const float WAVE_AMPLITUDE = 10f;
        private const float WAVE_SPEED_FACTOR = 1.5f;

        // raising
        private const float RAISE_MAX_HEIGHT = 16f * 4f; // 4 tiles
        private const int TIME_LEFT_RAISE = 60;
        private const float RAISE_MAX_SPEED = 2f * RAISE_MAX_HEIGHT / (float)TIME_LEFT_RAISE;
        private const float RAISE_ACC = RAISE_MAX_SPEED / (float)TIME_LEFT_RAISE * 2f;
        private const float RAISE_WAVE_AMPLITUDE = 10f;
        private const float RAISE_WAVE_SPEED_FACTOR = 7f;

        // plant
        private const int PLANT_EXIST_DURATION = 60*60*10; // 10 min
        private const float GRAVITY = 0.5f;
        private const float MAX_FALL_SPEED = 20f;

        // recall
        private const int RECALL_EXIST_DURATION = 60*60; // 1 min
        private const float RECALL_SPEED = 30f;
        private const float RECALL_ROTATE_SPEED = 0.3f;

        // state constants
        public const int WAVE_STATE = 0; // left-click: wave
        public const int RAISE_STATE = 1; // right-short-press: raise
        public const int PLANT_STATE = 2; // right-long-press: plant
        public const int RECALL_STATE = 3; // right-click after plant: recall

        // private variables
        private int FixedDirection = 1;
        private float AimAngle = 0f;
        private float Amplitude = WAVE_AMPLITUDE;
        private float WaveSpeed = WAVE_SPEED_FACTOR;
        private bool Initialized = false;
        private int RaiseTime = 0;
        private float ItemRot = -2.05f;
        private Dictionary<int, int> hitCountPerNPC = new Dictionary<int, int>();

        // public attributes
        public float WaveDirection = 1;
        public int State = WAVE_STATE;
        public bool SwitchFlag = false;
        public int OnGroundCnt = 0;

        public override void SetDefaults()
        {
            Projectile.width = 134;
            Projectile.height = 281;
            Projectile.friendly = true;
            Projectile.ownerHitCheck = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TIME_LEFT_WAVE;
            Projectile.localNPCHitCooldown = Projectile.timeLeft;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

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
                Amplitude = WAVE_AMPLITUDE;
                WaveSpeed = WAVE_SPEED_FACTOR;
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
                Amplitude = RAISE_WAVE_AMPLITUDE;
                WaveSpeed = RAISE_WAVE_SPEED_FACTOR;
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

            Vector2 StickOffset = new Vector2(STICK_OFFSET.X * FixedDirection, STICK_OFFSET.Y);
            Projectile.Center = CenterMapping(player.MountedCenter, StickOffset, 0) + new Vector2(0, RAISE_MAX_HEIGHT/2f-RaiseHeight);
            Projectile.spriteDirection = FixedDirection;
            
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
                Amplitude = RAISE_WAVE_AMPLITUDE;
                WaveSpeed = RAISE_WAVE_SPEED_FACTOR;
                FixedDirection = player.direction;
                Projectile.tileCollide = true;
                Projectile.friendly = false;
                // Main.NewText("PlantAI:"+Projectile.identity);
                Initialized = true;
            }

            // apply gravity
            Projectile.velocity.X = 0f;
            Projectile.velocity.Y += GRAVITY;
            if(Projectile.velocity.Y > MAX_FALL_SPEED)
            {
                Projectile.velocity.Y = MAX_FALL_SPEED;
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
                Amplitude = WAVE_AMPLITUDE;
                WaveSpeed = WAVE_SPEED_FACTOR;
                Projectile.friendly = true;
                Projectile.tileCollide = false;
                Projectile.timeLeft = RECALL_EXIST_DURATION;
                Projectile.localNPCHitCooldown = Projectile.timeLeft;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.ownerHitCheck = false;
            }

            Vector2 RecallDist = player.Center - Projectile.Center;
            Vector2 RecallDirection = RecallDist.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = RecallDirection * RECALL_SPEED;
            Projectile.rotation += RECALL_ROTATE_SPEED * Projectile.spriteDirection;

            if(RecallDist.Length() <= 100f)
            {
                Projectile.Kill();
            }
        }

        private Vector2 CenterMapping(Vector2 center, Vector2 offset, float rotation)
        {
            return center + offset.RotatedBy(rotation);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int npcID = target.whoAmI;

            // 统计同一次动作中命中的次数
            if (!hitCountPerNPC.ContainsKey(npcID))
                hitCountPerNPC[npcID] = 0;

            hitCountPerNPC[npcID]++;

            // 衰减系数：第一次全额，第二次0.5，第三次0.25...
            float multiplier = (float)Math.Pow(0.5f, hitCountPerNPC[npcID] - 1);

            modifiers.FinalDamage *= multiplier;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D flagTexture = ModContent.Request<Texture2D>("SummonerExpansionMod/Content/Projectiles/Summon/FlagWeaponProjectileV1").Value;
            int width = flagTexture.Width;
            int height = flagTexture.Height;
            Vector2 origin = new Vector2(width / 2 * Projectile.spriteDirection, height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount / WaveSpeed;
            // Main.NewText("WaveSpeed:"+WaveSpeed);
            int sliceWidth = WAVE_SLICE_WIDTH;
            int sliceCount = width / sliceWidth;

            for (int i = 0; i < sliceCount; i++)
            {
                int sliceX = i * sliceWidth;
                Rectangle sliceRect = new Rectangle(sliceX, 0, sliceWidth, height);
                float wave = sliceX < width - WAVE_DEAD_ZONE ? (float)Math.Sin((i / 6f) + time) * Amplitude : 0;

                Vector2 LocalPos = new Vector2(sliceX * Projectile.spriteDirection, wave);
                Vector2 StickOffset = new Vector2(STICK_OFFSET.X * FixedDirection, STICK_OFFSET.Y);
                Vector2 WorldPos = Projectile.Center/*  + StickOffset.RotatedBy(Projectile.rotation) */ + LocalPos.RotatedBy(Projectile.rotation) - Main.screenPosition;

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
            return false; // 阻止默认绘制
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnGroundCnt++;
            return false;
        }
    }

}