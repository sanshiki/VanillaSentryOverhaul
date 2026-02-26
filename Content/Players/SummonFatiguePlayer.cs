using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Content.NPCs;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;

namespace SummonerExpansionMod.Content.Players
{
    public class SummonFatiguePlayer : ModPlayer
    {
        private float minionCount = 0f;
        private float sentryCount = 0f;

        public float fatigueLevel = 0f;
        public float fatigueMultiplier = 1f;

        public override void ResetEffects()
        {
            minionCount = 0f;
            sentryCount = 0f;
            fatigueLevel = 0f;
            fatigueMultiplier = 1f;
        }

        public override void PostUpdate()
        {
            minionCount = 0f;
            sentryCount = 0f;

            foreach (Projectile p in Main.projectile)
            {
                if (!p.active || p.owner != Player.whoAmI)
                    continue;

                if (p.minion)
                    minionCount+=p.minionSlots;

                if (p.sentry)
                    sentryCount++;
            }

            fatigueLevel = Math.Min(minionCount / (Player.maxMinions), sentryCount / (Player.maxTurrets));

            // Main.NewText("minionCount: " + minionCount + " sentryCount: " + sentryCount + " fatigueLevel: " + fatigueLevel);

            fatigueMultiplier = MathHelper.Clamp(1-fatigueLevel*fatigueLevel*1.6f, 0.05f, 0.95f);

            if(fatigueLevel > 0f)
            {
                Player.AddBuff(ModContent.BuffType<SummonFatigueDebuff>(), 2);
            }
        }

        // public override void ModifyHitNPCWithProj(
        //     Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        // {
        //     // if (!proj.minion && !ProjectileID.Sets.MinionShot[proj.type] && !ProjectileID.Sets.SentryShot[proj.type] && !proj.IsMinionOrSentryRelated && !proj.sentry)
        //     //     return;
        //     if (proj.DamageType != DamageClass.Summon && !proj.DamageType != DamageClass.SummonMeleeSpeed)
        //         return;

        //     if (fatigueLevel <= 0)
        //         return;

        //     modifiers.FinalDamage *= fatigueMultiplier;

        //     // Main.NewText("Fatigue level: " + fatigueLevel + " Multiplier: " + multiplier);
        // }
    }

    public class SummonFatigueSentryProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitNPC(
            Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[projectile.owner];
            var mp = player.GetModPlayer<SummonFatiguePlayer>();
            float fatigueLevel = mp.fatigueLevel;

            // if (!projectile.minion && !ProjectileID.Sets.MinionShot[projectile.type] && !ProjectileID.Sets.SentryShot[projectile.type] && !projectile.IsMinionOrSentryRelated && !projectile.sentry)
            if (!(projectile.DamageType == DamageClass.Summon) && !(projectile.DamageType == DamageClass.SummonMeleeSpeed))
                return;

            if (fatigueLevel <= 0)
                return;

            modifiers.FinalDamage *= mp.fatigueMultiplier;

            // Main.NewText("Fatigue level: " + mp.fatigueLevel + " Multiplier: " + multiplier);
        }
    }
}