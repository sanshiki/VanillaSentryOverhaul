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
using Terraria.Graphics.CameraModifiers;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class OneTrueFlagAnchor : ModProjectile
    {
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.JimsDrone;
        private const int HELLPOD_SUMMON_HEIGHT = 1000;
        private const int HELLPOD_DAMAGE = 100;
        private const float HELLPOD_KNOCKBACK = 10f;

        private const float EXPLOSION_RADIUS = 100f;

        private Projectile HellpodProjectile = null;
        private bool HellpodSpawned = false;
        public SentryRecallInfo sentryInfo;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.JimsDrone);
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 60*2;
            Projectile.alpha = 255;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.alpha = (int)MathHelper.Lerp(0, 255, Math.Max(0f, Projectile.timeLeft - 60f) / 60f);

            if (!HellpodSpawned)
            {
                HellpodSpawned = true;
                float HeightOffset = MinionAIHelper.RandomFloat(-200, 200);
                HellpodProjectile = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + new Vector2(0, -HELLPOD_SUMMON_HEIGHT + HeightOffset),
                    new Vector2(0, 10f),
                    ModContent.ProjectileType<Hellpod>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
                HellpodProjectile.hostile = false;
            }

            if (HellpodProjectile != null)
            {
                Vector2 Dist2Hellpod = HellpodProjectile.Center - Projectile.Center;
                if (Dist2Hellpod.Length() < 10f || Dist2Hellpod.Y > 0)
                {
                    // create dust
                    for (int i = 0; i < 10; i++)
                    {
                        // dirt
                        float dust_ang = -(float)i * 180f / 10f;
                        float dust_speed = MinionAIHelper.RandomFloat(3f, 5f);
                        Vector2 vel = new Vector2(dust_speed, 0f).RotatedBy(dust_ang * MathHelper.ToRadians(1));
                        Dust dirtDust = Dust.NewDustDirect(Projectile.Center, 10, 10, DustID.Dirt, vel.X, vel.Y, 0, Color.White, 1.0f);
                        // smoke
                        Dust smokeDust = Dust.NewDustDirect(Projectile.Center, 10, 10, DustID.Smoke, 0, 0, 0, Color.White, 1f);
                        smokeDust.noGravity = true;
                        float smoke_ang = -(float)i * 180f / 10f;
                        float smoke_speed = MinionAIHelper.RandomFloat(3f, 6f);
                        float smoke_size = MinionAIHelper.RandomFloat(3f, 5f);
                        int smoke_alpha = (int)(MinionAIHelper.RandomFloat(0.0f, 1.0f) * 255);
                        smokeDust.velocity = new Vector2(1, 0f).RotatedBy(smoke_ang * MathHelper.ToRadians(1));
                        smokeDust.velocity *= smoke_speed;
                        smokeDust.scale = smoke_size;
                        smokeDust.alpha = smoke_alpha;


                        float flame_ang = -((float)i * 90f / 10f + 45f);
                        float flame_speed = MinionAIHelper.RandomFloat(5f, 8f);
                        float flame_size = MinionAIHelper.RandomFloat(1f, 1f);
                        // int flame_alpha = (int)(MinionAIHelper.RandomFloat(0.5f, 1.5f) * 255);
                        Vector2 flame_vel = new Vector2(1, 0f).RotatedBy(flame_ang * MathHelper.ToRadians(1));
                        // Main.NewText("flame_ang: " + flame_ang + " flame_speed: " + flame_speed + " flame_size: " + flame_size);
                        flame_vel *= flame_speed;
                        Dust flameDust = Dust.NewDustDirect(Projectile.Center, 10, 10, DustID.Torch, flame_vel.X, flame_vel.Y, 0, Color.White, flame_size);
                        // flameDust.noGravity = true;
                    }

                    // teleport sentry
                    if (sentryInfo != null)
                    {
                        var sentry = Main.projectile[sentryInfo.ID];
                        if (sentry != null && sentry.active)
                        {
                            sentry.Center = sentryInfo.TargetPos + new Vector2(0, -sentry.height * 0.55f);
                            sentry.velocity = sentryInfo.TileCollide ? new Vector2(0, 20f) : Vector2.Zero;
                            sentryInfo.IsRecalled = true;
                        }
                    }

                    SoundEngine.PlaySound(ModSounds.HellpodSignal_3_2, Projectile.Center);

                    // kill self and hellpod
                    HellpodProjectile.Kill();
                    Projectile.Kill();
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            // SoundEngine.PlaySound(in SoundID.Item14, Projectile.position);
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

            // 范围半径（像素）
            float radius = EXPLOSION_RADIUS; // 大约5格范围
            Vector2 center = Projectile.Center;

            // 伤害范围内的NPC
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(center) < radius && !npc.dontTakeDamage && !npc.immortal)
                {
                    // 计算伤害
                    NPC.HitInfo hitInfo = new NPC.HitInfo();
                    hitInfo.Damage = (int)(Projectile.damage * 0.8f); // 爆炸伤害稍低
                    hitInfo.Knockback = Projectile.knockBack;
                    hitInfo.Crit = Main.rand.NextFloat() < Projectile.CritChance / 100f;
                    npc.StrikeNPC(hitInfo);
                }
            }

            PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, (-MathHelper.PiOver2).ToRotationVector2(), 10f, 6f, 10, 1000f, "OneTrueFlagAnchor");
            Main.instance.CameraModifiers.Add(modifier);
        }
        
        public override bool MinionContactDamage()
		{
			return false;
		}
    }
}