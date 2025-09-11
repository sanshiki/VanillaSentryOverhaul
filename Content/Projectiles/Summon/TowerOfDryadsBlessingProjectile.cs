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
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class TowerOfDryadsBlessingProjectile : ModProjectile
    {
        // animation parameters
        private const int FRAME_SPEED = 10;
        private const int FRAME_NUM = 5;
        private const int LIVE_TIME = 40*10;
        private const int FIRST_STAGE = (int)(0.167 * LIVE_TIME);
        private const int SECOND_STAGE = (int)(0.333 * LIVE_TIME + FIRST_STAGE);
        private const int THIRD_STAGE = (int)(0.333 * LIVE_TIME + SECOND_STAGE);
        private const int FOURTH_STAGE = (int)(0.167 * LIVE_TIME + THIRD_STAGE);
        private const int FADE_TIME = 60;

        // projectile state
        private bool initialized = false;
        public Vector2 RotateCenter;
        public float RotateRadius;
        public float RotateAngle;
        public float RotateSpeed;
        private static int BuffProjectileID = -1;

        // public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DryadsWardCircle;
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.DryadsWardCircle;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FRAME_NUM;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14; // 尺寸可以和沙块差不多
            Projectile.height = 14;
            Projectile.aiStyle = 0; 
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = 1; // 击中一次就消失
            Projectile.timeLeft = LIVE_TIME; // 存活时间
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.alpha = 255;

        }

        public override void AI()
        {
            if (!initialized)
            {
                Initialize();
            }
            
            // fade in and out
            int alpha = Projectile.alpha;
            if (Projectile.timeLeft > LIVE_TIME - FADE_TIME)
            {
                alpha -= (int)(255 / (float)FADE_TIME);
            }
            else if (Projectile.timeLeft < FADE_TIME)
            {
                alpha += (int)(255 / (float)FADE_TIME);
            }

            CalculateRadius();
            RePosition();
            UpdateAnimation(alpha);
        }

        private void Initialize()
        {
            RotateCenter = Projectile.Center;
            RotateRadius = Projectile.damage;
            RotateAngle = Projectile.velocity.Y;
            RotateSpeed = (float)(Projectile.knockBack)/100f;
            initialized = true;
        }

        private void RePosition()
        {
            Projectile.Center = RotateCenter + new Vector2(RotateRadius, 0).RotatedBy(RotateAngle);
            RotateAngle += RotateSpeed;
        }

        private void UpdateAnimation(int alpha)
        {
            // cycle through the frames
            Projectile.frame = Projectile.frameCounter / FRAME_SPEED % FRAME_NUM;
            Projectile.frameCounter++;
            
            Projectile.alpha = alpha > 255 ? 255 : alpha < 0 ? 0 : alpha;

            // add rotation
            Projectile.rotation = RotateAngle + MathHelper.PiOver2;

            // add dust
            Random ran = new Random();
            float rate = ran.Next(100) / 100.0f;
            if (rate < 0.01)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DryadsWard, 0f, 0f);
                Dust dust = Main.dust[dustIndex];
                dust.velocity = (RotateCenter - dust.position) * 0.038f;
                dust.noGravity = true;
            }
        }

        private void CalculateRadius()
        {
            // second stage: radius increase to double
            if (Projectile.timeLeft > LIVE_TIME - SECOND_STAGE && Projectile.timeLeft <= LIVE_TIME - FIRST_STAGE)
            {
                int tick = (LIVE_TIME - FIRST_STAGE) - Projectile.timeLeft;
                RotateRadius = Projectile.damage + tick * (Projectile.damage * 2 - Projectile.damage) / (float) (SECOND_STAGE - FIRST_STAGE);
                RotateRadius = RotateRadius > Projectile.damage * 2 ? Projectile.damage * 2 : RotateRadius;
            }
            // fourth stage: radius increase to four times
            else if (Projectile.timeLeft > LIVE_TIME - FOURTH_STAGE && Projectile.timeLeft <= LIVE_TIME - THIRD_STAGE)
            {
                int tick = (LIVE_TIME - THIRD_STAGE) - Projectile.timeLeft;
                RotateRadius = Projectile.damage * 2 + tick * (Projectile.damage * 4 - Projectile.damage * 2) / (float) (FOURTH_STAGE - THIRD_STAGE);
                RotateRadius = RotateRadius > Projectile.damage * 4 ? Projectile.damage * 4 : RotateRadius;
            }
        }

    }
}
