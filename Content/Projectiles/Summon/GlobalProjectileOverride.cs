// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework;
// using Terraria.DataStructures;
// using System;

// namespace SummonerExpansionMod.Content.Items.Weapons.Summon
// {
//     public class MyGlobalProjectile : GlobalProjectile
//     {

//         private bool USE_VANILLA_AI = true;
//         private int AI_SWITCH_CNT = 0;
// 		private int minionPos = 0; // indicate the order of the minion in the summoner

// 		public override bool InstancePerEntity => true;

//         public override void SetDefaults(Projectile projectile)
//         {

//         }

//         // public override void AI(Projectile projectile)
// 		// {
//         //     AI_SWITCH_CNT++;
//         //     if (AI_SWITCH_CNT > 100)
//         //     {
//         //         USE_VANILLA_AI = !USE_VANILLA_AI;
//         //         AI_SWITCH_CNT = 0;
//         //     }

//         //     if (projectile.type == ProjectileID.BabySlime)
//         //     {
// 		// 		minionPos = Main.player[projectile.owner].numMinions;
// 		// 		int num = 60 + 30 * minionPos;
// 		// 		if (Main.player[projectile.owner].dead)
// 		// 		{
// 		// 			Main.player[projectile.owner].slime = false;
// 		// 		}
// 		// 		if (Main.player[projectile.owner].slime)
// 		// 		{
// 		// 			projectile.timeLeft = 2;
// 		// 		}
//         //         // if (USE_VANILLA_AI)
//         //         // {
//         //         //     projectile.aiStyle = 26;
//         //         //     base.AI(projectile);
//         //         //     return;
//         //         // }
//         //         // else
//         //         // {
//         //         //     // projectile.aiStyle = 0;	// bug here
//         //         //     // // base.AI(projectile);
//         //         //     // // return;
//         //         //     // Player owner = Main.player[projectile.owner];

//         //         //     // if (!CheckActive(owner, projectile))
//         //         //     // {
//         //         //     //     return;
//         //         //     // }

//         //         //     // GeneralBehavior(owner, projectile, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
//         //         //     // SearchForTargets(owner, projectile, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
//         //         //     // Movement(projectile, foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
//         //         //     // Visuals(projectile);
//         //         // }
//         //     }
// 		// }

//         private bool CheckActive(Player owner, Projectile projectile)
// 		{
// 			if (owner.dead || !owner.active)
// 			{
// 				owner.ClearBuff(BuffID.BabySlime);

// 				return false;
// 			}

// 			if (owner.HasBuff(BuffID.BabySlime))
// 			{
// 				projectile.timeLeft = 2;
// 			}

// 			return true;
// 		}
    

//         private void GeneralBehavior(Player owner,Projectile projectile, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
// 		{
// 			Vector2 idlePosition = owner.Center;
// 			idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

// 			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
// 			// The index is projectile.minionPos
// 			float minionPositionOffsetX = (10 + projectile.minionPos * 40) * -owner.direction;
// 			idlePosition.X += minionPositionOffsetX; // Go behind the player

// 			// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

// 			// Teleport to player if distance is too big
// 			vectorToIdlePosition = idlePosition - projectile.Center;
// 			distanceToIdlePosition = vectorToIdlePosition.Length();

// 			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
// 			{
// 				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
// 				// and then set netUpdate to true
// 				projectile.position = idlePosition;
// 				projectile.velocity *= 0.1f;
// 				projectile.netUpdate = true;
// 			}

// 			// If your minion is flying, you want to do this independently of any conditions
// 			float overlapVelocity = 0.04f;

// 			// Fix overlap with other minions
// 			foreach (var other in Main.ActiveProjectiles)
// 			{
// 				if (other.whoAmI != projectile.whoAmI && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width)
// 				{
// 					if (projectile.position.X < other.position.X)
// 					{
// 						projectile.velocity.X -= overlapVelocity;
// 					}
// 					else
// 					{
// 						projectile.velocity.X += overlapVelocity;
// 					}

// 					if (projectile.position.Y < other.position.Y)
// 					{
// 						projectile.velocity.Y -= overlapVelocity;
// 					}
// 					else
// 					{
// 						projectile.velocity.Y += overlapVelocity;
// 					}
// 				}
// 			}
// 		}

//         private void SearchForTargets(Player owner, Projectile projectile, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
// 		{
// 			// Starting search distance
// 			distanceFromTarget = 700f;
// 			targetCenter = projectile.position;
// 			foundTarget = false;

// 			// This code is required if your minion weapon has the targeting feature
// 			if (owner.HasMinionAttackTargetNPC)
// 			{
// 				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
// 				float between = Vector2.Distance(npc.Center, projectile.Center);

// 				// Reasonable distance away so it doesn't target across multiple screens
// 				if (between < 2000f)
// 				{
// 					distanceFromTarget = between;
// 					targetCenter = npc.Center;
// 					foundTarget = true;
// 				}
// 			}

// 			if (!foundTarget)
// 			{
// 				// This code is required either way, used for finding a target
// 				foreach (var npc in Main.ActiveNPCs)
// 				{
// 					if (npc.CanBeChasedBy())
// 					{
// 						float between = Vector2.Distance(npc.Center, projectile.Center);
// 						bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
// 						bool inRange = between < distanceFromTarget;
// 						bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
// 						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
// 						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
// 						bool closeThroughWall = between < 100f;

// 						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
// 						{
// 							distanceFromTarget = between;
// 							targetCenter = npc.Center;
// 							foundTarget = true;
// 						}
// 					}
// 				}
// 			}

// 			// friendly needs to be set to true so the minion can deal contact damage
// 			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
// 			// Both things depend on if it has a target or not, so it's just one assignment here
// 			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
// 			projectile.friendly = foundTarget;
// 		}

// 		private void Movement(Projectile projectile, bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
// 		{
// 			// Default movement parameters (here for attacking)
// 			float speed = 8f;
// 			float inertia = 20f;

// 			if (foundTarget)
// 			{
// 				// Minion has a target: attack (here, fly towards the enemy)
// 				if (distanceFromTarget > 40f)
// 				{
// 					// The immediate range around the target (so it doesn't latch onto it when close)
// 					Vector2 direction = targetCenter - projectile.Center;
// 					direction.Normalize();
// 					direction *= speed;

// 					projectile.velocity = (projectile.velocity * (inertia - 1) + direction) / inertia;
// 				}
// 			}
// 			else
// 			{
// 				// Minion doesn't have a target: return to player and idle
// 				if (distanceToIdlePosition > 600f)
// 				{
// 					// Speed up the minion if it's away from the player
// 					speed = 12f;
// 					inertia = 60f;
// 				}
// 				else
// 				{
// 					// Slow down the minion if closer to the player
// 					speed = 4f;
// 					inertia = 80f;
// 				}

// 				if (distanceToIdlePosition > 20f)
// 				{
// 					// The immediate range around the player (when it passively floats about)

// 					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
// 					vectorToIdlePosition.Normalize();
// 					// vectorToIdlePosition *= distanceToIdlePosition > 600f ? speed : distanceToIdlePosition;
// 					// projectile.velocity = vectorToIdlePosition * 0.05f + projectile.velocity * 0.03f;
// 					vectorToIdlePosition *= speed;
// 					projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
// 				}
// 				else if (projectile.velocity == Vector2.Zero)
// 				{
// 					// If there is a case where it's not moving at all, give it a little "poke"
// 					projectile.velocity.X = -0.15f;
// 					projectile.velocity.Y = -0.05f;
// 				}
// 			}
// 		}

// 		private void Visuals(Projectile projectile)
// 		{
// 			// So it will lean slightly towards the direction it's moving
// 			projectile.rotation = projectile.velocity.X * 0.05f;

// 			// This is a simple "loop through all frames from top to bottom" animation
// 			int frameSpeed = 5;

// 			projectile.frameCounter++;

// 			if (projectile.frameCounter >= frameSpeed)
// 			{
// 				projectile.frameCounter = 0;
// 				projectile.frame++;

// 				if (projectile.frame >= Main.projFrames[projectile.type])
// 				{
// 					projectile.frame = 0;
// 				}
// 			}

// 			// Some visuals here
// 			Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.78f);
// 		}
//     }
// }