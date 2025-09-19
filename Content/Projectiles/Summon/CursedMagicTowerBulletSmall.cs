using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class CursedMagicTowerBulletSmall : ModProjectile
    {
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.SapphireBolt;

        private const int DUST_INTERVAL = 10;

        private const float MAX_SPEED = 15f;
        private const float DEACC_DIST = 900f;
        private const float DEACC = MAX_SPEED * MAX_SPEED / (2 * DEACC_DIST);

        private float DustDir = 0f;

        private bool HasFoundSphere = false;

        /* 
         * 29： dark blue small
         * 41： similar to 29, little brighter
         * 42： normal blue large
         * 45： simailar to 42, little brighter
         * 54:  black large
         * 59:  light blue small
         * 62:  pink + purple small
         * 65:  similar to 62, little darker
         * 71:  light pink large
         * 86:  similar to 71, little brighter
         * 88:  white + light blue large
         * 109:  dark black large
         * 113:  similar to 88
         * 164:  white + pink small
         * 173:  white + purple small
         */

        private List<int> DustIDs = new List<int> { 29, 41, 42, 45, 54, 59, 62, 65, 71, 86, 88, 109, 113, 164, 173 };

        public Vector2 Target;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;

            Projectile.aiStyle = -1;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 999999;
            Projectile.timeLeft = 600;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;

            Projectile.alpha = 250;

            // DynamicParamManager.Register("DustIDIdx", 0f, 0f, (float)(DustIDs.Count-1), null);
            // DynamicParamManager.Register("CursedMagicTowerBulletSmall.DeAccDist", DEACC_DIST, 0f, 2000f, null);
        }

        public override void AI()
        {
            // 27 29 41 42 45 54 59 62 65 71 86 88 109 113 164 173
            // int BlueDustIDIdx = (int)DynamicParamManager.Get("DustIDIdx").value;
            int BlueDustID = 29;
            // int BlueDustID = DustIDs[BlueDustIDIdx];
            Dust BlueDust = Dust.NewDustDirect(Projectile.Center - Projectile.Size/2f, Projectile.width, Projectile.height, BlueDustID, Projectile.velocity.X, Projectile.velocity.Y);
            BlueDust.noGravity = true;
            BlueDust.scale = MinionAIHelper.RandomFloat(0.8f, 2.0f);

            // Vector2 ShadowflameDustPos = Projectile.Center + new Vector2(0, -8f).RotatedBy(DustDir);
            // Dust ShadowflameDust = Dust.NewDustPerfect(ShadowflameDustPos, DustID.Shadowflame);
            // DustDir += 0.8f;
            // ShadowflameDust.noGravity = true;
            // ShadowflameDust.scale = 1.3f;

            Vector2 spherePos = SearchForSphere();

            long timestamp = DateTime.UtcNow.Ticks;

            // found sphere
            if(HasFoundSphere)
            {
                float DynamicInertia = 10f + (Projectile.Center.Distance(spherePos) / 500f) * 10f;
                MinionAIHelper.HomeinToTarget(Projectile, spherePos, MAX_SPEED, DynamicInertia);

                // Main.NewText("[" + timestamp + "] Bullet Small: HasFoundSphere");

                // // if close enough to sphere, kill self
                // if(Projectile.Center.Distance(spherePos) < 50f)
                // {
                //     Projectile.Kill();
                //     return;
                // }
            }
            // not found sphere
            else
            {
                // float DeAccDist = (float)DynamicParamManager.Get("CursedMagicTowerBulletSmall.DeAccDist").value;
                float DeAccDist = DEACC_DIST;
                float DeAcc = MAX_SPEED * MAX_SPEED / (2 * DeAccDist);
                if(Projectile.Center.Distance(Target) < DeAccDist || Projectile.velocity.Length() < MAX_SPEED)
                {
                    // deaccelerate when close to target
                    Vector2 vel = Projectile.velocity;

                    if(vel.Length() < DeAcc)
                    {
                        Projectile.velocity = Vector2.Zero;
                    }
                    else
                    {
                        Projectile.velocity -= vel.SafeNormalize(Vector2.Zero) * DeAcc;
                    }
                }

                // Main.NewText("[" + timestamp + "] Bullet Small: Not FoundSphere");

                // if velocity is too small, emit sphere
                if(Projectile.velocity.Length() < 0.1f)
                {
                    Projectile Sphere = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModProjectileID.CursedMagicTowerBulletSphere, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Sphere.ai[0] = Projectile.ai[0];
                    Projectile.Kill();
                    // Main.NewText("[" + timestamp + "] Bullet Small: Kill Self, EmitSphere");
                    return;
                }
            }

            
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int penetrate_times = 999999 - Projectile.penetrate;
            // reduce damage when go through multiple times
            Projectile.damage = (int)(Projectile.damage * Math.Pow(0.8f, penetrate_times));
        }

        private Vector2 SearchForSphere()
        {
            HasFoundSphere = false;
            long timestamp = DateTime.UtcNow.Ticks;
            for(int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if(proj.type == ModProjectileID.CursedMagicTowerBulletSphere && proj.active && proj.owner == Projectile.owner && proj.Center.Distance(Projectile.Center) < 2000f)
                {
                    HasFoundSphere = true;
                    // Main.NewText("[" + timestamp + "] Bullet Small: Found Sphere: " + proj.Center);
                    return proj.Center;
                }
            }
            return Vector2.Zero;
        }

        // public override bool PreDraw(ref Color lightColor)
        // {
            
        //     Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

        //     // 绘制
        //     Main.EntitySpriteDraw(
        //         texture,
        //         Projectile.Center - Main.screenPosition,
        //         null,
        //         Color.White,
        //         Projectile.rotation,
        //         texture.Size() / 2f,
        //         Projectile.scale*0.1f,
        //         SpriteEffects.None,
        //         0
        //     );
        //     return false; // 阻止默认绘制
        // }
    }
}
