using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Content.Items.Accessories;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class MachineGunSentryBullet : ClonedSentryProjectile
    {
        public override int BaseProjectileID => ProjectileID.Bullet;

        public override string TexturePath => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        public int SelfDamage = 5;
        public int SelfArmorPenetration = 10;

        private bool DamageDebug = false;

        private bool hasAccessory = false;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.SummonTagDamageMultiplier[Type] = 0.25f;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            HD2SentryDmgReductionPlayer hd2SentryDmgReductionPlayer = player.GetModPlayer<HD2SentryDmgReductionPlayer>();
            hasAccessory = hd2SentryDmgReductionPlayer.hasAccessory;
        }

        public override void AI()
        {
            float DifficultyFactor = 1f;
            float expertFactor = DamageDebug ? 1f : DynamicParamManager.QuickGet("MachineGunSentryBulletExpertFactor", 1f, 1f, 3f).value;
            float masterFactor = DamageDebug ? 1f : DynamicParamManager.QuickGet("MachineGunSentryBulletMasterFactor", 1f, 1f, 3f).value;
            if(Main.expertMode && !Main.masterMode)
            {
                DifficultyFactor = 1f/expertFactor;
            }
            else if (Main.masterMode)
            {
                DifficultyFactor = 1f/masterFactor;
            }

            float ReductionFactor = hasAccessory ? 0.5f : 1f;

            Player player = Main.player[Projectile.owner];
            if (Projectile.Hitbox.Intersects(player.Hitbox) && !player.immune)
            {
                int hitDir = Projectile.Center.X > player.Center.X ? 1 : -1;
                player.Hurt(
                    PlayerDeathReason.ByProjectile(
                        player.whoAmI,
                        Projectile.whoAmI),
                    (int)(SelfDamage * DifficultyFactor * ReductionFactor),
                    hitDir,
                    knockback: hasAccessory ? 0f : Projectile.knockBack,
                    armorPenetration:SelfArmorPenetration
                );

                if(hasAccessory) player.immuneTime += 30;

                Projectile.Kill(); // 避免每帧重复触发
            }
        }
    }

    public class AutocannonSentryBullet : ClonedSentryProjectile
    {
        public override int BaseProjectileID => ProjectileID.BulletHighVelocity;

        public override string TexturePath => "Terraria/Images/Projectile_" + ProjectileID.BulletHighVelocity;

        public int SelfDamage = 65;

        public int SelfArmorPenetration = 5;

        private bool HurtFlag = false;

        private bool DamageDebug = false;

        private bool hasAccessory = false;

        private const float DAMAGE_DECAY_FACTOR = 0.8f;
        private int hitCount = 0;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            HD2SentryDmgReductionPlayer hd2SentryDmgReductionPlayer = player.GetModPlayer<HD2SentryDmgReductionPlayer>();
            hasAccessory = hd2SentryDmgReductionPlayer.hasAccessory;
        }

        public override void AI()
        {
            if(HurtFlag)
            {
                return;
            }

            Player player = Main.player[Projectile.owner];

            float DifficultyFactor = 1f;
            float ReductionFactor = hasAccessory ? 0.5f : 1f;
            float expertFactor = DamageDebug ? 1.9f : DynamicParamManager.QuickGet("AutocannonSentryBulletExpertFactor", 1.9f, 1f, 3f).value;
            float masterFactor = DamageDebug ? 2.2f : DynamicParamManager.QuickGet("AutocannonSentryBulletMasterFactor", 2.2f, 1f, 3f).value;
            if(Main.expertMode && !Main.masterMode)
            {
                DifficultyFactor = 1f/expertFactor;
            }
            else if (Main.masterMode)
            {
                DifficultyFactor = 1f/masterFactor;
            }

            if (Projectile.Hitbox.Intersects(player.Hitbox) && !player.immune)
            {
                int hitDir = Projectile.Center.X > player.Center.X ? 1 : -1;
                player.Hurt(
                    PlayerDeathReason.ByProjectile(
                        player.whoAmI,
                        Projectile.whoAmI),
                    (int)(SelfDamage * DifficultyFactor * ReductionFactor),
                    hitDir,
                    knockback: hasAccessory ? 0f : Projectile.knockBack,
                    armorPenetration:SelfArmorPenetration
                );

                if(hasAccessory) player.immuneTime += 30;

                Projectile.penetrate--;
                HurtFlag = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = (target.Center - player.Center).X > 0 ? 1 : -1;

            float multiplier = (float)Math.Pow(DAMAGE_DECAY_FACTOR, hitCount);

            modifiers.FinalDamage *= multiplier;

            hitCount++;
        }
    }
}