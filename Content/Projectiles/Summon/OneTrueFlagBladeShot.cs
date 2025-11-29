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
    public class OneTrueFlagBladeShot : FlagBladeShot
    {
        protected override float RadiusBig => 210f;
        protected override float RadiusSmall => 140f;
        protected override float Angle => MathHelper.ToRadians(160f);
        protected override float MAX_SCALE => 2.5f;
        protected override float MIN_SCALE => 2.2f;
        protected override int TIME_LEFT => 30;
        protected override Color BladeColor => new Color(123, 62, 33, 100);
        protected override int NPC_DEBUFF_ID => ModBuffID.OneTrueFlagDebuff;

        public override bool PreAI()
        {
            // generate dust
            for (float theta = -Angle / 2f; theta < Angle / 2f; theta += Angle / 10f)
            {
                float radius = (RadiusBig + RadiusSmall) / 2f;
                if (Main.rand.NextFloat() < 0.05f)
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center + new Vector2(radius, 0).RotatedBy(theta + Projectile.rotation), 4, 4, 91, 0, 0, 0, Color.White, 1.0f);
                    d.noGravity = true;
                }
            }

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            float height = texture.Height / Main.projFrames[Projectile.type];
            float width = texture.Width;

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(1 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(35, 54, 84, 150)),
                Projectile.rotation+MathHelper.ToRadians(15f),
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(0 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(25, 41, 67, 150)),
                Projectile.rotation+MathHelper.ToRadians(-15f),
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(0 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(212, 205, 189, 200)),
                Projectile.rotation,
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(3 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(230, 230, 228, 255)),
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