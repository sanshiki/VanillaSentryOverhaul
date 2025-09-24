using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using SummonerExpansionMod.Content.Buffs.Summon;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
using SummonerExpansionMod.Initialization;


namespace SummonerExpansionMod.ModUtils
{
	/// <summary>
	/// 召唤物AI通用工具类
	/// </summary>
	public static class MinionAIHelper
	{
		#region Constants
		// 默认距离常量
		public const float DEFAULT_TELEPORT_DISTANCE = 2500f;
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
		/// <returns>搜索结果</returns>
		public static TargetSearchResult SearchForTargets(Player owner, Projectile minion, 
			float searchRange = DEFAULT_TARGET_SEARCH_RANGE, 
			bool checkCanHit = true,
			Func<NPC, bool> otherCondition = null)
		{
			float distanceFromTarget = searchRange;
			Vector2 targetCenter = minion.position;
			bool foundTarget = false;
			NPC targetNPC = null;

			// 检查玩家指定的目标
			if (owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, minion.Center);
				bool canBeChased = npc.CanBeChasedBy(minion);
				bool canHit = checkCanHit ? Collision.CanHitLine(minion.position, minion.width, minion.height, 
																  npc.position, npc.width, npc.height) : true;

				if (distance < searchRange && canHit && canBeChased && (otherCondition == null || otherCondition(npc)))
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
					bool canBeChased = npc.CanBeChasedBy(minion);
					bool canHit = checkCanHit ? Collision.CanHitLine(minion.position, minion.width, minion.height, 
																  npc.position, npc.width, npc.height) : true;
					if (canBeChased && canHit && (otherCondition == null || otherCondition(npc)))
					{
						float distance = Vector2.Distance(npc.Center, minion.Center);
						// bool hasSentryTargetTag = npc.HasBuff(ModBuffID.SentryTarget);
						bool hasSentryTargetTag = false;	// temporarily disabled
						if (distance < distanceFromTarget || hasSentryTargetTag)
						{
							distanceFromTarget = distance;
							targetCenter = npc.Center;
							foundTarget = true;
							targetNPC = npc;
						}

						if (hasSentryTargetTag) break;
					}
				}
			}

			return new TargetSearchResult(foundTarget, distanceFromTarget, targetCenter, targetNPC);
		}

		public static List<int> SearchForProjectiles(int type, Vector2 center, float radius)
		{
			List<int> projectileIDs = new List<int>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile projectile = Main.projectile[i];
				if(projectile.type == type && projectile.active)
				{
					if(projectile.Center.Distance(center) < radius)
					{
						projectileIDs.Add(i);
					}
				}
			}
			return projectileIDs;
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

		#region Prediction Methods
		/// <summary>
		/// 预测目标位置
		/// </summary>
		/// <param name="projectile">发射物</param>
		/// <param name="target">目标</param>
		/// <param name="bulletSpeed">子弹速度</param>
		/// <param name="accelerationFactor">加速度因子</param>
		/// <param name="maxPredictionTicks">最大预测帧数</param>
		/// <param name="tickStep">预测步长</param>
		/// <returns>预测位置</returns>
		public static Vector2 PredictTargetPosition(Projectile projectile, NPC target, float bulletSpeed, int maxPredictionTicks = 60, int tickStep = 3)
		{
			return PredictTargetPosition(projectile.Center, target.Center, target.velocity, bulletSpeed, maxPredictionTicks, tickStep);
		}

		public static Vector2 PredictTargetPosition(Vector2 projectileCenter, Vector2 targetCenter, Vector2 targetVelocity, float bulletSpeed, int maxPredictionTicks = 60, int tickStep = 3)
		{
			Vector2 predictedPos = targetCenter;
			
			for (int tick = 0; tick < maxPredictionTicks; tick += tickStep)
			{
				Vector2 targetPredictedPos = targetCenter + targetVelocity * tick;
				predictedPos = targetPredictedPos;

				if(Collision.SolidCollision(targetPredictedPos, 1, 1)) continue;

				float bulletFlyTime = Vector2.Distance(projectileCenter, targetPredictedPos) / bulletSpeed;

				if (bulletFlyTime < tick)
				{
					break;
				}
			}

			return predictedPos;
		}

		public static float PredictParabolaAngle(Projectile projectile, NPC target,float gravity, float bulletSpeed, int maxPredictionTicks = 60, int tickStep = 3)
		{
			float predictedAngle = (target.Center - projectile.Center).ToRotation();

			for(int tick = 0; tick < maxPredictionTicks; tick += tickStep)
			{
				Vector2 targetPredictedPos = target.Center + target.velocity * tick;

				ParabolaSolution bulletSolution = SolveParabola(bulletSpeed, gravity, targetPredictedPos);

				if(bulletSolution.valid)
				{
					predictedAngle = bulletSolution.angle;
					if(bulletSolution.time < tick)
					{
						Main.NewText("Find solution:" + bulletSolution.time + " " + bulletSolution.angle);
						break;
					}
				}
			}

			return predictedAngle;
		}

		public struct ParabolaSolution
		{
			public float time;
			public float angle;
			public bool valid;

			public ParabolaSolution(float time, float angle, bool valid)
			{
				this.time = time;
				this.angle = angle;
				this.valid = valid;
			}
		}

		public static ParabolaSolution SolveParabola(float speed, float gravity, Vector2 goal)
		{
			float v = speed;
			float g = gravity;
			float x = goal.X;
			float y = goal.Y;

			float A = (g*g) / 4f;
			float B = y * g - v * v;
			float C = x * x + y * y;
			float D = B * B - 4 * A * C;
			if(D < 0)
			{
				return new ParabolaSolution(-1f, -1f, false);
			}
			
			float sqrtD = (float)Math.Sqrt(D);
			float[] t2Candidates = new float[]
			{
				(-B + sqrtD) / (2 * A),
				(-B - sqrtD) / (2 * A)
			};
			float minTime = float.MaxValue;
			float bestAngle = -1;

			foreach(float t2 in t2Candidates)
			{
				if(t2 > 0)
				{
					float t = (float)Math.Sqrt(t2);
					float cosTheta = x / (t * v);
					float sinTheta = (y + g * t2 / 2) / (t * v);
					float angle = (float)Math.Atan2(sinTheta, cosTheta);
					if(cosTheta >= 0f && cosTheta <= 1f && (float)Math.Abs(sinTheta) <= 1f)
					{
						if(t < minTime)
						{
							minTime = t;
							bestAngle = angle;
						}
					}
				}
			}

			if(minTime == float.MaxValue)
			{
				return new ParabolaSolution(-1f, -1f, false);
			}

			return new ParabolaSolution(minTime, bestAngle, true);
		}

		/// <summary>
		/// 预测目标位置（带加速度计算）
		/// </summary>
		/// <param name="projectile">发射物</param>
		/// <param name="target">目标</param>
		/// <param name="bulletSpeed">子弹速度</param>
		/// <param name="lastVelocity">上一帧速度</param>
		/// <param name="accelerationFactor">加速度因子</param>
		/// <param name="maxPredictionTicks">最大预测帧数</param>
		/// <param name="tickStep">预测步长</param>
		/// <returns>预测位置和当前速度</returns>
		public static (Vector2 PredictedPosition, Vector2 CurrentVelocity) PredictTargetPositionWithAcceleration(
			Projectile projectile, NPC target, float bulletSpeed, Vector2 lastVelocity, 
			float accelerationFactor = 0.05f, int maxPredictionTicks = 60, int tickStep = 3)
		{
			Vector2 predictedPos = target.Center;
			Vector2 acceleration = (target.velocity - lastVelocity) * accelerationFactor;
			Vector2 currentVelocity = target.velocity;

			for (int tick = 0; tick < maxPredictionTicks; tick += tickStep)
			{
				Vector2 targetPredictedPos = target.Center + target.velocity * tick + 
					0.5f * acceleration * tick * tick;
				predictedPos = targetPredictedPos;

				float bulletFlyTime = Vector2.Distance(projectile.Center, targetPredictedPos) / bulletSpeed;

				if (bulletFlyTime < tick)
				{
					break;
				}
			}

			return (predictedPos, currentVelocity);
		}
		#endregion

		#region Utility Methods

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


		public static float NormalizeAngle(float angle)
		{
			return (angle + ModGlobal.PI_FLOAT) % ModGlobal.TWO_PI_FLOAT - ModGlobal.PI_FLOAT;
		}
		#endregion

		#region Texture Methods
		/// <summary>
		/// 绘制纹理
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="texture">纹理</param>
		/// <param name="worldPos">世界坐标（屏幕坐标，屏幕左上角为原点），由本地坐标使用ConvertToWorldPos转换得到</param>
		/// <param name="rect">矩形（贴图坐标，贴图左上角为原点）</param>
		/// <param name="color">颜色</param>
		/// <param name="origin">原点（碰撞箱坐标，碰撞箱中心为原点）</param>
		/// <param name="rotation">旋转角度</param>
		public static void DrawPart(Projectile projectile, Texture2D texture, Vector2 worldPos, Rectangle rect, Color color,float rotation, Vector2 origin)
        {
            Main.spriteBatch.Draw(
                texture,
                worldPos,
                rect,
                color,
                rotation,
                origin,
                projectile.scale,
                projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0f
            );
        }

		/// <summary>
		/// 转换为世界坐标
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="localPos">本地坐标（碰撞箱坐标，碰撞箱左上角为原点）</param>
		/// <returns>世界坐标（屏幕坐标，屏幕左上角为原点）</returns>
		public static Vector2 ConvertToWorldPos(Projectile projectile, Vector2 localPos)
        {
            return projectile.Center + localPos.RotatedBy(projectile.rotation) - Main.screenPosition;
        }

		public static Vector2 CenterMapping(Vector2 center, Vector2 offset, float rotation)
		{
			return center + offset.RotatedBy(rotation);
		}

		/// <summary>
		/// 计算裁剪矩形
		/// </summary>
		/// <param name="rect">矩形</param>
		/// <param name="pos">位置</param>
		/// <param name="origin">原点</param>
		/// <param name="threshold_y">阈值，映射后的Y坐标低于这个阈值的矩形部分需要裁剪</param>
		public static Rectangle CalculateClipRect(Rectangle rect, Vector2 pos, Vector2 origin,float threshold_x = -1f, float threshold_y = -1f)
		{
			// float Vertex_Y1 = pos.Y - origin.Y;
			// float Vertex_Y2 = pos.Y + rect.Height - origin.Y;
			Vector2 Vertex_1 = pos - origin;
			Vector2 Vertex_2 = pos - origin + new Vector2(rect.Width, rect.Height);
			int clip_width = rect.Width;
			int clip_height = rect.Height;
			if(threshold_x > -1f && Vertex_2.X > threshold_x)
			{
				clip_width = (int)(threshold_x - Vertex_1.X);
				if(clip_width > rect.Width)
				{
					clip_width = rect.Width;
				}
				else if (clip_width < 0)
				{
					clip_width = 0;
				}
			}
			if(threshold_y > -1f && Vertex_2.Y > threshold_y)
			{
				clip_height = (int)(threshold_y - Vertex_1.Y);
				if(clip_height > rect.Height)
				{
					clip_height = rect.Height;
				}
				else if (clip_height < 0)
				{
					clip_height = 0;
				}
			}
			Rectangle clip_rect = new Rectangle(rect.X, rect.Y, clip_width, clip_height);
			return clip_rect;
		}
		#endregion

		#region Random Methods
		public static float RandomFloat(float min, float max)
		{
			return (float)Main.rand.NextDouble() * (max - min) + min;
		}
		public static float RandomFloat(double min, double max)
		{
			return (float)(Main.rand.NextDouble() * (max - min) + min);
		}
		public static float RandomFloat(float min, double max)
		{
			return (float)Main.rand.NextDouble() * ((float)max - min) + min;
		}
		public static float RandomFloat(double min, float max)
		{
			return (float)Main.rand.NextDouble() * (max - (float)min) + (float)min;
		}
		public static bool RandomBool()
		{
			return Main.rand.Next(2) == 1;
		}
		public static float RandomSign()
		{
			return Main.rand.Next(2) == 1 ? 1 : -1;
		}
		#endregion

		#region Dust Methods
		public static void GenerateLightning(Vector2 start, Vector2 end, float displacement, float minDisplacement, List<Vector2> points)
		{
			if (displacement < minDisplacement)
			{
				points.Add(start);
				points.Add(end);
			}
			else
			{
				Vector2 mid = (start + end) / 2;
				// 垂直方向
				Vector2 dir = new Vector2(end.Y - start.Y, start.X - end.X);
				dir.Normalize();
				// 随机偏移
				mid += dir * (RandomFloat(-displacement, displacement));

				// 递归生成
				GenerateLightning(start, mid, displacement / 2, minDisplacement, points);
				GenerateLightning(mid, end, displacement / 2, minDisplacement, points);
			}
		}
		#endregion

		#region Homein Methods
		public static void HomeinToTarget(Projectile projectile, Vector2 target, float speed, float inertia)
		{
			Vector2 direction = target - projectile.Center;
			direction.Normalize();
			direction *= speed;
			projectile.velocity = (projectile.velocity * (inertia - 1) + direction) / inertia;
		}
		#endregion
	}
} 