using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace SummonerExpansionMod.Systems
{
    public class HitboxDrawerSystem : ModSystem
    {
        private const int LINE_WIDTH = 2;
        private const bool ENABLE_HITBOX_DRAW = false;
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (!ENABLE_HITBOX_DRAW)
            {
                return;
            }

            // 绘制所有 NPC 的 hitbox
            foreach (NPC npc in Main.npc)
            {
                if (npc.active)
                {
                    DrawHitbox(spriteBatch, npc.Hitbox, Color.Red);
                }
            }

            // 绘制所有 Projectile 的 hitbox
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active)
                {
                    DrawHitbox(spriteBatch, proj.Hitbox, Color.Red);
                }
            }
        }

        private void DrawHitbox(SpriteBatch spriteBatch, Rectangle hitbox, Color color)
        {
            // 转换到屏幕坐标
            Rectangle box = new Rectangle(
                ((int)((hitbox.X - (int)(Main.screenPosition.X)) * 0.9926)), //0.9875
                ((int)((hitbox.Y - (int)(Main.screenPosition.Y)) * 0.9926)), //0.9926    
                hitbox.Width,
                hitbox.Height
            );

            // 绘制四条边
            DrawLine(spriteBatch, new Vector2(box.Left, box.Top), new Vector2(box.Right, box.Top), color);   // 上边
            DrawLine(spriteBatch, new Vector2(box.Left, box.Bottom), new Vector2(box.Right, box.Bottom), color); // 下边
            DrawLine(spriteBatch, new Vector2(box.Left, box.Top), new Vector2(box.Left, box.Bottom), color); // 左边
            DrawLine(spriteBatch, new Vector2(box.Right, box.Top), new Vector2(box.Right, box.Bottom), color); // 右边
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
        {
            // 使用一像素纹理来画线
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 edge = end - start;
            Rectangle rect = new Rectangle(0, 0, (int)(edge.Length()), LINE_WIDTH);
            // Main.NewText("edge.Length(): " + edge.Length());
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(tex, start, rect, color,
                angle, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
