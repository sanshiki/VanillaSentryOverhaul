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
    public class RocketSentryBullet : ModProjectile
    {
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.RocketI;

        private const float MAX_HOMING_RANGE = 600f;
        private const float MAX_HOMING_TURN_SPEED = 0.2f;
        private const float EXPLOSION_RADIUS = 100f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.RocketI);
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (Math.Abs(Projectile.velocity.X) >= 8f || Math.Abs(Projectile.velocity.Y) >= 8f)
            {
                for (int n = 0; n < 2; n++)
                {
                    float num23 = 0f;
                    float num24 = 0f;
                    if (n == 1)
                    {
                        num23 = Projectile.velocity.X * 0.5f;
                        num24 = Projectile.velocity.Y * 0.5f;
                    }
                    int num13 = 6;
                    int num25 = Dust.NewDust(new Vector2(Projectile.position.X + 3f + num23, Projectile.position.Y + 3f + num24) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, num13, 0f, 0f, 100);
                    Main.dust[num25].scale *= 2f + (float)Main.rand.Next(10) * 0.1f;
                    Main.dust[num25].velocity *= 0.2f;
                    Main.dust[num25].noGravity = true;
                    if (Main.dust[num25].type == 152)
                    {
                        Main.dust[num25].scale *= 0.5f;
                        Main.dust[num25].velocity += Projectile.velocity * 0.1f;
                    }
                    else if (Main.dust[num25].type == 35)
                    {
                        Main.dust[num25].scale *= 0.5f;
                        Main.dust[num25].velocity += Projectile.velocity * 0.1f;
                    }
                    else if (Main.dust[num25].type == Dust.dustWater())
                    {
                        Main.dust[num25].scale *= 0.65f;
                        Main.dust[num25].velocity += Projectile.velocity * 0.1f;
                    }
                    if (Projectile.type == 793 || Projectile.type == 796)
                    {
                        Dust dust5 = Main.dust[num25];
                        if (dust5.dustIndex != 6000)
                        {
                            dust5 = Dust.NewDustPerfect(dust5.position, dust5.type, dust5.velocity, dust5.alpha, dust5.color, dust5.scale);
                            dust5.velocity = Main.rand.NextVector2Circular(3f, 3f);
                            dust5.noGravity = true;
                        }
                        if (dust5.dustIndex != 6000)
                        {
                            dust5 = Dust.NewDustPerfect(dust5.position, dust5.type, dust5.velocity, dust5.alpha, dust5.color, dust5.scale);
                            dust5.velocity = ((float)Math.PI * 2f * ((float)Projectile.timeLeft / 20f)).ToRotationVector2() * 3f;
                            dust5.noGravity = true;
                        }
                    }
                    num25 = Dust.NewDust(new Vector2(Projectile.position.X + 3f + num23, Projectile.position.Y + 3f + num24) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, 31, 0f, 0f, 100, default(Color), 0.5f);
                    Main.dust[num25].fadeIn = 1f + (float)Main.rand.Next(5) * 0.1f;
                    Main.dust[num25].velocity *= 0.05f;
                }
            }
            if (Math.Abs(Projectile.velocity.X) < 15f && Math.Abs(Projectile.velocity.Y) < 15f)
            {
                Projectile.velocity *= 1.1f;
            }
            if (Projectile.velocity != Vector2.Zero)
			{
				Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
			}

            NPC target = Main.npc[(int)(Projectile.ai[0])];

            if(target == null || !target.active)
            {
                target = MinionAIHelper.SearchForTargets(
                Main.player[Projectile.owner], 
                Projectile, 
                MAX_HOMING_RANGE, 
                true, 
                null).TargetNPC;
            }

            if(target != null)
            {
                float VelMod = Projectile.velocity.Length();
                float VelDir = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
                float HomeDir = (float)Math.Atan2(target.Center.Y - Projectile.Center.Y, target.Center.X - Projectile.Center.X);
                float error = MinionAIHelper.NormalizeAngle(HomeDir - VelDir);
                float turnSpeed = Math.Min(MAX_HOMING_TURN_SPEED, Math.Abs(error));
                if(turnSpeed > 0.01f)
                {
                    VelDir += Math.Sign(error) * turnSpeed;
                }
                Projectile.velocity = new Vector2((float)Math.Cos(VelDir), (float)Math.Sin(VelDir)) * VelMod;
                
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(in SoundID.Item14, Projectile.position);
            Projectile.position.X += Projectile.width / 2;
            Projectile.position.Y += Projectile.height / 2;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.position.X -= Projectile.width / 2;
            Projectile.position.Y -= Projectile.height / 2;
            for (int num903 = 0; num903 < 30; num903++)
            {
                int num904 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust306 = Main.dust[num904];
                Dust dust3 = dust306;
                dust3.velocity *= 1.4f;
            }
            for (int num905 = 0; num905 < 20; num905++)
            {
                int num906 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 3.5f);
                Main.dust[num906].noGravity = true;
                Dust dust307 = Main.dust[num906];
                Dust dust3 = dust307;
                dust3.velocity *= 7f;
                num906 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                dust307 = Main.dust[num906];
                dust3 = dust307;
                dust3.velocity *= 3f;
            }
            for (int num907 = 0; num907 < 2; num907++)
            {
                float num908 = 0.4f;
                if (num907 == 1)
                {
                    num908 = 0.8f;
                }
                IEntitySource source = Projectile.GetSource_FromThis();
                int num909 = Gore.NewGore(source, new Vector2(Projectile.position.X, Projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
                Gore gore54 = Main.gore[num909];
                Gore gore3 = gore54;
                gore3.velocity *= num908;
                Main.gore[num909].velocity.X += 1f;
                Main.gore[num909].velocity.Y += 1f;
                num909 = Gore.NewGore(source, new Vector2(Projectile.position.X, Projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
                gore54 = Main.gore[num909];
                gore3 = gore54;
                gore3.velocity *= num908;
                Main.gore[num909].velocity.X -= 1f;
                Main.gore[num909].velocity.Y += 1f;
                num909 = Gore.NewGore(source, new Vector2(Projectile.position.X, Projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
                gore54 = Main.gore[num909];
                gore3 = gore54;
                gore3.velocity *= num908;
                Main.gore[num909].velocity.X += 1f;
                Main.gore[num909].velocity.Y -= 1f;
                num909 = Gore.NewGore(source, new Vector2(Projectile.position.X, Projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
                gore54 = Main.gore[num909];
                gore3 = gore54;
                gore3.velocity *= num908;
                Main.gore[num909].velocity.X -= 1f;
                Main.gore[num909].velocity.Y -= 1f;
            }

            // judge tile explodes
            // int num1013 = 3;
            // Vector2 center3 = Projectile.position;
            // int num1014 = num1013;
            // int num1015 = num1013;
            // int num1016 = (int)(center3.X / 16f - (float)num1014);
            // int num1017 = (int)(center3.X / 16f + (float)num1014);
            // int num1018 = (int)(center3.Y / 16f - (float)num1015);
            // int num1019 = (int)(center3.Y / 16f + (float)num1015);
            // if (num1016 < 0)
            // {
            //     num1016 = 0;
            // }
            // if (num1017 > Main.maxTilesX)
            // {
            //     num1017 = Main.maxTilesX;
            // }
            // if (num1018 < 0)
            // {
            //     num1018 = 0;
            // }
            // if (num1019 > Main.maxTilesY)
            // {
            //     num1019 = Main.maxTilesY;
            // }
            // bool wallSplode2 = Projectile.ShouldWallExplode(center3, num1013, num1016, num1017, num1018, num1019);
            // Projectile.ExplodeTiles(center3, num1013, num1016, num1017, num1018, num1019, wallSplode2);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0f;
            Projectile.alpha = 255;
            Projectile.timeLeft = 3;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 范围半径（像素）
            float radius = EXPLOSION_RADIUS; // 大约5格范围
            Vector2 center = Projectile.Center;

            // 伤害范围内的NPC
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(center) < radius && npc != target && !npc.dontTakeDamage && !npc.immortal)
                {
                    // 计算伤害
                    int damage = (int)(Projectile.damage * 0.8f); // 爆炸伤害稍低
                    // Main.NewText("damage:" + damage.ToString());
                    NPC.HitInfo hitInfo = hit;
                    hitInfo.Damage = damage;
                    npc.StrikeNPC(hitInfo, false, false);
                }
            }
        }
    }

    
}