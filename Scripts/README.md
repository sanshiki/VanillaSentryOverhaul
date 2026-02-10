# 布料仿真程序

一个基于Python和Pygame的简易布料仿真程序，使用弹簧约束模型和Verlet积分法进行物理计算。

## 功能特点

- **弹簧约束模型**: 使用F=kx公式建模节点间的约束关系
- **重力仿真**: 考虑重力对布料的影响
- **固定边界**: 最左侧节点固定，模拟悬挂的布料
- **YAML配置**: 通过配置文件调整仿真参数
- **实时可视化**: 使用Pygame进行实时渲染

## 安装依赖

```bash
pip install -r requirements.txt
```

## 运行程序

```bash
python cloth_sim_test.py
```

## 控制说明

- **ESC键**: 退出程序
- **R键**: 重置仿真

## 配置文件

编辑 `cloth_config.yaml` 文件来调整仿真参数：

### 布料网格参数
- `cloth_height`: 布料高度（行数）
- `cloth_width`: 布料宽度（列数）
- `dist_between_nodes`: 节点间距离

### 物理参数
- `gravity`: 重力加速度
- `spring_k`: 弹簧常数k
- `damping`: 阻尼系数（0-1之间）
- `dt`: 时间步长

### 显示参数
- `window_width/height`: 窗口尺寸
- `fps`: 帧率
- 颜色设置和节点大小等

## 物理模型

### 弹簧约束
每个节点通过弹簧约束与相邻节点连接，弹簧力计算公式：
```
F = k * (current_length - rest_length)
```

### 数值积分
使用Verlet积分法进行位置更新：
```
new_position = 2 * position - old_position + acceleration * dt²
```

### 阻尼
添加阻尼来模拟能量损失，使仿真更稳定。

## 技术实现

- **ClothNode**: 表示布料节点，包含位置、速度、质量等物理属性
- **ClothSim**: 管理整个布料仿真，处理约束计算和物理更新
- **ClothRenderer**: 负责可视化渲染
- **YAML配置**: 灵活的参数配置系统












