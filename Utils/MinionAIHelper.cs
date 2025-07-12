using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SummonerExpansionMod.Utils
{
	/// <summary>
	/// 召唤物AI通用工具类
	/// </summary>
	public static class MinionAIHelper
	{
		#region Constants
		// 默认距离常量
		public const float DEFAULT_TELEPORT_DISTANCE = 2000f;
		public const float DEFAULT_TARGET_SEARCH_RANGE = 700f;
		public const float DEFAULT_MAX_TARGET_DISTANCE = 2000f;
		public const float DEFAULT_CLOSE_THROUGH_WALL_DISTANCE = 100f;
		public const float DEFAULT_MINION_SPACING = 40f;
		public const float DEFAULT_OVERLAP_VELOCITY = 0.04f;
		#endregion

		#region Position and Movement
		/// <summary>
		/// 计算召唤物的理想位置
		/// </summary>
		/// <param name="owner">召唤物主人</param>
		/// <param name="projectile">召唤物</param>
		/// <param name="spacing">召唤物间距</param>
		/// <returns>理想位置</returns>
		public static Vector2 CalculateIdlePosition(Player owner, Projectile projectile, float spacing = DEFAULT_MINION_SPACING)
		{
			Vector2 idlePosition = owner.Center;
			float minionOffset = (spacing + projectile.minionPos * spacing) * -owner.direction;
			idlePosition.X += minionOffset;
			return idlePosition;
		}

		/// <summary>
		/// 处理距离过远时的传送
		/// </summary>
		/// <param name="owner">召唤物主人</param>
		/// <param name="projectile">召唤物</param>
		/// <param name="idlePosition">理想位置</param>
		/// <param name="teleportDistance">传送距离</param>
		public static void HandleTeleportation(Player owner, Projectile projectile, Vector2 idlePosition, float teleportDistance = DEFAULT_TELEPORT_DISTANCE)
		{
			float distanceToIdle = Vector2.Distance(projectile.Center, idlePosition);
			
			if (Main.myPlayer == owner.whoAmI && distanceToIdle > teleportDistance)
			{
				projectile.position = idlePosition;
				projectile.velocity *= 0.1f;
				projectile.netUpdate = true;
			}
		}

		/// <summary>
		/// 处理与其他召唤物的重叠
		/// </summary>
		/// <param name="projectile">当前召唤物</param>
		/// <param name="overlapVelocity">重叠修正速度</param>
		public static void HandleMinionOverlap(Projectile projectile, float overlapVelocity = DEFAULT_OVERLAP_VELOCITY)
		{
			foreach (var other in Main.ActiveProjectiles)
			{
				if (other.whoAmI != projectile.whoAmI && 
					other.owner == projectile.owner && 
					IsOverlapping(projectile, other))
				{
					ApplyOverlapCorrection(projectile, other, overlapVelocity);
				}
			}
		}

		/// <summary>
		/// 检查是否与其他召唤物重叠
		/// </summary>
		/// <param name="projectile">当前召唤物</param>
		/// <param name="other">其他召唤物</param>
		/// <returns>是否重叠</returns>
		public static bool IsOverlapping(Projectile projectile, Projectile other)
		{
			return Math.Abs(projectile.position.X - other.position.X) + 
				   Math.Abs(projectile.position.Y - other.position.Y) < projectile.width;
		}

		/// <summary>
		/// 应用重叠修正
		/// </summary>
		/// <param name="projectile">当前召唤物</param>
		/// <param name="other">其他召唤物</param>
		/// <param name="overlapVelocity">重叠修正速度</param>
		public static void ApplyOverlapCorrection(Projectile projectile, Projectile other, float overlapVelocity)
		{
			if (projectile.position.X < other.position.X)
				projectile.velocity.X -= overlapVelocity;
			else
				projectile.velocity.X += overlapVelocity;

			if (projectile.position.Y < other.position.Y)
				projectile.velocity.Y -= overlapVelocity;
			else
				projectile.velocity.Y += overlapVelocity;
		}
		#endregion

		#region Target Search
		/// <summary>
		/// 搜索目标的结果
		/// </summary>
		public struct TargetSearchResult
		{
			public bool FoundTarget;
			public float DistanceFromTarget;
			public Vector2 TargetCenter;
			public NPC TargetNPC;

			public TargetSearchResult(bool foundTarget, float distance, Vector2 center, NPC npc = null)
			{
				FoundTarget = foundTarget;
				DistanceFromTarget = distance;
				TargetCenter = center;
				TargetNPC = npc;
			}
		}

		/// <summary>
		/// 搜索目标
		/// </summary>
		/// <param name="owner">召唤物主人</param>
		/// <param name="projectile">召唤物</param>
		/// <param name="searchRange">搜索范围</param>
		/// <param name="maxDistance">最大目标距离</param>
		/// <param name="closeThroughWallDistance">穿墙近距离</param>
		/// <returns>搜索结果</returns>
		public static TargetSearchResult SearchForTargets(Player owner, Projectile projectile, 
			float searchRange = DEFAULT_TARGET_SEARCH_RANGE, 
			float maxDistance = DEFAULT_MAX_TARGET_DISTANCE,
			float closeThroughWallDistance = DEFAULT_CLOSE_THROUGH_WALL_DISTANCE)
		{
			float distanceFromTarget = searchRange;
			Vector2 targetCenter = projectile.position;
			bool foundTarget = false;
			NPC targetNPC = null;

			// 检查玩家指定的目标
			if (owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, projectile.Center);

				if (distance < maxDistance)
				{
					distanceFromTarget = distance;
					targetCenter = npc.Center;
					foundTarget = true;
					targetNPC = npc;
				}
			}

			// 搜索附近的目标
			if (!foundTarget)
			{
				foreach (var npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy())
					{
						float distance = Vector2.Distance(npc.Center, projectile.Center);
						bool isClosest = Vector2.Distance(projectile.Center, targetCenter) > distance;
						bool inRange = distance < distanceFromTarget;
						bool hasLineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, 
																  npc.position, npc.width, npc.height);
						bool isCloseThroughWall = distance < closeThroughWallDistance;

						if (((isClosest && inRange) || !foundTarget) && (hasLineOfSight || isCloseThroughWall))
						{
							distanceFromTarget = distance;
							targetCenter = npc.Center;
							foundTarget = true;
							targetNPC = npc;
						}
					}
				}
			}

			return new TargetSearchResult(foundTarget, distanceFromTarget, targetCenter, targetNPC);
		}

		/// <summary>
		/// 更新召唤物的友好状态
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="hasTarget">是否有目标</param>
		public static void UpdateFriendlyState(Projectile projectile, bool hasTarget)
		{
			projectile.friendly = hasTarget;
		}
		#endregion

		#region Movement Helpers
		/// <summary>
		/// 应用重力
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="gravity">重力加速度</param>
		/// <param name="maxFallSpeed">最大下落速度</param>
		public static void ApplyGravity(Projectile projectile, float gravity = 0.4f, float maxFallSpeed = 10f)
		{
			projectile.velocity.Y += gravity;
			if (projectile.velocity.Y > maxFallSpeed)
			{
				projectile.velocity.Y = maxFallSpeed;
			}
		}

		/// <summary>
		/// 应用平滑移动
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="targetVelocity">目标速度</param>
		/// <param name="inertia">惯性系数</param>
		public static void ApplySmoothMovement(Projectile projectile, Vector2 targetVelocity, float inertia)
		{
			projectile.velocity = (projectile.velocity * (inertia - 1) + targetVelocity) / inertia;
		}

		/// <summary>
		/// 应用水平平滑移动
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="targetSpeedX">目标水平速度</param>
		/// <param name="inertia">惯性系数</param>
		public static void ApplySmoothHorizontalMovement(Projectile projectile, float targetSpeedX, float inertia)
		{
			float currentSpeed = projectile.velocity.X;
			projectile.velocity.X = (currentSpeed * (inertia - 1) + targetSpeedX) / inertia;
		}

		/// <summary>
		/// 检查是否卡在固体中
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <returns>是否卡在固体中</returns>
		public static bool IsStuckInSolid(Projectile projectile)
		{
			return Collision.SolidCollision(projectile.position, projectile.width, projectile.height);
		}
		#endregion

		#region Animation Helpers
		/// <summary>
		/// 更新精灵方向
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="threshold">速度阈值</param>
		public static void UpdateSpriteDirection(Projectile projectile, float threshold = 0.01f)
		{
			projectile.spriteDirection = projectile.velocity.X > threshold ? -1 :
										projectile.velocity.X < -threshold ? 1 :
										projectile.spriteDirection;
		}

		/// <summary>
		/// 更新动画帧
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="frameSpeed">帧率</param>
		/// <param name="maxFrames">最大帧数</param>
		/// <param name="resetToZero">是否重置到第0帧</param>
		public static void UpdateAnimationFrame(Projectile projectile, int frameSpeed, int maxFrames, bool resetToZero = true)
		{
			projectile.frameCounter++;

			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;

				if (projectile.frame >= maxFrames)
				{
					projectile.frame = resetToZero ? 0 : maxFrames - 1;
				}
			}
		}
		#endregion

		#region Utility Methods
		/// <summary>
		/// 检查召唤物是否应该保持活跃状态
		/// </summary>
		/// <param name="owner">召唤物主人</param>
		/// <param name="buffType">关联的Buff类型</param>
		/// <returns>是否应该保持活跃</returns>
		public static bool IsActive(Player owner, int buffType)
		{
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(buffType);
				return false;
			}
            return true;

			// if (owner.HasBuff(buffType))
			// {
			// 	return true;
			// }

			// return false;
		}

		/// <summary>
		/// 计算两点之间的距离
		/// </summary>
		/// <param name="point1">点1</param>
		/// <param name="point2">点2</param>
		/// <returns>距离</returns>
		public static float GetDistance(Vector2 point1, Vector2 point2)
		{
			return Vector2.Distance(point1, point2);
		}

		/// <summary>
		/// 计算水平距离
		/// </summary>
		/// <param name="point1">点1</param>
		/// <param name="point2">点2</param>
		/// <returns>水平距离</returns>
		public static float GetHorizontalDistance(Vector2 point1, Vector2 point2)
		{
			return Math.Abs(point1.X - point2.X);
		}
		#endregion
	}
} 