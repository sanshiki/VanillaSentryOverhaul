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
using SummonerExpansionMod.Initialization;


namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class FlagProjectile : ModProjectile
    {
        // animation constants
        private const float SLOW_WAVE_AMPLITUDE = 10f;
        private const float SLOW_WAVE_SPEED_FACTOR = 7f;

        private const float FAST_WAVE_AMPLITUDE = 10f;
        private const float FAST_WAVE_SPEED_FACTOR = 1.5f;

        private const int WAVE_SLICE_WIDTH = 4;

        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/FlagProjectile";

        public override string Texture => TEXTURE_PATH;

        // private variables
        private int FixedDirection = 1;
        private float AimAngle = 0f;
        private float Amplitude = SLOW_WAVE_AMPLITUDE;
        private float WaveSpeed = SLOW_WAVE_SPEED_FACTOR;
        private bool Initialized = false;
        private int RaiseTime = 0;
        private float ItemRot = -2.05f;
        private Dictionary<int, int> hitCountPerNPC = new Dictionary<int, int>();

        // public attributes
        public float WaveDirection = 1;
        public bool UseFastAnimation = false;

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 80;
            Projectile.friendly = false;
            Projectile.ownerHitCheck = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60 * 60 * 10; // 10 min
            Projectile.localNPCHitCooldown = Projectile.timeLeft;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if(UseFastAnimation)
            {
                Amplitude = FAST_WAVE_AMPLITUDE;
                WaveSpeed = FAST_WAVE_SPEED_FACTOR;
            }
            else
            {
                Amplitude = SLOW_WAVE_AMPLITUDE;
                WaveSpeed = SLOW_WAVE_SPEED_FACTOR;
            }
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
            Texture2D flagTexture = ModContent.Request<Texture2D>(TEXTURE_PATH).Value;
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
                float RealAmplitude = Amplitude * ((float)sliceCount - (float)(i)) / (float)sliceCount;
                float wave = sliceX < width ? (float)Math.Sin((i / 6f) + time) * RealAmplitude : 0;

                Vector2 LocalPos = new Vector2(sliceX * Projectile.spriteDirection, wave);
                Vector2 WorldPos = Projectile.Center + LocalPos.RotatedBy(Projectile.rotation) - Main.screenPosition;

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
    }

}