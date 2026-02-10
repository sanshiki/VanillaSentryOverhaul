import numpy as np
from scipy.optimize import minimize

# 定义目标函数
def f(x):
    x1, x2 = x
    return 2*x1**2 + 2*x2**2 - 2*x1*x2 - 4*x1 - 6*x2

# 定义约束条件
cons = [
    {'type': 'ineq', 'fun': lambda x: 2 - (x[0] + x[1])},     # x1 + x2 <= 2
    {'type': 'ineq', 'fun': lambda x: 5 - (x[0] + 5*x[1])},   # x1 + 5x2 <= 5
    {'type': 'ineq', 'fun': lambda x: x[0]},                  # x1 >= 0
    {'type': 'ineq', 'fun': lambda x: x[1]},                  # x2 >= 0
]

# 初始点
x0 = np.array([0.5, 0.5])

# 求解
res = minimize(f, x0, constraints=cons)

# 输出结果
print("最优解:")
print("x1 =", res.x[0])
print("x2 =", res.x[1])
print("最小值 f(x) =", res.fun)
