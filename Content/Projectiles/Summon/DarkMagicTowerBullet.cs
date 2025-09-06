using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class DarkMagicTowerBullet : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SapphireBolt;

        private const int DUST_INTERVAL = 10;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SapphireBolt);

            Projectile.aiStyle = -1;

            // 关键：召唤伤害
            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
            // return true;
        }

        public override void AI()
        {
            Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Water);
            // dust.velocity = Projectile.velocity;
            dust.noGravity = true;
            // dust.scale = 0.5f;
            // dust.alpha = 100;
            // dust.rotation = Projectile.rotation;
            // dust.fadeIn = 0.5f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 先获取贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // 设定你想要的颜色，比如粉色
            Color drawColor = new Color(255, 100, 200, 150);  

            // 绘制
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false; // 阻止默认绘制
        }
    }
}
