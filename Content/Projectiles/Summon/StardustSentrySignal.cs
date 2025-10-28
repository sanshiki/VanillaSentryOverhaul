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
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{


    public class StardustSentrySignal : ModProjectile
    {
        private const int SIGNAL_WIDTH = 11;
        private const int SIGNAL_HEIGHT = 1000;
        private const int SIGNAL_SLICE_HEIGHT = 4;
        private const int SIGNAL_BASE_HEIGHT = 10;
        private const int SIGNAL_TIME = 60;
        private const float FADEIN_TIME_RATE = 0.25f;
        private const float FADEOUT_TIME_RATE = 0.1f;
        private const int BRIGHT_SLICE_NUM = 18;
        private const float BRIGHT_ALPHA_MAX = 9f;
        private const float GLOBAL_ALPHA = 0.25f;
        private const int DUST_EMIT_INTERVAL = 5;
        private const int ELLIPSE_DUST_NUM = 3;
        private Vector3 LIGHT_RGB = new Vector3(1f, 1f, 1f);
        private const float LIGHT_STRENGTH = 6.0f;
        private bool initialized = false;
        private bool hasPlayedSound = false;
        private float GlobalAlphaMultiplier = 1.0f;
        private float AnimationRatio = 0.0f;
        private int ellipseDustTimer = 0;
        private int ellipseDustCnt = 0;
        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/StardustSentrySignal";
        public override string Texture => TEXTURE_PATH;
        public override void SetDefaults()
        {
            // Projectile.CloneDefaults(ProjectileID.PhantasmalDeathray); 
            Projectile.aiStyle = -1;
            Projectile.hostile = false;
            Projectile.friendly = false; 
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = SIGNAL_TIME;
            Projectile.width = SIGNAL_WIDTH;
            Projectile.height = SIGNAL_HEIGHT;
            Projectile.alpha = 150;

            DynamicParamManager.Register("Global alpha", GLOBAL_ALPHA, 0f, 1f);
            DynamicParamManager.Register("Bright slice num", BRIGHT_SLICE_NUM, 0, SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT);
            DynamicParamManager.Register("Bright alpha max", BRIGHT_ALPHA_MAX, 1f, 20f);
        }

        public override void AI()
        {
            // adjust signal position
            if (!initialized)
            {
                Projectile.rotation = (float)Math.PI;
                initialized = true;
            }

            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            int FadeInTime = (int)(SIGNAL_TIME * FADEIN_TIME_RATE);
            int FadeOutTime = (int)(SIGNAL_TIME * FADEOUT_TIME_RATE);
            int AnimationTime = SIGNAL_TIME - FadeInTime - FadeOutTime;

            if (Projectile.timeLeft > SIGNAL_TIME - FadeInTime)  // fade in
            {
                GlobalAlphaMultiplier = (float)(SIGNAL_TIME - Projectile.timeLeft) / FadeInTime;
            }
            else if (Projectile.timeLeft >= SIGNAL_TIME - FadeInTime - AnimationTime) // animation
            {
                AnimationRatio = (float)(SIGNAL_TIME - Projectile.timeLeft - FadeInTime) / AnimationTime;
                GlobalAlphaMultiplier = 1f;
                if (!hasPlayedSound)
                {
                    Vector2 SoundOffset = new Vector2(0, SIGNAL_HEIGHT / 2f);
                    SoundEngine.PlaySound(MinionAIHelper.RandomBool() ? SoundID.Item67 : SoundID.Item68, Projectile.Center + SoundOffset);
                    hasPlayedSound = true;
                }
                ellipseDustTimer++;
                if(ellipseDustTimer >= DUST_EMIT_INTERVAL)
                {
                    ellipseDustTimer = 0;
                    ellipseDustCnt++;
                    if(ellipseDustCnt <= ELLIPSE_DUST_NUM)
                    {
                        float rate = (float)ellipseDustCnt / ELLIPSE_DUST_NUM;
                        float a = MathHelper.Lerp(1f, 0.6f, rate);
                        float e = 0.95f;
                        float c = e * a;
                        float b = (float)Math.Sqrt(a * a - c * c);
                        float offset_height = MathHelper.Lerp(8f, 56f, rate);
                        EmitEllipseDust(111, a, b, 20, Projectile.Center +new Vector2(0, SIGNAL_HEIGHT / 2f - offset_height), 0.6f, 0.9f);
                    }
                }
            }
            else  // fade out
            {
                GlobalAlphaMultiplier = (float)Projectile.timeLeft / FadeOutTime;
            }
            float globalAlpha = DynamicParamManager.Get("Global alpha").value;
            GlobalAlphaMultiplier *= globalAlpha;
        }
        
        private void EmitEllipseDust(int dust_id, float a, float b, int sample, Vector2 center, float min_scale, float max_scale)
        {
            for (int i = 0; i < sample; i++)
            {
                float theta = MathHelper.Lerp(-ModGlobal.PI_FLOAT, ModGlobal.PI_FLOAT, (float)i / sample);
                float x = a * (float)Math.Cos(theta);
                float y = b * (float)Math.Sin(theta);
                Dust dust;
                Vector2 DustVel = new Vector2(x, y);
                dust = Terraria.Dust.NewDustPerfect(center, dust_id, DustVel, 0, new Color(0, 67, 255), 1f);
                dust.scale = MathHelper.Lerp(min_scale, max_scale, ((float)Math.Sin(theta) + 1f) / 2f);
                // dust.shader = GameShaders.Armor.GetSecondaryShader(30, Main.LocalPlayer);

            }
        }
        
        private float GetAlpha(float CurrentSliceRatio)
        {
            int BrightSliceNum = (int)DynamicParamManager.Get("Bright slice num").value;
            float BrightAlphaMax = DynamicParamManager.Get("Bright alpha max").value;
            float BrightSlicePartRatio = (float)BrightSliceNum / (SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT);
            float MaxAnimationRatio = AnimationRatio;
            float MinAnimationRatio = Math.Max(0f, AnimationRatio - BrightSlicePartRatio);
            float MidRatio = (MinAnimationRatio + MaxAnimationRatio) / 2f;
            float BirghtAlphaMultiplier = 1f;
            if (CurrentSliceRatio > MinAnimationRatio && CurrentSliceRatio <= MidRatio)
            {
                BirghtAlphaMultiplier = MathHelper.Lerp(1f, BrightAlphaMax, (CurrentSliceRatio - MinAnimationRatio) / (MidRatio - MinAnimationRatio));
            }
            else if(CurrentSliceRatio > MidRatio && CurrentSliceRatio < MaxAnimationRatio)
            {
                BirghtAlphaMultiplier = MathHelper.Lerp(BrightAlphaMax, 1f, (CurrentSliceRatio - MidRatio) / (MaxAnimationRatio - MidRatio));
            }
            return (1f - CurrentSliceRatio) * GlobalAlphaMultiplier * BirghtAlphaMultiplier;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if(!initialized) return false;
            
            Player player = Main.player[Projectile.owner];
            Texture2D texture = ModContent.Request<Texture2D>(TEXTURE_PATH).Value;
            int width = SIGNAL_WIDTH;
            int height = SIGNAL_HEIGHT;
            int TextureHeight = texture.Height;
            Vector2 origin = new Vector2(width / 2, height / 2);

            // draw base part
            Rectangle basePart = new Rectangle(0, 0, width, SIGNAL_BASE_HEIGHT);
            DrawPart(texture, ConvertToWorldPos(new Vector2(0, 0)), basePart, Color.White * GlobalAlphaMultiplier, origin);
            Lighting.AddLight(Projectile.Center + new Vector2(0, SIGNAL_HEIGHT / 2f), new Vector3(0.2f, 0.8f, 2.0f) * GlobalAlphaMultiplier);  // base part
    
            

            // draw repeat part
            for(int i = 0; i < SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT; i++)
            {
                float SliceRatio = (float)i / (SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT);
                float alpha = GetAlpha(SliceRatio);
                int repeatY = SIGNAL_BASE_HEIGHT + i * SIGNAL_SLICE_HEIGHT;
                // Main.NewText("repeatY: " + repeatY + " alpha: " + alpha);
                Rectangle slicePart = new Rectangle(0, SIGNAL_BASE_HEIGHT, width, SIGNAL_SLICE_HEIGHT);
                Vector2 repeatLocalPos = new Vector2(0, repeatY);
                Vector2 repeatWorldPos = ConvertToWorldPos(repeatLocalPos);
                DrawPart(texture, repeatWorldPos, slicePart, Color.White * alpha, origin);
                Lighting.AddLight(Projectile.Center + new Vector2(0, SIGNAL_HEIGHT / 2f) - new Vector2(0, repeatY), LIGHT_RGB * alpha);
            }

            return false;
        }

        private Vector2 ConvertToWorldPos(Vector2 localPos)
        {
            return Projectile.Center + localPos.RotatedBy(Projectile.rotation) - Main.screenPosition;
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
    }
}