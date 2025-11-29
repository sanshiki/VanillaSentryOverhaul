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
    public class FlagBladeShot : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NightsEdge;
        protected virtual float RadiusBig => 100f;
        protected virtual float RadiusSmall => 80f;
        protected virtual float Angle => 120f / 180f * ModGlobal.PI_FLOAT;
        protected virtual Color BladeColor => Color.White;
        protected virtual int TIME_LEFT => 60 * 1;
        protected virtual float MAX_SCALE => 2f;
        protected virtual float MIN_SCALE => 1f;
        protected virtual float DAMAGE_DECAY_FACTOR => 0.5f;
        protected int hitCount = 0;
        protected virtual int NPC_DEBUFF_ID => ModContent.BuffType<NormalFlagBuff>();
        protected virtual int NPC_DEBUFF_DURATION => 60*7;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
            ProjectileID.Sets.TrailCacheLength[Type] = 4;//这一项代表记录的轨迹最多能追溯到多少帧以前
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = TIME_LEFT;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.localNPCHitCooldown = Projectile.timeLeft;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            float timeLeftRate = Projectile.timeLeft / (float)TIME_LEFT;
            Projectile.scale = MathHelper.Lerp(MIN_SCALE, MAX_SCALE, timeLeftRate);
            Projectile.alpha = (int)MathHelper.Lerp(255, 0, timeLeftRate);
            Projectile.velocity *= 0.95f;

            // for(float r = 0;r < RadiusBig;r += 10f)
            // {
            //     for(float theta = 0;theta < ModGlobal.TWO_PI_FLOAT;theta += 0.4f)
            //     {
            //         Vector2 pos = Projectile.Center + new Vector2(r, 0).RotatedBy(theta);
            //         if (IsInArcRange(pos))
            //         {
            //             Dust.QuickDust(pos, BladeColor);
            //         }
            //     }
            // }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = (target.Center - player.Center).X > 0 ? 1 : -1;

            float multiplier = (float)Math.Pow(DAMAGE_DECAY_FACTOR, hitCount);

            modifiers.FinalDamage *= multiplier;

            hitCount++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(NPC_DEBUFF_ID, NPC_DEBUFF_DURATION);

            Player player = Main.player[Projectile.owner];
            player.MinionAttackTargetNPC = target.whoAmI;
            // player.HasMinionAttackTargetNPC = true;

            int ImbueDeBuffID = MinionAIHelper.GetImbueDebuff(player);
            if (ImbueDeBuffID != -1) target.AddBuff(ImbueDeBuffID, 3 * 60);
            if(MinionAIHelper.IsPartyImbue(player))
            {
                Projectile.NewProjectile(new EntitySource_Misc("WeaponEnchantment_Confetti"), target.Center.X, target.Center.Y, target.velocity.X, target.velocity.Y, 289, 0, 0f, player.whoAmI);
            }
        }

        private bool IsInArcRange(Vector2 pos)
        {
            Vector2 direction = pos - Projectile.Center;

            if (direction.Length() < RadiusSmall || direction.Length() > RadiusBig)
                return false;
            if (Math.Abs(MathHelper.WrapAngle(direction.ToRotation() - Projectile.rotation)) > Angle / 2f) // bug here!
            {
                return false;   
            }

            return true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 TargetCenter = targetHitbox.Center.ToVector2();

            return IsInArcRange(TargetCenter);
        }
    }
}