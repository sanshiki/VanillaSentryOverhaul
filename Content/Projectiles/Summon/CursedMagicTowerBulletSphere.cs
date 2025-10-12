using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class CursedMagicTowerBulletSphere : ModProjectile
    {
        public override string Texture => ModGlobal.VANILLA_PROJECTILE_TEXTURE_PATH + ProjectileID.CultistBossFireBallClone;

        private const int FRAME_COUNT = 4;
        private const int FRAME_SPEED = 7;
        private const int RADIUS = 18;
        private const int MAX_CHARGE_COUNT = 3;

        private const float MAX_SPEED_1 = 10.0f;
        private const float MAX_SPEED_2 = 30.0f;
        private const float ACCELERATION = 2f;

        private int ChargeCount = 0;
        private int DamageStep = 0;
        private bool Inited = false;
        private bool FullyCharged = false;

        private bool DEBUG = false;

        private List<int> DustIDs = new List<int> { 29, 41, 42, 45, 54, 59, 62, 65, 71, 86, 88, 109, 113, 164, 173 };

        private List<int> DarkDustIDs = new List<int> { 29, 41, 42, 65};

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FRAME_COUNT;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10; // 保存10帧历史位置
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // 拖影模式（0 = 基础，1 = 平滑）
            ProjectileID.Sets.SentryShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {   
            Projectile.width = 40;
            Projectile.height = 40;

            Projectile.aiStyle = -1;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60*20;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;

            DynamicParamManager.Register("DustIDIdx", 0f, 0f, (float)(DustIDs.Count-1), null);
            
        }

        public override void Kill(int timeLeft)
        {
            if(FullyCharged)
            {
                // create dust
                for(int i = 0; i < 10; i++)
                {
                    float AngleOffset = MathHelper.Lerp(-ModGlobal.PI_FLOAT/4f, ModGlobal.PI_FLOAT/4f, i/10f);
                    Vector2 DustVel = new Vector2(1, 0).RotatedBy(Projectile.velocity.ToRotation() + AngleOffset);
                    DustVel *= (MinionAIHelper.RandomFloat(5f, 10f) + Projectile.velocity.Length());
                    int EmitDustIDHit = DarkDustIDs[Main.rand.Next(DarkDustIDs.Count)];
                    Dust Dust = Dust.NewDustDirect(Projectile.Center, 1, 1, EmitDustIDHit, DustVel.X, DustVel.Y);
                    Dust.noGravity = true;
                    Dust.scale = MinionAIHelper.RandomFloat(2f, 3f);
                }

                if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: OnHitNPC");

                SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            }
        }

        public override void AI()
        {
            if(!Inited)
            {
                DamageStep = (int)(Projectile.damage * 1.5f);
                Inited = true;

                int EmitDustIDSpawn = DustIDs[11];
                for(int i = 0; i < 20; i++)
                {
                    float Angle = MinionAIHelper.RandomFloat(-ModGlobal.PI_FLOAT, ModGlobal.PI_FLOAT);
                    Vector2 DustVel = new Vector2(1, 0).RotatedBy(Angle);
                    DustVel *= MinionAIHelper.RandomFloat(10f, 20f);
                    Dust Dust = Dust.NewDustDirect(Projectile.Center, 1, 1, EmitDustIDSpawn, DustVel.X, DustVel.Y);
                    Dust.noGravity = true;
                    Dust.scale = MinionAIHelper.RandomFloat(0.8f, 1.6f);
                }
            }

            // search for bullet small nearby and kill
            List<int> BulletSmallIDs = MinionAIHelper.SearchForProjectiles(ModProjectileID.CursedMagicTowerBulletSmall, Projectile.Center, RADIUS);
            foreach(int id in BulletSmallIDs)
            {
                Projectile proj = Main.projectile[id];
                ChargeCount++;
                if(!FullyCharged) Projectile.velocity += proj.velocity * 0.6f;
                proj.Kill();

                // create dust when "swallow" small bullet
                int EmitDustIDSwallow = DustIDs[11];
                for(int i = 0; i < 5; i++)
                {
                    float Angle = MinionAIHelper.RandomFloat(-ModGlobal.PI_FLOAT, ModGlobal.PI_FLOAT);
                    Vector2 DustVel = new Vector2(1, 0).RotatedBy(Angle);
                    DustVel *= MinionAIHelper.RandomFloat(6f, 12f);
                    Dust Dust = Dust.NewDustDirect(Projectile.Center, 1, 1, EmitDustIDSwallow, DustVel.X, DustVel.Y);
                    Dust.noGravity = true;
                    Dust.scale = MinionAIHelper.RandomFloat(0.5f, 1.2f);
                }
            }

            // increase damage
            int NewDamage = DamageStep * (ChargeCount+1);

            if(ChargeCount >= MAX_CHARGE_COUNT && !FullyCharged)
            {
                // ChargeCount = MAX_CHARGE_COUNT;
                // Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModProjectileID.CursedMagicTowerBulletLarge, NewDamage, Projectile.knockBack, Projectile.owner);
                // proj.ai[0] = Projectile.ai[0];
                // Projectile.Kill();
                // // Main.NewText("[" + timestamp + "] Bullet Sphere: Kill Self, Emit Large Bullet");
                // return;
                FullyCharged = true;
                Projectile.penetrate = 1;
                Projectile.timeLeft = 60*10;
            }

            if(FullyCharged)
            {
                if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: FullyCharged checkpoint 1");
                // emit dust
                for(int i = 0; i < 3; i++)
                {
                    // int BlueDustID = (int)DynamicParamManager.Get("DustID").value;
                    // int BlueDustID = DustIDs[Main.rand.Next(DustIDs.Count)];
                    int BlueDustID = DarkDustIDs[Main.rand.Next(DarkDustIDs.Count)];
                    Dust BlueDust = Dust.NewDustDirect(Projectile.Center - new Vector2(Projectile.width/2, Projectile.height/2), Projectile.width, Projectile.height, BlueDustID, Projectile.velocity.X, Projectile.velocity.Y);
                    BlueDust.noGravity = true;
                    BlueDust.scale = MinionAIHelper.RandomFloat(0.8f, 2.0f);
                }

                if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: FullyCharged checkpoint 2");

                // find target
                int TargetID = (int)Projectile.ai[0];
                NPC Target = Main.npc[TargetID];
                if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: FullyCharged checkpoint 3");
                if(!Target.active)
                {
                    Target = MinionAIHelper.SearchForTargets(
                    Main.player[Projectile.owner],
                    Projectile,
                    2000f,
                    true,
                    null).TargetNPC;
                }
                if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: FullyCharged checkpoint 4");



                if(Target != null && Target.active)
                {
                    float SpdCurrent = Projectile.velocity.Length();
                    SpdCurrent = Math.Min(SpdCurrent + ACCELERATION, MAX_SPEED_2);
                    MinionAIHelper.HomeinToTarget(Projectile, Target.Center, SpdCurrent, 2f);
                    if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: FullyCharged checkpoint 5");
                }

                // reset damage
                Projectile.damage = NewDamage;
                if(DEBUG) Main.NewText("[" + DateTime.UtcNow.Ticks + "] Bullet Sphere: FullyCharged checkpoint 6");
            }
            else
            {
                // slowly approach target
                NPC Target = Main.npc[(int)Projectile.ai[0]];
                if(Target != null)
                {
                    float SpdCurrent = Projectile.velocity.Length();
                    SpdCurrent = Math.Min(SpdCurrent + ACCELERATION, MAX_SPEED_1);
                    MinionAIHelper.HomeinToTarget(Projectile, Target.Center, SpdCurrent, 10f);
                }

                // emit dust
                int EmitDustIDNormal = DarkDustIDs[Main.rand.Next(DarkDustIDs.Count)];
                float ChargeRate = (float)(ChargeCount+1) / MAX_CHARGE_COUNT;
                int DustNum = (int)(5f * ChargeRate);
                float DustVelBase = 6f * ChargeRate + 2f;
                float DustScaleBase = 0.5f * ChargeRate + 0.5f;

                for(int i = 0; i < DustNum; i++)
                {
                    float Angle = MinionAIHelper.RandomFloat(-ModGlobal.PI_FLOAT, ModGlobal.PI_FLOAT);
                    Vector2 DustVel = new Vector2(1, 0).RotatedBy(Angle);
                    DustVel *= MinionAIHelper.RandomFloat(DustVelBase - 2f, DustVelBase + 2f);
                    DustVel += Projectile.velocity;
                    Dust Dust = Dust.NewDustDirect(Projectile.Center, 1, 1, EmitDustIDNormal, DustVel.X, DustVel.Y);
                    Dust.noGravity = true;
                    Dust.scale = MinionAIHelper.RandomFloat(DustScaleBase - 0.3f, DustScaleBase + 0.3f);
                }
            }

            UpdateAnimation();
        }


        private void UpdateAnimation()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FRAME_SPEED)
            {
                Projectile.frame = (Projectile.frame + 1) % FRAME_COUNT;
                Projectile.frameCounter = 0;
            }
            Projectile.rotation += 0.5f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 先获取贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int TextureHeightPerFrame = texture.Height / Main.projFrames[Projectile.type];

            Rectangle RectCurFrame = new Rectangle(0, TextureHeightPerFrame * Projectile.frame, Projectile.width, TextureHeightPerFrame);

            Vector2 Origin = new Vector2(texture.Width / 2, TextureHeightPerFrame / 2);

            // 设定你想要的颜色，比如粉色
            Color BaseDrawColor = new Color(65, 38, 250, 255);

            // 绘制
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                RectCurFrame,
                BaseDrawColor,
                Projectile.rotation,
                Origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            for(int i = Projectile.oldPos.Length-1;i >= 0; i-=2)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color color = Projectile.GetAlpha(BaseDrawColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    RectCurFrame,
                    color,
                    Projectile.rotation,
                    Origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                );
            }


            return false; // 阻止默认绘制
        }
    }
}
