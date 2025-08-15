using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using SummonerExpansionMod.Content.Buffs.Summon;
using SummonerExpansionMod.Utils;

namespace SummonerExpansionMod.Content.Projectiles.Summon
{
    public class DarkMagicTower : ModProjectile
    {
        // private int fireCooldown = 30;
        private const int FIRE_INTERVAL = 30;
        private int fireTimer = 0;

        private const float ENHANCEMENT_FACTOR = 0.75f;
        private int BUFF_ID = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 112;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.sentry = true;
            Projectile.netImportant = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            
            BUFF_ID = ModBuffID.SentryEnhancement;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 0.5f); // Add a faint magic glow

            // Float in the air
            Projectile.velocity = Vector2.Zero;
            Projectile.position.Y += (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.5f;

            // teleport to owner if needed
            float maxDistance = 2000f;
            Player owner = Main.player[Projectile.owner];
            if (Vector2.Distance(Projectile.Center, owner.Center) > maxDistance)
            {
                TryTeleportNearPlayer(owner);
            }

            int fireInterval = FIRE_INTERVAL;
            if(owner.HasBuff(BUFF_ID))
            {
                fireInterval = (int)(fireInterval * ENHANCEMENT_FACTOR);
            }

            // find target and fire, as well as cooldown control
            // NPC target = FindTarget();
            NPC target = MinionAIHelper.SearchForTargets(
                owner, 
                Projectile, 
                1000f, 
                true, 
                null).TargetNPC;
            if (target != null && fireTimer >= fireInterval)
            {
                FireAt(target);
                fireTimer = 0;
            }

            fireTimer++;
        }

        private NPC FindTarget()
        {
            float maxDetectDistance = 1000f;
            NPC closestNPC = null;
            float closestDistance = maxDetectDistance;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(this))
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            return closestNPC;
        }

        private void FireAt(NPC target)
        {
            Vector2 distanceToTarget = target.Center - Projectile.Center;
            float speed = 8f;
            float hitTime = distanceToTarget.Length() / speed;
            float heightDiff = distanceToTarget.Y;
            float gravity = 0.1f;
            float predictedFallDist = 0.5f * gravity * hitTime * hitTime;
            distanceToTarget.Y -= predictedFallDist;
            Vector2 direction = distanceToTarget;
            direction.Normalize();
            

            Vector2 CrystalOffset = new Vector2(0, -30f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                Projectile.Center + CrystalOffset,
                direction * speed,
                ProjectileID.WaterStream, // Reusing vanilla projectile
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            SoundEngine.PlaySound(SoundID.Item13, Projectile.Center);
        }

        private void TryTeleportNearPlayer(Player player)
        {
            Vector2 bestPosition = Projectile.Center;
            int bestTileCount = int.MaxValue;

            for (int xOffset = -5; xOffset <= 5; xOffset++)
            {
                for (int yOffset = -5; yOffset <= 5; yOffset++)
                {
                    Vector2 checkPos = player.Center + new Vector2(xOffset * 16, yOffset * 16);
                    if (!Collision.SolidCollision(checkPos, Projectile.width, Projectile.height))
                    {
                        int tileCount = CountNearbySolidTiles(checkPos);
                        if (tileCount < bestTileCount)
                        {
                            bestTileCount = tileCount;
                            bestPosition = checkPos;
                        }
                    }
                }
            }

            // add random offset to best position
            bestPosition += new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10));

            // Teleport if we found a better position
            Projectile.position = bestPosition;
            Projectile.netUpdate = true;

            // Optional: Visual or sound effect
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(bestPosition, Projectile.width, Projectile.height, DustID.MagicMirror, Scale: 1.5f);
                }
                SoundEngine.PlaySound(SoundID.Item8, bestPosition);
            }
        }

        private int CountNearbySolidTiles(Vector2 center)
        {
            int tileCount = 0;
            Point tileCenter = center.ToTileCoordinates();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Tile tile = Framing.GetTileSafely(tileCenter.X + x, tileCenter.Y + y);
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        tileCount++;
                    }
                }
            }

            return tileCount;
        }

        public override bool MinionContactDamage()
		{
			return false;
		}
    }
}