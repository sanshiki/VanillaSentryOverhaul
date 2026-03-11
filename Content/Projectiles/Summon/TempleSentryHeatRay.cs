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
using SummonerExpansionMod;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class TempleSentryHeatRay : ModProjectile
    {
        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Projectiles/TempleSentryHeatRay";
        public override string Texture => TEXTURE_PATH;

        private const int SIGNAL_WIDTH = 7;
        private const int SIGNAL_HEIGHT = 1500;
        private const int SIGNAL_SLICE_HEIGHT = 4;
        private const int SIGNAL_BASE_HEIGHT = 6;

        private Vector2 start, end;
        private const float LaserStep = 4f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.HeatRay);
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 50;
            Projectile.alpha = 150;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void OnSpawn(IEntitySource source)
        {
            start = Projectile.Center;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 HeatRayPos = Projectile.position;
            Vector2 HeatRayDir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            for(int i = 0; i < 10; i++)
            {
                Dust HeatRayDust = Dust.NewDustDirect(HeatRayPos, 12, 12, DustID.TheDestroyer);
                HeatRayDust.position = HeatRayPos;
                HeatRayDust.position.X += Projectile.width / 2;
                HeatRayDust.position.Y += Projectile.height / 2;
                HeatRayDust.scale = (float)Main.rand.Next(70, 110) * 0.008f;
                HeatRayDust.noGravity = true;
                float reflectDustAngle = MinionAIHelper.RandomFloat(-ModGlobal.PI_FLOAT/2f, ModGlobal.PI_FLOAT/2f);
                HeatRayDust.velocity = HeatRayDir.RotatedBy(ModGlobal.PI_FLOAT)*MinionAIHelper.RandomFloat(1f, 5f) + reflectDustAngle.ToRotationVector2();
                HeatRayDust.shader = GameShaders.Armor.GetSecondaryShader(90, Main.LocalPlayer);
            }
        }


        public override void AI()
        {
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 3f)
            {
                if (Main.rand.NextFloat() < 0.1f)
                {
                    Vector2 HeatRayPos = Projectile.position;
                    Projectile.alpha = 255;
                    Vector2 HeatRayDir = Projectile.velocity.SafeNormalize(Vector2.Zero);

                    // HeatRayPos -= Projectile.velocity * ((float)i * 0.25f);	
                    Dust HeatRayDust = Dust.NewDustDirect(HeatRayPos, 8, 8, DustID.TheDestroyer);   // 162
                    HeatRayDust.position = HeatRayPos;
                    HeatRayDust.position.X += Projectile.width / 2;
                    HeatRayDust.position.Y += Projectile.height / 2;
                    HeatRayDust.scale = (float)Main.rand.Next(70, 110) * 0.013f;
                    HeatRayDust.noGravity = true;
                    // HeatRayDust.velocity = HeatRayDir * 5f;
                    HeatRayDust.shader = GameShaders.Armor.GetSecondaryShader(90, Main.LocalPlayer);
                    // HeatRayDust
                    // HeatRayDust.fadeIn = 1.0f;
                }


                // if (Main.rand.NextFloat() < 0.5116279f)
                // {
                //     Dust dust;
                //     // You need to set position depending on what you are doing. You may need to subtract width/2 and height/2 as well to center the spawn rectangle.
                //     Vector2 position = HeatRayPos;
                //     dust = Terraria.Dust.NewDustPerfect(position, 173, HeatRayDir, 255, new Color(255,255,255), 2f);
                //     dust.shader = GameShaders.Armor.GetSecondaryShader(61, Main.LocalPlayer);
                //     dust.scale = (float)Main.rand.Next(70, 110) * 0.013f;
                //     dust.velocity = HeatRayDir * 2f;
                //     dust.fadeIn = 1.0f;
                // }

            }
        }

        public override void Kill(int timeLeft)
        {
            Vector2 HeatRayPos = Projectile.position;
            Vector2 HeatRayDir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            for(int i = 0; i < 10; i++)
            {
                Dust HeatRayDust = Dust.NewDustDirect(HeatRayPos, 12, 12, DustID.TheDestroyer);
                HeatRayDust.position = HeatRayPos;
                HeatRayDust.position.X += Projectile.width / 2;
                HeatRayDust.position.Y += Projectile.height / 2;
                HeatRayDust.scale = (float)Main.rand.Next(70, 110) * 0.008f;
                HeatRayDust.noGravity = true;
                float reflectDustAngle = MinionAIHelper.RandomFloat(-ModGlobal.PI_FLOAT/2f, ModGlobal.PI_FLOAT/2f);
                HeatRayDust.velocity = HeatRayDir.RotatedBy(ModGlobal.PI_FLOAT)*MinionAIHelper.RandomFloat(1f, 5f) + reflectDustAngle.ToRotationVector2();
                HeatRayDust.shader = GameShaders.Armor.GetSecondaryShader(90, Main.LocalPlayer);
            }
        }

        

        /// <summary>
        /// 沿 direction 从 start 射线检测，返回首次碰到物块的点；若未碰到则返回 start + direction * maxLength。
        /// </summary>
        private static Vector2 GetLaserEndOnTileCollision(Vector2 start, Vector2 direction, float maxLength)
        {
            Vector2 dir = direction.SafeNormalize(Vector2.Zero);
            if (dir == Vector2.Zero) return start;
            for (float d = LaserStep; d <= maxLength; d += LaserStep)
            {
                Vector2 p = start + dir * d;
                int tileX = (int)(p.X / 16f);
                int tileY = (int)(p.Y / 16f);
                if (!WorldGen.InWorld(tileX, tileY))
                    return start + dir * (d - LaserStep);
                Tile tile = Main.tile[tileX, tileY];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return start + dir * (d - LaserStep);
            }
            return start + dir * maxLength;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float LaserMaxLength = Projectile.velocity.Length() * 50f;
            Vector2 end = GetLaserEndOnTileCollision(Projectile.Center, Projectile.velocity, LaserMaxLength);
            float width = 12f;
            DrawLaserVertices(start, end, width, Color.Yellow, ModGlobal.MOD_TEXTURE_PATH + "Vertexes/LaserVertex");
            return false;
        }

        /// <summary>
        /// 用顶点绘制一条激光（横向纹理：U 沿起点→终点，V 沿宽度方向）。
        /// </summary>
        /// <param name="start">起点（世界坐标）</param>
        /// <param name="end">终点（世界坐标）</param>
        /// <param name="width">激光宽度（世界单位）</param>
        /// <param name="color">着色颜色，可为 null 表示白色不透明</param>
        /// <param name="texturePath">纹理路径，可为 null 使用默认 TEXTURE_PATH</param>
        public static void DrawLaserVertices(Vector2 start, Vector2 end, float width, Color? color = null, string texturePath = null)
        {
            Vector2 dir = (end - start);
            float len = dir.Length();
            if (len < 0.0001f) return;
            dir /= len;

            Vector2 perp = new Vector2(-dir.Y, dir.X);
            float halfWidth = width * 0.5f;
            Vector2 leftTop = start - halfWidth * perp;
            Vector2 leftBottom = start + halfWidth * perp;
            Vector2 rightTop = end - halfWidth * perp;
            Vector2 rightBottom = end + halfWidth * perp;

            Color drawColor = color ?? Color.White;
            List<Vertex> vertices = new List<Vertex>();
            // TriangleStrip 顺序：左上、左下、右上、右下 → UV (0,0),(0,1),(1,0),(1,1)
            vertices.Add(new Vertex(leftTop - Main.screenPosition, new Vector3(0f, 0f, 1f), drawColor));
            vertices.Add(new Vertex(leftBottom - Main.screenPosition, new Vector3(0f, 1f, 1f), drawColor));
            vertices.Add(new Vertex(rightTop - Main.screenPosition, new Vector3(1f, 0f, 1f), drawColor));
            vertices.Add(new Vertex(rightBottom - Main.screenPosition, new Vector3(1f, 1f, 1f), drawColor));

            SpriteBatch sb = Main.spriteBatch;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            string path = texturePath ?? TEXTURE_PATH;
            Texture2D tex = ModContent.Request<Texture2D>(path).Value;

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            gd.Textures[0] = tex;
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, 2);
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}