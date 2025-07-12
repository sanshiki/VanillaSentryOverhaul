using System;
using System.Collections.Generic;

namespace SummonerExpansionMod.Utils
{
	/// <summary>
	/// 通用状态机基类，用于管理召唤物的状态转换
	/// </summary>
	/// <typeparam name="TState">状态枚举类型</typeparam>
	/// <typeparam name="TContext">状态机上下文类型</typeparam>
	public abstract class StateMachine<TState, TContext> where TState : struct, Enum
	{
		#region Fields
		private TState _currentState;
		private readonly Dictionary<TState, StateInfo<TState, TContext>> _states;
		private readonly List<StateTransition<TState, TContext>> _transitions;
		#endregion

		#region Properties
		/// <summary>
		/// 当前状态
		/// </summary>
		public TState CurrentState => _currentState;

		/// <summary>
		/// 状态机是否已初始化
		/// </summary>
		public bool IsInitialized { get; private set; }
		#endregion

		#region Constructor
		protected StateMachine(TState initialState)
		{
			_currentState = initialState;
			_states = new Dictionary<TState, StateInfo<TState, TContext>>();
			_transitions = new List<StateTransition<TState, TContext>>();
		}
		#endregion

		#region State Management
		/// <summary>
		/// 添加状态
		/// </summary>
		/// <param name="state">状态</param>
		/// <param name="onEnter">进入状态时的回调</param>
		/// <param name="onUpdate">状态更新时的回调</param>
		/// <param name="onExit">退出状态时的回调</param>
		protected void AddState(TState state, Action<TContext> onEnter = null, Action<TContext> onUpdate = null, Action<TContext> onExit = null)
		{
			_states[state] = new StateInfo<TState, TContext>(state, onEnter, onUpdate, onExit);
		}

		/// <summary>
		/// 添加状态转换
		/// </summary>
		/// <param name="fromState">起始状态</param>
		/// <param name="toState">目标状态</param>
		/// <param name="condition">转换条件</param>
		protected void AddTransition(TState fromState, TState toState, Func<TContext, bool> condition)
		{
			_transitions.Add(new StateTransition<TState, TContext>(fromState, toState, condition));
		}

		/// <summary>
		/// 初始化状态机
		/// </summary>
		protected virtual void Initialize()
		{
			IsInitialized = true;
			if (_states.TryGetValue(_currentState, out var stateInfo))
			{
				stateInfo.OnEnter?.Invoke(GetContext());
			}
		}

		/// <summary>
		/// 更新状态机（使用抽象方法获取上下文）
		/// </summary>
		public void Update()
		{
			if (!IsInitialized)
			{
				Initialize();
			}

			var context = GetContext();

			// 检查状态转换
			CheckTransitions(context);

			// 更新当前状态
			if (_states.TryGetValue(_currentState, out var stateInfo))
			{
				stateInfo.OnUpdate?.Invoke(context);
			}
		}

		/// <summary>
		/// 更新状态机（使用外部传入的上下文）
		/// </summary>
		/// <param name="context">状态机上下文</param>
		public void Update(TContext context)
		{
			if (!IsInitialized)
			{
				IsInitialized = true;
				if (_states.TryGetValue(_currentState, out var stateInfo))
				{
					stateInfo.OnEnter?.Invoke(context);
				}
			}

			// 检查状态转换
			CheckTransitions(context);

			// 更新当前状态
			if (_states.TryGetValue(_currentState, out var currentStateInfo))
			{
				currentStateInfo.OnUpdate?.Invoke(context);
			}
		}

		/// <summary>
		/// 强制切换到指定状态
		/// </summary>
		/// <param name="newState">新状态</param>
		public void ForceState(TState newState)
		{
			if (EqualityComparer<TState>.Default.Equals(_currentState, newState))
				return;

			var context = GetContext();

			// 退出当前状态
			if (_states.TryGetValue(_currentState, out var currentStateInfo))
			{
				currentStateInfo.OnExit?.Invoke(context);
			}

			// 切换到新状态
			_currentState = newState;

			// 进入新状态
			if (_states.TryGetValue(_currentState, out var newStateInfo))
			{
				newStateInfo.OnEnter?.Invoke(context);
			}
		}

		/// <summary>
		/// 强制切换到指定状态（使用外部上下文）
		/// </summary>
		/// <param name="newState">新状态</param>
		/// <param name="context">状态机上下文</param>
		public void ForceState(TState newState, TContext context)
		{
			if (EqualityComparer<TState>.Default.Equals(_currentState, newState))
				return;

			// 退出当前状态
			if (_states.TryGetValue(_currentState, out var currentStateInfo))
			{
				currentStateInfo.OnExit?.Invoke(context);
			}

			// 切换到新状态
			_currentState = newState;

			// 进入新状态
			if (_states.TryGetValue(_currentState, out var newStateInfo))
			{
				newStateInfo.OnEnter?.Invoke(context);
			}
		}

		/// <summary>
		/// 检查状态转换
		/// </summary>
		private void CheckTransitions(TContext context)
		{
			foreach (var transition in _transitions)
			{
				if (EqualityComparer<TState>.Default.Equals(transition.FromState, _currentState) && 
					transition.Condition(context))
				{
					ForceState(transition.ToState, context);
					break; // 只执行第一个匹配的转换
				}
			}
		}
		#endregion

		#region Abstract Methods
		/// <summary>
		/// 获取状态机上下文（抽象方法，用于向后兼容）
		/// </summary>
		/// <returns>上下文对象</returns>
		protected abstract TContext GetContext();
		#endregion
	}

	#region Supporting Classes
	/// <summary>
	/// 状态信息
	/// </summary>
	internal class StateInfo<TState, TContext> where TState : struct, Enum
	{
		public TState State { get; }
		public Action<TContext> OnEnter { get; }
		public Action<TContext> OnUpdate { get; }
		public Action<TContext> OnExit { get; }

		public StateInfo(TState state, Action<TContext> onEnter, Action<TContext> onUpdate, Action<TContext> onExit)
		{
			State = state;
			OnEnter = onEnter;
			OnUpdate = onUpdate;
			OnExit = onExit;
		}
	}

	/// <summary>
	/// 状态转换
	/// </summary>
	internal class StateTransition<TState, TContext> where TState : struct, Enum
	{
		public TState FromState { get; }
		public TState ToState { get; }
		public Func<TContext, bool> Condition { get; }

		public StateTransition(TState fromState, TState toState, Func<TContext, bool> condition)
		{
			FromState = fromState;
			ToState = toState;
			Condition = condition;
		}
	}
	#endregion
} 