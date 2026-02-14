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
using SummonerExpansionMod.Content.Items.Accessories;


namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class Hellpod : ModProjectile
    {

        private const float GRAVITY = 0.8f;

        private const int FLAME_OFFSET_X = 17;
        private const int FLAME_OFFSET_Y = 24;
        private const int FLAME_SPEED_X = 3;
        private const int FLAME_SPEED_Y = 6;

        public override string Texture => ModGlobal.MOD_TEXTURE_PATH + "Projectiles/Hellpod";

        private bool DamageDebug = false;
        private int SelfDamage = 100;
        private bool HurtFlag = false;

        private bool hasAccessory = false;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 53;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            HD2SentryDmgReductionPlayer hd2SentryDmgReductionPlayer = player.GetModPlayer<HD2SentryDmgReductionPlayer>();
            hasAccessory = hd2SentryDmgReductionPlayer.hasAccessory;
        }

        public override void AI()
        {
            MinionAIHelper.ApplyGravity(Projectile, GRAVITY, 20f);

            // left flame
            Vector2 LeftFlameOffset = new Vector2(-FLAME_OFFSET_X, -FLAME_OFFSET_Y);
            Vector2 LeftFlameSpeed = new Vector2(-FLAME_SPEED_X, FLAME_SPEED_Y);
            float LeftSpeedAngleOffset = MinionAIHelper.RandomFloat(-Math.PI / 4.0, Math.PI / 4.0);
            LeftFlameSpeed = LeftFlameSpeed.RotatedBy(LeftSpeedAngleOffset);
            Dust flameDust = Dust.NewDustPerfect(Projectile.Center + LeftFlameOffset, DustID.FlameBurst, LeftFlameSpeed, 0, default, 1f);
            flameDust.noGravity = true;

            // right flame
            Vector2 RightFlameOffset = new Vector2(FLAME_OFFSET_X, -FLAME_OFFSET_Y);
            Vector2 RightFlameSpeed = new Vector2(FLAME_SPEED_X, FLAME_SPEED_Y);
            float RightSpeedAngleOffset = MinionAIHelper.RandomFloat(-Math.PI / 4.0, Math.PI / 4.0);
            RightFlameSpeed = RightFlameSpeed.RotatedBy(RightSpeedAngleOffset);
            flameDust = Dust.NewDustPerfect(Projectile.Center + RightFlameOffset, DustID.FlameBurst, RightFlameSpeed, 0, default, 1f);
            flameDust.noGravity = true;

            // smoke
            Vector2 SmokeOffset = new Vector2(0, -FLAME_OFFSET_Y);
            Vector2 SmokeSpeed = new Vector2(0, FLAME_SPEED_Y);
            float SmokeSpeedAngleOffset = MinionAIHelper.RandomFloat(-Math.PI / 16.0, Math.PI / 16.0);
            SmokeSpeed = SmokeSpeed.RotatedBy(SmokeSpeedAngleOffset);
            float SmokeScale = MinionAIHelper.RandomFloat(0.8f, 1.2f);
            Dust smokeDust = Dust.NewDustPerfect(Projectile.Center + SmokeOffset, DustID.Smoke, SmokeSpeed, 0, default, SmokeScale);

            // middle flame
            Vector2 MiddleFlameOffset = new Vector2(0, -FLAME_OFFSET_Y);
            Vector2 MiddleFlameSpeed = new Vector2(0, FLAME_SPEED_Y);
            float MiddleSpeedAngleOffset = MinionAIHelper.RandomFloat(-Math.PI / 4.0, Math.PI / 4.0);
            MiddleFlameSpeed = MiddleFlameSpeed.RotatedBy(MiddleSpeedAngleOffset);
            float MiddleFlameScale = MinionAIHelper.RandomFloat(1.0f, 1.5f);
            Dust middleFlameDust = Dust.NewDustPerfect(Projectile.Center + MiddleFlameOffset, DustID.FlameBurst, MiddleFlameSpeed, 0, default, MiddleFlameScale);
            middleFlameDust.noGravity = true;

            // deal damage
            Player player = Main.player[Projectile.owner];

            float DifficultyFactor = 1f;
            float expertFactor = DamageDebug ? 2f : DynamicParamManager.QuickGet("HellpodExpertFactor", 2f, 1f, 3f).value;
            float masterFactor = DamageDebug ? 3f : DynamicParamManager.QuickGet("HellpodMasterFactor", 3f, 1f, 3f).value;
            float ReductionFactor = hasAccessory ? 0.5f : 1f;
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
                    armorPenetration:5
                );

                if(hasAccessory) player.immuneTime += 30;

                HurtFlag = true;
            }
        }

    }
}