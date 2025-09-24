using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{


    public class HellpodSummonSignal : ModProjectile
    {
        private const int SIGNAL_WIDTH = 11;
        private const int SIGNAL_HEIGHT = 1000;
        private const int SIGNAL_SLICE_HEIGHT = 4;
        private const int SIGNAL_BASE_HEIGHT = 10;

        private const int SIGNAL_TIME = 60*4;

        private Vector3 LIGHT_RGB = new Vector3(1f, 1f, 1f);
        private const float LIGHT_STRENGTH = 6.0f;

        private bool initialized = false;

        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/HellpodSummonSignal";

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
        }

        public override void AI()
        {
            // adjust signal position
            if(!initialized)
            {
                Projectile.rotation = (float)Math.PI;
                initialized = true;
            }

            // add light effect
            Lighting.AddLight(Projectile.Center + new Vector2(0, SIGNAL_HEIGHT/2f), new Vector3(0.2f, 0.8f, 2.0f) * LIGHT_STRENGTH);  // base part
            for(int i = 0; i < SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT; i++)
            {
                int repeatY = SIGNAL_BASE_HEIGHT + i * SIGNAL_SLICE_HEIGHT;
                Lighting.AddLight(Projectile.Center + new Vector2(0, SIGNAL_HEIGHT/2f) - new Vector2(0, repeatY), LIGHT_RGB * LIGHT_STRENGTH);
            }
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
            DrawPart(texture, ConvertToWorldPos(new Vector2(0, 0)), basePart, Color.White, origin);
    
            

            // draw repeat part
            for(int i = 0; i < SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT; i++)
            {
                float alpha = 1f - (float)i / (SIGNAL_HEIGHT / SIGNAL_SLICE_HEIGHT);
                int repeatY = SIGNAL_BASE_HEIGHT + i * SIGNAL_SLICE_HEIGHT;
                // Main.NewText("repeatY: " + repeatY + " alpha: " + alpha);
                Rectangle slicePart = new Rectangle(0, SIGNAL_BASE_HEIGHT, width, SIGNAL_SLICE_HEIGHT);
                Vector2 repeatLocalPos = new Vector2(0, repeatY);
                Vector2 repeatWorldPos = ConvertToWorldPos(repeatLocalPos);
                DrawPart(texture, repeatWorldPos, slicePart, Color.White * alpha, origin);
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