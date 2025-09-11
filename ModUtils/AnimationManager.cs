using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace SummonerExpansionMod.ModUtils
{
	/// <summary>
	/// 动画状态信息
	/// </summary>
	public class AnimationState
	{
		public int FrameSpeed { get; set; }
		public int StartFrame { get; set; }
		public int EndFrame { get; set; }
		public bool ResetToStart { get; set; }
		public float RotationMultiplier { get; set; }
		public bool UseRotation { get; set; }

		public AnimationState(int frameSpeed, int startFrame, int endFrame, bool resetToStart = true, float rotationMultiplier = 0f, bool useRotation = false)
		{
			FrameSpeed = frameSpeed;
			StartFrame = startFrame;
			EndFrame = endFrame;
			ResetToStart = resetToStart;
			RotationMultiplier = rotationMultiplier;
			UseRotation = useRotation;
		}
	}

	/// <summary>
	/// 通用动画管理器
	/// </summary>
	public class AnimationManager
	{
		#region Fields
		private readonly Dictionary<int, AnimationState> _animationStates;
		private int _currentState;
		#endregion

		#region Properties
		/// <summary>
		/// 当前动画状态
		/// </summary>
		public int CurrentState => _currentState;

		/// <summary>
		/// 动画状态数量
		/// </summary>
		public int StateCount => _animationStates.Count;
		#endregion

		#region Constructor
		public AnimationManager()
		{
			_animationStates = new Dictionary<int, AnimationState>();
			_currentState = 0;
		}
		#endregion

		#region State Management
		/// <summary>
		/// 添加动画状态
		/// </summary>
		/// <param name="stateId">状态ID</param>
		/// <param name="animationState">动画状态</param>
		public void AddState(int stateId, AnimationState animationState)
		{
			_animationStates[stateId] = animationState;
		}

		/// <summary>
		/// 设置当前状态
		/// </summary>
		/// <param name="stateId">状态ID</param>
		public void SetState(int stateId)
		{
			if (_animationStates.ContainsKey(stateId))
			{
				_currentState = stateId;
			}
		}

		/// <summary>
		/// 获取当前动画状态
		/// </summary>
		/// <returns>当前动画状态</returns>
		public AnimationState GetCurrentState()
		{
			return _animationStates.TryGetValue(_currentState, out var state) ? state : null;
		}
		#endregion

		#region Animation Update
		/// <summary>
		/// 更新动画
		/// </summary>
		/// <param name="projectile">召唤物</param>
		public void UpdateAnimation(Projectile projectile)
		{
			var currentState = GetCurrentState();
			if (currentState == null) return;

			// 更新帧
			UpdateFrame(projectile, currentState);

			// 更新旋转
			UpdateRotation(projectile, currentState);
		}

		/// <summary>
		/// 更新帧
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="state">动画状态</param>
		private void UpdateFrame(Projectile projectile, AnimationState state)
		{
			projectile.frameCounter++;

			if (projectile.frameCounter >= state.FrameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;

				if (projectile.frame >= state.EndFrame)
				{
					projectile.frame = state.ResetToStart ? state.StartFrame : state.EndFrame - 1;
				}
				else if (projectile.frame < state.StartFrame)
				{
					projectile.frame = state.StartFrame;
				}
			}
		}

		/// <summary>
		/// 更新旋转
		/// </summary>
		/// <param name="projectile">召唤物</param>
		/// <param name="state">动画状态</param>
		private void UpdateRotation(Projectile projectile, AnimationState state)
		{
			if (state.UseRotation)
			{
				projectile.rotation = projectile.velocity.X * state.RotationMultiplier;
			}
			else
			{
				projectile.rotation = 0f;
			}
		}
		#endregion
	}

	/// <summary>
	/// 史莱姆宝宝动画管理器
	/// </summary>
	public class BabySlimeAnimationManager : AnimationManager
	{
		#region Constants
		private const int STABLE_STATE = 0;
		private const int MOVING_STATE = 1;
		private const int FLYING_STATE = 2;

		private const int TOTAL_FRAMES = 6;
		private const int STABLE_FRAME_COUNT = 2;
		private const int FLYING_START_FRAME = 2;
		#endregion

		#region Constructor
		public BabySlimeAnimationManager() : base()
		{
			InitializeStates();
		}
		#endregion

		#region Initialization
		/// <summary>
		/// 初始化动画状态
		/// </summary>
		private void InitializeStates()
		{
			// 稳定状态：帧率20，使用前2帧，不旋转
			AddState(STABLE_STATE, new AnimationState(20, 0, STABLE_FRAME_COUNT, true, 0f, false));

			// 移动状态：帧率5，使用前2帧，不旋转
			AddState(MOVING_STATE, new AnimationState(5, 0, STABLE_FRAME_COUNT, true, 0f, false));

			// 飞行状态：帧率10，使用后4帧，根据速度旋转
			AddState(FLYING_STATE, new AnimationState(10, FLYING_START_FRAME, TOTAL_FRAMES, false, 0.05f, true));
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// 根据状态设置动画
		/// </summary>
		/// <param name="state">史莱姆宝宝状态</param>
		public void SetStateFromBabySlimeState(object state)
		{
			// 使用反射或动态类型来避免直接依赖
			string stateName = state.ToString();
			
			switch (stateName)
			{
				case "Stable":
					SetState(STABLE_STATE);
					break;
				case "Moving":
					SetState(MOVING_STATE);
					break;
				case "Flying":
					SetState(FLYING_STATE);
					break;
			}
		}
		#endregion
	}
} 