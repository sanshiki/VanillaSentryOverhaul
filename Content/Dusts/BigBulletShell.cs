using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

using SummonerExpansionMod.Initialization;


namespace SummonerExpansionMod.Content.Dusts
{
    public class BigBulletShell : ModDust
    {
        private const string TEXTURE_PATH = ModGlobal.MOD_TEXTURE_PATH + "Dusts/BigBulletShell";
        public override string Texture => TEXTURE_PATH;
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.frame = new Rectangle(0, 0, 48, 10);

            dust.customData = Main.rand.NextFloat(0.5f, 0.8f);
        }
		public override bool Update(Dust dust) {
            float rotSpeed = 0.6f;
            if (dust.customData is float s)
                rotSpeed = s;
            if(dust.noGravity == false) dust.velocity += new Vector2(0, 0.2f);
			dust.position += dust.velocity;
            dust.rotation += rotSpeed;
            dust.alpha += 5;
			if (dust.alpha > 250)
				dust.active = false;

            Point tilePos = dust.position.ToTileCoordinates();
            if (WorldGen.SolidTile(tilePos.X, tilePos.Y + 1))
            {
                if (dust.velocity.Y > 0f && dust.position.Y % 16 > 12)
                {
                    dust.velocity.Y *= -0.5f; // 弹跳
                    dust.velocity.X *= 0.8f;  // 水平减速
                    if (dust.customData is float spd)
                        dust.customData = spd * 0.3f;
                }
            }

			return false;
		}

        public override bool PreDraw(Dust dust) {
            var texture = ModContent.Request<Texture2D>(TEXTURE_PATH).Value;
            int TextureWidth = texture.Width;
            int TextureHeight = texture.Height;

            Color drawColor = Lighting.GetColor((int)(dust.position.X + TextureWidth/2) / 16, (int)(dust.position.Y + TextureHeight/2) / 16);
            drawColor = dust.GetAlpha(drawColor);
            Vector2 origin = new Vector2(dust.frame.Width / 2, dust.frame.Height / 2);
            Main.spriteBatch.Draw(
                texture,
                dust.position - Main.screenPosition,
                dust.frame,
                drawColor,
                dust.rotation,
                origin,
                dust.scale,
                SpriteEffects.None,
                0f
            );
            return false; // 返回 false，跳过默认绘制
        }
    }
}
