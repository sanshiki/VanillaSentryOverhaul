using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria;
using SummonerExpansionMod.Content.Projectiles.Summon;
using SummonerExpansionMod.Content.Buffs.Summon;

namespace SummonerExpansionMod.Initialization
{
    public class ModIDLoader
    {
        public static void Load()
        {
			/* ------------------------------- projectiles ------------------------------- */
            // sentries
			ModProjectileID.DarkMagicTower = ModContent.ProjectileType<DarkMagicTower>();
			ModProjectileID.BunnySentry = ModContent.ProjectileType<BunnySentry>();
			ModProjectileID.MachineGunSentry = ModContent.ProjectileType<MachineGunSentry>();
			ModProjectileID.RocketSentry = ModContent.ProjectileType<RocketSentry>();
			ModProjectileID.GatlingSentry = ModContent.ProjectileType<GatlingSentry>();
			ModProjectileID.AutocannonSentry = ModContent.ProjectileType<AutocannonSentry>();
			ModProjectileID.TempleSentry = ModContent.ProjectileType<TempleSentry>();

			// sentry projectiles
			ModProjectileID.AntlionSentryBullet = ModContent.ProjectileType<AntlionSentryBullet>();
			ModProjectileID.TowerOfDryadsBlessingProjectile = ModContent.ProjectileType<TowerOfDryadsBlessingProjectile>();
			ModProjectileID.MachineGunSentryBullet = ModContent.ProjectileType<MachineGunSentryBullet>();
			ModProjectileID.HoneyCombSentryBullet = ModContent.ProjectileType<HoneyCombSentryBullet>();
			ModProjectileID.DarkMagicTowerBullet = ModContent.ProjectileType<DarkMagicTowerBullet>();
			ModProjectileID.CursedMagicTowerBulletSmall = ModContent.ProjectileType<CursedMagicTowerBulletSmall>();
			ModProjectileID.CursedMagicTowerBulletSphere = ModContent.ProjectileType<CursedMagicTowerBulletSphere>();
			ModProjectileID.RocketSentryBullet = ModContent.ProjectileType<RocketSentryBullet>();
			ModProjectileID.AutocannonSentryBullet = ModContent.ProjectileType<AutocannonSentryBullet>();
			ModProjectileID.TempleSentryHeatRay = ModContent.ProjectileType<TempleSentryHeatRay>();
			ModProjectileID.StardustSentrySignal = ModContent.ProjectileType<StardustSentrySignal>();
			ModProjectileID.StardustSentryBullet = ModContent.ProjectileType<StardustSentryBullet>();
			// minion
			ModProjectileID.SentryPlatform = ModContent.ProjectileType<SentryPlatform>();

			// flag weapon
			ModProjectileID.FlagProjectile = ModContent.ProjectileType<FlagProjectile>();
			ModProjectileID.PirateFlagProjectile = ModContent.ProjectileType<PirateFlagProjectile>();
			ModProjectileID.NormalFlagProjectile = ModContent.ProjectileType<NormalFlagProjectile>();
			ModProjectileID.GoblinFlagProjectile = ModContent.ProjectileType<GoblinFlagProjectile>();
			ModProjectileID.HellFlagProjectile = ModContent.ProjectileType<HellFlagProjectile>();
			ModProjectileID.HolyFlagProjectile = ModContent.ProjectileType<HolyFlagProjectile>();
			ModProjectileID.TikiFlagProjectile = ModContent.ProjectileType<TikiFlagProjectile>();
			ModProjectileID.TikiFlagBladeShot = ModContent.ProjectileType<TikiFlagBladeShot>();
			ModProjectileID.SantaFlagProjectile = ModContent.ProjectileType<SantaFlagProjectile>();
			ModProjectileID.SantaFlagBladeShot = ModContent.ProjectileType<SantaFlagBladeShot>();
			ModProjectileID.OneTrueFlagProjectile = ModContent.ProjectileType<OneTrueFlagProjectile>();
			ModProjectileID.OneTrueFlagBladeShot = ModContent.ProjectileType<OneTrueFlagBladeShot>();

			/* ------------------------------- buffs ------------------------------- */
			ModBuffID.SentryEnhancement = ModContent.BuffType<SentryEnhancementBuff>();
			ModBuffID.SentryTarget = ModContent.BuffType<SentryTargetBuff>();
			ModBuffID.ElectricShock = ModContent.BuffType<ElectricShock>();
			ModBuffID.NormalFlagBuff = ModContent.BuffType<NormalFlagBuff>();
			ModBuffID.GoblinFlagBuff = ModContent.BuffType<GoblinFlagBuff>();
			ModBuffID.HellFlagBuff = ModContent.BuffType<HellFlagBuff>();
			ModBuffID.PirateFlagBuff = ModContent.BuffType<PirateFlagBuff>();
			ModBuffID.PirateFlagDebuff = ModContent.BuffType<PirateFlagDebuff>();
			ModBuffID.HolyFlagBuff = ModContent.BuffType<HolyFlagBuff>();
			ModBuffID.TikiFlagBuff = ModContent.BuffType<TikiFlagBuff>();
			ModBuffID.TikiFlagDebuff = ModContent.BuffType<TikiFlagDebuff>();
			ModBuffID.SantaFlagBuff = ModContent.BuffType<SantaFlagBuff>();
			ModBuffID.OneTrueFlagBuff = ModContent.BuffType<OneTrueFlagBuff>();
			ModBuffID.OneTrueFlagDebuff = ModContent.BuffType<OneTrueFlagDebuff>();
        }

		
    }
}