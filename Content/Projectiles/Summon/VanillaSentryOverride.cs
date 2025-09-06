using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System;

using System.Collections.Generic;

using SummonerExpansionMod.Initialization;
using SummonerExpansionMod.Content.Projectiles.Summon;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
	public interface IProjectileOverride
	{
		void SetDefaults(Projectile projectile);
		bool PreAI(Projectile projectile);
		void AI(Projectile projectile);
		bool OnTileCollide(Projectile projectile, Vector2 oldVelocity);
		void Kill(Projectile projectile, int timeLeft);
	}
	
	public class VanillaSentryOverride : GlobalProjectile
	{

		public override bool InstancePerEntity => true;

		private readonly Dictionary<int, IProjectileOverride> overrides = new()
		{
			{ ProjectileID.DD2BallistraTowerT1, new BallistaTowerT1Override() },
			{ ProjectileID.DD2BallistraTowerT2, new BallistaTowerT2Override() },
			{ ProjectileID.DD2BallistraTowerT3, new BallistaTowerT3Override() },
			// 以后只需要在这里加映射
		};

		public override void SetDefaults(Projectile projectile)
		{
			if (overrides.TryGetValue(projectile.type, out var handler))
				handler.SetDefaults(projectile);
			else
				base.SetDefaults(projectile);
		}

		public override bool PreAI(Projectile projectile)
		{
			if (overrides.TryGetValue(projectile.type, out var handler))
				return handler.PreAI(projectile);
			return base.PreAI(projectile);
		}

		public override void AI(Projectile projectile)
		{
			if (overrides.TryGetValue(projectile.type, out var handler))
				handler.AI(projectile);
			else
				base.AI(projectile);
		}

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			if (overrides.TryGetValue(projectile.type, out var handler))
				return handler.OnTileCollide(projectile, oldVelocity);
			return base.OnTileCollide(projectile, oldVelocity);
		}

		public override void Kill(Projectile projectile, int timeLeft)
		{
			if (overrides.TryGetValue(projectile.type, out var handler))
				handler.Kill(projectile, timeLeft);
		}
	}

}