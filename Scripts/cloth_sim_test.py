import pygame
import numpy as np
import yaml
import math

class ClothNode:
    def __init__(self, position, is_fixed, config):
        self.position = np.array(position, dtype=float)
        self.old_position = np.array(position, dtype=float)
        self.mass = config['mass']
        self.is_fixed = is_fixed
        self.dt = config['dt']
        self.gravity = config['gravity']
        self.damping = config['damping']
        
        # 物理属性
        self.velocity = np.array([0.0, 0.0])
        self.acceleration = np.array([0.0, 0.0])
        self.force = np.array([0.0, 0.0])
        
        # 约束连接
        self.neighbors = []
        self.constraints = []  # 存储约束信息 (neighbor_node, rest_length)

    def add_neighbor(self, neighbor, rest_length):
        """添加邻居节点和约束"""
        self.neighbors.append(neighbor)
        self.constraints.append((neighbor, rest_length))

    def update(self):
        """更新节点物理状态"""
        if self.is_fixed:
            return
            
        # 重力
        # self.force = np.array([0.0, gravity * self.mass])
        self.force += np.array([0.0, self.gravity * self.mass])

        # 阻尼力
        self.force += -self.velocity * self.damping
        
        # 计算加速度
        self.acceleration = self.force / self.mass
        
        # Verlet积分
        new_position = 2 * self.position - self.old_position + self.acceleration * self.dt * self.dt
        
        # 阻尼
        # new_position += (self.position - self.old_position) * (1 - damping)
        
        self.old_position = self.position.copy()
        self.position = new_position
        
        # 更新速度
        self.velocity = (self.position - self.old_position) / self.dt


class ClothSim:
    def __init__(self, config):
        self.config = config
        self.dist_between_nodes = config['dist_between_nodes']
        self.cloth_height = config['cloth_height']
        self.cloth_width = config['cloth_width']
        self.gravity = config['gravity']
        self.mass = config['mass']
        self.spring_k = config['spring_k']
        self.damping = config['damping']
        self.dt = config['dt']

        self.start_x = config['window_width'] / 2 - self.cloth_width * self.dist_between_nodes / 2
        self.start_y = config['window_height'] / 2 - self.cloth_height * self.dist_between_nodes / 2
        
        # 创建布料网格
        self.nodes = []
        self.create_cloth_grid()
        
    def create_cloth_grid(self):
        """创建布料网格，最左侧节点固定"""
        rows = self.cloth_height
        cols = self.cloth_width
        
        # 创建节点
        for i in range(rows):
            row_nodes = []
            for j in range(cols):
                x = j * self.dist_between_nodes + self.start_x  # 起始x位置
                y = i * self.dist_between_nodes + self.start_y   # 起始y位置
                
                # 最左侧的节点固定
                is_fixed = (j == 0)
                
                node = ClothNode([x, y], is_fixed, self.config)
                row_nodes.append(node)
            self.nodes.append(row_nodes)
        
        # 建立约束连接
        for i in range(rows):
            for j in range(cols):
                current_node = self.nodes[i][j]
                
                # 水平连接 (右邻居)
                if j < cols - 1:
                    right_node = self.nodes[i][j + 1]
                    current_node.add_neighbor(right_node, self.dist_between_nodes)
                
                # 垂直连接 (下邻居)
                if i < rows - 1:
                    bottom_node = self.nodes[i + 1][j]
                    current_node.add_neighbor(bottom_node, self.dist_between_nodes)
                
                # 对角线连接 (可选，增加稳定性)
                # if i < rows - 1 and j < cols - 1:
                #     diagonal_node = self.nodes[i + 1][j + 1]
                #     diagonal_length = self.dist_between_nodes * math.sqrt(2)
                #     current_node.add_neighbor(diagonal_node, diagonal_length)
    
    def apply_spring_constraints(self):
        """应用弹簧约束"""
        for row in self.nodes:
            for node in row:
                for neighbor, rest_length in node.constraints:
                    # 计算当前距离
                    diff = node.position - neighbor.position
                    current_length = np.linalg.norm(diff)
                    
                    if current_length > 0:
                        # 弹簧力 F = k * (current_length - rest_length)
                        force_magnitude = self.spring_k * (current_length - rest_length)
                        force_direction = diff / current_length
                        force = force_magnitude * force_direction
                        
                        # 应用力
                        if not node.is_fixed:
                            node.force += -force
                        if not neighbor.is_fixed:
                            neighbor.force += force

    def apply_wind_force(self):
        """应用风力"""
        wind_force = 0.1
        wind_dir = -0.2
        seed = np.random.rand()
        for row in self.nodes:
            for node in row:
                random_wind_force = wind_force + (seed-0.5)*2 * 0.1
                random_wind_dir = wind_dir + (seed-0.5)*2 * 0.4
                random_wind_dir = np.array([np.cos(random_wind_dir), np.sin(random_wind_dir)])
                node.force += random_wind_force * random_wind_dir
    
    def update(self):
        """更新仿真"""
        # 重置所有节点的力
        for row in self.nodes:
            for node in row:
                node.force = np.array([0.0, 0.0])
        
        # 应用弹簧约束
        self.apply_spring_constraints()
        # self.apply_wind_force()
        # 更新所有节点
        for row in self.nodes:
            for node in row:
                node.update()
    
    def get_all_nodes(self):
        """获取所有节点的扁平列表"""
        all_nodes = []
        for row in self.nodes:
            all_nodes.extend(row)
        return all_nodes


class ClothRenderer:
    def __init__(self, screen, config):
        self.screen = screen
        self.config = config
        
        # 颜色设置
        self.bg_color = tuple(config['background_color'])
        self.node_color = tuple(config['node_color'])
        self.constraint_color = tuple(config['constraint_color'])
        self.fixed_node_color = tuple(config['fixed_node_color'])
        
        # 尺寸设置
        self.node_radius = config['node_radius']
        self.line_width = config['line_width']
    
    def render(self, cloth_sim):
        """渲染布料"""
        # 清空屏幕
        self.screen.fill(self.bg_color)
        
        # 绘制约束线
        for row in cloth_sim.nodes:
            for node in row:
                for neighbor, _ in node.constraints:
                    start_pos = (int(node.position[0]), int(node.position[1]))
                    end_pos = (int(neighbor.position[0]), int(neighbor.position[1]))
                    pygame.draw.line(self.screen, self.constraint_color, start_pos, end_pos, self.line_width)
        
        # 绘制节点
        for row in cloth_sim.nodes:
            for node in row:
                pos = (int(node.position[0]), int(node.position[1]))
                color = self.fixed_node_color if node.is_fixed else self.node_color
                pygame.draw.circle(self.screen, color, pos, self.node_radius)
        
        # 更新显示
        pygame.display.flip()


def load_config(config_path):
    """加载YAML配置文件"""
    with open(config_path, 'r', encoding='utf-8') as file:
        return yaml.safe_load(file)


def main():
    """主程序"""
    # 加载配置
    config = load_config('cloth_config.yaml')
    
    # 初始化pygame
    pygame.init()
    screen = pygame.display.set_mode((config['window_width'], config['window_height']))
    pygame.display.set_caption("布料仿真 - Cloth Simulation")
    clock = pygame.time.Clock()
    
    # 创建仿真和渲染器
    cloth_sim = ClothSim(config)
    renderer = ClothRenderer(screen, config)
    
    # 主循环
    running = True
    while running:
        # 处理事件
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_r:
                    # 按R键重置仿真
                    cloth_sim = ClothSim(config)
                elif event.key == pygame.K_ESCAPE:
                    running = False
        
        # 更新仿真
        cloth_sim.update()
        
        # 渲染
        renderer.render(cloth_sim)
        
        # 控制帧率
        clock.tick(config['fps'])
    
    pygame.quit()


if __name__ == "__main__":
    main()