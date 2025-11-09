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
    public class SantaFlagBladeShot : FlagBladeShot
    {
        protected override float RadiusBig => 200;
        protected override float RadiusSmall => 135f;
        protected override float Angle => MathHelper.ToRadians(150f);
        protected override float MAX_SCALE => 2.2f;
        protected override float MIN_SCALE => 1.9f;
        protected override int TIME_LEFT => 30;
        protected override Color BladeColor => new Color(123, 62, 33, 100);

        public override bool PreAI()
        {
            // generate dust
            for (float theta = -Angle / 2f; theta < Angle / 2f; theta += Angle / 10f)
            {
                float radius = (RadiusBig + RadiusSmall) / 2f;
                if (Main.rand.NextFloat() < 0.05f)
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center + new Vector2(radius, 0).RotatedBy(theta + Projectile.rotation), 4, 4, 67, 0, 0, 0, Color.White, 1.5f);
                    d.noGravity = true;
                }
                if (Main.rand.NextFloat() < 0.05f)
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center + new Vector2(radius, 0).RotatedBy(theta + Projectile.rotation), 4, 4, 135, 0, 0, 0, Color.White, 2.0f);
                    d.noGravity = true;
                }
            }

            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 1)
            {
                target.AddBuff(BuffID.Frostburn, 3 * 60);
            }
            base.OnHitNPC(target, hit, damageDone);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            float height = texture.Height / Main.projFrames[Projectile.type];
            float width = texture.Width;

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(1 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(68, 187, 253, 150)),
                Projectile.rotation + MathHelper.ToRadians(15f),
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            
            // foreach(var d in Main.dust) if(d.active && d.type != 0) Main.NewText(d.type);

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(0 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(205, 237, 254, 200)),
                Projectile.rotation+MathHelper.ToRadians(-15f),
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(0 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(2, 139, 218, 150)),
                Projectile.rotation,
                new Vector2(width / 2f, height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, (int)(3 * height), (int)width, (int)height),
                Projectile.GetAlpha(new Color(219, 236, 255, 255)),
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