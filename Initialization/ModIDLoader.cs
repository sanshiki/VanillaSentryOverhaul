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
			// minion
			ModProjectileID.SentryPlatform = ModContent.ProjectileType<SentryPlatform>();

			// flag weapon
			ModProjectileID.FlagProjectile = ModContent.ProjectileType<FlagProjectile>();

			/* ------------------------------- buffs ------------------------------- */
			ModBuffID.SentryEnhancement = ModContent.BuffType<SentryEnhancementBuff>();
			ModBuffID.SentryTarget = ModContent.BuffType<SentryTargetBuff>();
			ModBuffID.ElectricShock = ModContent.BuffType<ElectricShock>();
        }

		
    }
}