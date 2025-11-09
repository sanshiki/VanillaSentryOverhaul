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
    public class TikiFlagBladeShot : FlagBladeShot
    {
        protected override float RadiusBig => 170f;
        protected override float RadiusSmall => 130f;
        protected override float Angle => MathHelper.ToRadians(150f);
        protected override float MAX_SCALE => 2.0f;
        protected override float MIN_SCALE => 1.65f;
        protected override int TIME_LEFT => 30;
        protected override Color BladeColor => new Color(123, 62, 33, 100);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            float height = texture.Height / Main.projFrames[Projectile.type];
            float width = texture.Width;

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(1 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(59, 37, 24, 150)),
                Projectile.rotation+MathHelper.ToRadians(15f),
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(0 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(123, 62, 33, 150)),
                Projectile.rotation+MathHelper.ToRadians(-15f),
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(0 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(53, 31, 48, 200)),
                Projectile.rotation,
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(3 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(53, 31, 48, 255)),
                Projectile.rotation,
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}