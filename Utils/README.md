# SummonerExpansionMod Utils

这个文件夹包含了召唤师扩展模组中可复用的工具类和组件。

## 架构概述

Utils模块采用了模块化设计，将召唤物AI的各个功能分离到不同的工具类中：

### 核心组件

1. **StateMachine.cs** - 通用状态机基类
2. **MinionAIHelper.cs** - 召唤物AI通用工具类
3. **AnimationManager.cs** - 通用动画管理器

## 详细说明

### 1. StateMachine.cs

通用状态机基类，提供了状态管理的核心功能。

**特性：**
- 支持泛型状态和上下文类型
- 自动状态转换检查
- 状态进入/更新/退出回调
- 强制状态切换

**使用示例：**
```csharp
public class MyStateMachine : StateMachine<MyState, MyContext>
{
    public MyStateMachine() : base(MyState.Initial)
    {
        // 添加状态和转换
        AddState(MyState.Idle, OnEnterIdle, OnUpdateIdle, OnExitIdle);
        AddTransition(MyState.Idle, MyState.Moving, context => context.ShouldMove);
    }
    
    protected override MyContext GetContext()
    {
        return new MyContext { /* 上下文数据 */ };
    }
}
```

### 2. MinionAIHelper.cs

召唤物AI的通用工具类，提供了常用的AI功能。

**主要功能：**
- 位置计算和传送
- 目标搜索
- 召唤物重叠处理
- 移动辅助函数
- 动画辅助函数

**使用示例：**
```csharp
// 计算理想位置
Vector2 idlePosition = MinionAIHelper.CalculateIdlePosition(owner, projectile);

// 搜索目标
var targetResult = MinionAIHelper.SearchForTargets(owner, projectile);

// 应用重力
MinionAIHelper.ApplyGravity(projectile, 0.4f, 10f);
```

### 3. AnimationManager.cs

通用动画管理器，支持多种动画状态和配置。

**特性：**
- 支持多种动画状态
- 可配置的帧率和帧范围
- 旋转控制
- 循环和单次播放

**使用示例：**
```csharp
var animationManager = new BabySlimeAnimationManager();
animationManager.SetStateFromBabySlimeState(currentState);
animationManager.UpdateAnimation(projectile);
```

## 扩展指南

### 创建新的召唤物状态机

1. 定义状态枚举
2. 创建状态机上下文类
3. 在召唤物类中实现状态机作为内部类
4. 实现状态处理器

**示例（在召唤物类中）：**
```csharp
public class MyMinion : ModProjectile
{
    // 状态机作为内部类
    public class MyStateMachine
    {
        // 实现状态机逻辑
    }
    
    private MyStateMachine _stateMachine;
    
    public override void AI()
    {
        _stateMachine.Update(context);
    }
}
```

### 创建新的动画管理器

1. 继承AnimationManager类
2. 在构造函数中初始化动画状态
3. 实现状态切换逻辑

### 添加新的AI功能

1. 在MinionAIHelper中添加静态方法
2. 使用适当的参数和返回值
3. 添加详细的中文注释

## 最佳实践

1. **模块化设计**：将功能分离到不同的工具类中
2. **可复用性**：设计通用的接口和基类
3. **可扩展性**：使用继承和组合模式
4. **文档化**：为所有公共方法添加详细注释
5. **测试友好**：设计易于测试的接口
6. **职责分离**：将特定召唤物的逻辑放在召唤物类中，通用功能放在Utils中

## 依赖关系

- `StateMachine.cs` - 独立，无依赖
- `MinionAIHelper.cs` - 独立，无依赖
- `AnimationManager.cs` - 独立，无依赖

## 性能考虑

1. 状态机使用字典查找，时间复杂度O(1)
2. 动画管理器使用延迟初始化
3. AI助手使用静态方法，避免对象创建开销
4. 上下文对象可以复用以减少GC压力

## 设计原则

1. **通用性**：Utils中的类应该是通用的，可以被多个召唤物复用
2. **特定性**：特定召唤物的逻辑应该放在召唤物类中作为内部类
3. **封装性**：内部类可以访问外部类的私有成员，提供更好的封装
4. **可维护性**：代码结构清晰，易于理解和修改 