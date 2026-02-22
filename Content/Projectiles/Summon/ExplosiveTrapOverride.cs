using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.WorldBuilding;

using Microsoft.Xna.Framework.Graphics;
using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.ModUtils;
using SummonerExpansionMod.Initialization;
using Terraria.Audio;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
	public class ExplosiveTrapOverride : IProjectileOverride
	{
		private Vector2 lastVelocity = new Vector2(0, 0);
		public ExplosiveTrapOverride()
		{
			RegisterFlags["OnTileCollide"] = true;
			RegisterFlags["PostAI"] = true;
		}

		public override void PostAI(Projectile projectile)
		{
			if(projectile.velocity.X != lastVelocity.X) projectile.tileCollide = false;
			else projectile.tileCollide = true;
			lastVelocity = projectile.velocity;
		}

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			if(projectile.velocity.X != 0f) projectile.netUpdate = true;
			projectile.velocity.X = 0f;
			lastVelocity = new Vector2(0, 0);
			return true;
		}
	}
}