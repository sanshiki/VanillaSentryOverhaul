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
	public class MoonlordTurretOverride : IProjectileOverride
	{
		private Vector2 lastVelocity = new Vector2(0, 0);
        private int SpawnCnt = 0;
        private const float DEACCELERATION = 0.5f;
		public MoonlordTurretOverride()
		{
            RegisterFlags["PreAI"] = true;
		}

        public override bool PreAI(Projectile projectile)
        {
            SpawnCnt++;
            if(SpawnCnt >= 5)
            {
                Vector2 vel = lastVelocity;
                Vector2 vel_dir = vel.SafeNormalize(Vector2.Zero);
                if(vel.Length() > DEACCELERATION)
                {
                    lastVelocity -= vel_dir * DEACCELERATION;
                }
                else
                {
                    lastVelocity = Vector2.Zero;
                }
                // apply velocity
                projectile.Center += lastVelocity;
                if(!(projectile.velocity == Vector2.Zero && lastVelocity != Vector2.Zero))
                    lastVelocity = projectile.velocity;
                SpawnCnt = 5;
            }

            return true;
        }
	}
}