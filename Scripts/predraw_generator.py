#!/usr/bin/env python3
"""
predraw_generator.py

功能:
- scan: 扫描 Content/Projectiles/Summon 下的 .cs 文件, 查找是否实现了 PreDraw 以便参考风格
- ui: 打开一个简易可视化工具(基于 pygame), 用于在贴图上标注 rect / origin / 本地偏移, 并保存为 YAML
- gen: 根据 YAML 配置生成 C# PreDraw 代码片段, 使用 MinionAIHelper.DrawPart 风格输出

依赖: pygame, PyYAML (已在 Scripts/requirements.txt)

使用示例:
  1) 扫描已有 PreDraw
     python Scripts/predraw_generator.py scan

  2) 可视化标注(加载贴图)
     python Scripts/predraw_generator.py ui \
       --image Assets/Textures/Projectiles/GatlingSentryGun.png \
       --asset_path SummonerExpansionMod/Assets/Textures/Projectiles/GatlingSentryGun \
       --out_yaml Scripts/predraw_samples/gatling_gun.yaml

  3) 由 YAML 生成 C# 代码片段
     python Scripts/predraw_generator.py gen \
       --yaml Scripts/predraw_samples/gatling_gun.yaml \
       --out_cs Scripts/predraw_samples/gatling_gun_PreDraw.cs.txt

注意:
- 生成的 C# 代码仅为片段, 需要手动拷贝到相应 Projectile 的 PreDraw 中, 并按需调整变量名/常量名
- 贴图 Asset 路径以 ModContent.Request<Texture2D>(ASSET).Value 方式获取
"""

import argparse
import os
import re
import sys
import math
import json
from typing import List, Dict, Any, Tuple

try:
    import pygame  # type: ignore
except Exception:
    pygame = None

try:
    import yaml  # type: ignore
except Exception:
    yaml = None

WORKSPACE_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir))
DEFAULT_CS_DIR = os.path.join(WORKSPACE_ROOT, "Content", "Projectiles", "Summon")


# -----------------------------
# Utilities
# -----------------------------

def find_cs_files(root: str) -> List[str]:
    cs_files: List[str] = []
    for dirpath, _, filenames in os.walk(root):
        for fn in filenames:
            if fn.lower().endswith(".cs"):
                cs_files.append(os.path.join(dirpath, fn))
    return cs_files


def file_contains_predraw(path: str) -> bool:
    try:
        with open(path, "r", encoding="utf-8") as f:
            text = f.read()
        return re.search(r"public\s+override\s+bool\s+PreDraw\s*\(\s*ref\s+Color\s+lightColor\s*\)", text) is not None
    except Exception:
        return False


def read_text(path: str) -> str:
    with open(path, "r", encoding="utf-8") as f:
        return f.read()


def write_text(path: str, content: str) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


# -----------------------------
# Subcommand: scan
# -----------------------------

def cmd_scan(args: argparse.Namespace) -> None:
    target = args.dir or DEFAULT_CS_DIR
    if not os.path.isdir(target):
        print(f"[scan] 目录不存在: {target}")
        sys.exit(1)
    cs_files = find_cs_files(target)
    with_predraw = []
    without_predraw = []
    for path in cs_files:
        if file_contains_predraw(path):
            with_predraw.append(path)
        else:
            without_predraw.append(path)

    print("[scan] 含 PreDraw 的文件:")
    for p in with_predraw:
        print(" - ", os.path.relpath(p, WORKSPACE_ROOT))

    print("\n[scan] 不含 PreDraw 的文件:")
    for p in without_predraw:
        print(" - ", os.path.relpath(p, WORKSPACE_ROOT))


# -----------------------------
# Subcommand: ui  (pygame 简易标注器)
# -----------------------------

class Part:
    def __init__(self, name: str, asset_path: str):
        self.name = name  # 仅用于标识
        self.asset_path = asset_path  # ModContent 的资源标识(不带扩展名)
        self.rect = pygame.Rect(0, 0, 16, 16) if pygame else None
        self.origin = (0, 0)  # 在贴图上的 origin 点 (像素)
        self.local_offset = (0.0, 0.0)  # 映射到 ConvertToWorldPos 的本地偏移(像素)
        self.rotation_expr = "Projectile.rotation"  # C# 表达式

    def to_dict(self) -> Dict[str, Any]:
        r = self.rect
        return {
            "name": self.name,
            "asset_path": self.asset_path,
            "rect": [int(r.x), int(r.y), int(r.w), int(r.h)] if r else [0, 0, 0, 0],
            "origin": [int(self.origin[0]), int(self.origin[1])],
            "local_offset": [float(self.local_offset[0]), float(self.local_offset[1])],
            "rotation_expr": self.rotation_expr,
        }


def cmd_ui(args: argparse.Namespace) -> None:
    if pygame is None:
        print("[ui] 需要 pygame, 请先 pip install pygame")
        sys.exit(1)

    image_path = args.image
    asset_path = args.asset_path
    out_yaml = args.out_yaml
    if not image_path or not os.path.isfile(image_path):
        print("[ui] 请使用 --image 指定存在的贴图路径")
        sys.exit(1)
    if not asset_path:
        print("[ui] 请使用 --asset_path 指定 ModContent 资源路径(不含扩展名), 例如: SummonerExpansionMod/Assets/Textures/Projectiles/GatlingSentryGun")
        sys.exit(1)

    pygame.init()
    pygame.display.set_caption("PreDraw 可视化标注器 - 左键拖动origin, 右键拖动rect, WASD微调offset, 数字键切换部件")

    img = pygame.image.load(image_path)
    img_w, img_h = img.get_width(), img.get_height()
    scale = max(1, int(800 / max(img_w, img_h)))
    disp_w, disp_h = img_w * scale + 300, img_h * scale + 40
    screen = pygame.display.set_mode((disp_w, disp_h))
    clock = pygame.time.Clock()

    parts: List[Part] = [Part("part_1", asset_path)]
    active_idx = 0
    dragging_origin = False
    dragging_rect = False
    drag_start = (0, 0)

    def draw_ui():
        screen.fill((30, 30, 30))
        # 贴图
        screen.blit(pygame.transform.scale(img, (img_w * scale, img_h * scale)), (20, 20))

        # 网格
        for x in range(0, img_w * scale + 1, 20):
            pygame.draw.line(screen, (50, 50, 50), (20 + x, 20), (20 + x, 20 + img_h * scale))
        for y in range(0, img_h * scale + 1, 20):
            pygame.draw.line(screen, (50, 50, 50), (20, 20 + y), (20 + img_w * scale, 20 + y))

        # 部件绘制
        font = pygame.font.SysFont(None, 20)
        for i, p in enumerate(parts):
            color = (0, 200, 255) if i == active_idx else (180, 180, 180)
            if p.rect:
                rx, ry, rw, rh = p.rect
                pygame.draw.rect(screen, color, pygame.Rect(20 + rx * scale, 20 + ry * scale, rw * scale, rh * scale), 2)
            # origin
            ox, oy = p.origin
            pygame.draw.circle(screen, (255, 120, 0), (20 + int(ox * scale), 20 + int(oy * scale)), 4)
            # info
            txt = font.render(f"[{i+1}] {p.name} rect={list(p.rect)} origin={p.origin} offset={p.local_offset}", True, (220, 220, 220))
            screen.blit(txt, (img_w * scale + 40, 30 + i * 24))

        hint_lines = [
            "鼠标: 左键拖动origin, 右键拖动rect角点",
            "键盘: WASD 微调 local_offset, Q/E 微调旋转(仅表达式标记)",
            "     N 新建部件, Tab 切换, Del 删除当前, S 保存YAML",
        ]
        for i, t in enumerate(hint_lines):
            txt = font.render(t, True, (220, 220, 220))
            screen.blit(txt, (20, img_h * scale + 24 + i * 18))

        pygame.display.flip()

    while True:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                return
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    pygame.quit()
                    return
                elif event.key == pygame.K_n:
                    parts.append(Part(f"part_{len(parts)+1}", asset_path))
                    active_idx = len(parts) - 1
                elif event.key == pygame.K_TAB:
                    active_idx = (active_idx + 1) % len(parts)
                elif event.key == pygame.K_DELETE:
                    if parts:
                        parts.pop(active_idx)
                        active_idx = max(0, active_idx - 1)
                        if not parts:
                            parts.append(Part("part_1", asset_path))
                elif event.key == pygame.K_s:
                    if yaml is None:
                        print("[ui] 需要 PyYAML 才能保存, 请安装后再试")
                    else:
                        data = {
                            "image_path": image_path,
                            "asset_path": asset_path,
                            "parts": [p.to_dict() for p in parts],
                        }
                        write_text(out_yaml, yaml.safe_dump(data, allow_unicode=True, sort_keys=False))
                        print(f"[ui] 已保存: {out_yaml}")
                elif event.key in (pygame.K_w, pygame.K_a, pygame.K_s, pygame.K_d):
                    if parts:
                        p = parts[active_idx]
                        dx = (-1 if event.key == pygame.K_a else 1 if event.key == pygame.K_d else 0)
                        dy = (-1 if event.key == pygame.K_w else 1 if event.key == pygame.K_s else 0)
                        p.local_offset = (p.local_offset[0] + dx, p.local_offset[1] + dy)
                elif event.key in (pygame.K_1, pygame.K_2, pygame.K_3, pygame.K_4, pygame.K_5, pygame.K_6, pygame.K_7, pygame.K_8, pygame.K_9):
                    idx = event.key - pygame.K_1
                    if 0 <= idx < len(parts):
                        active_idx = idx

            elif event.type == pygame.MOUSEBUTTONDOWN:
                mx, my = event.pos
                # 相对贴图坐标
                lx = (mx - 20) / scale
                ly = (my - 20) / scale
                if event.button == 1:  # left -> origin
                    dragging_origin = True
                    drag_start = (lx, ly)
                elif event.button == 3:  # right -> rect
                    dragging_rect = True
                    drag_start = (lx, ly)

            elif event.type == pygame.MOUSEBUTTONUP:
                if event.button == 1:
                    dragging_origin = False
                elif event.button == 3:
                    dragging_rect = False

            elif event.type == pygame.MOUSEMOTION:
                if parts:
                    p = parts[active_idx]
                    mx, my = event.pos
                    lx = max(0, min(img_w, (mx - 20) / scale))
                    ly = max(0, min(img_h, (my - 20) / scale))
                    if dragging_origin:
                        p.origin = (int(lx), int(ly))
                    if dragging_rect:
                        x0, y0 = drag_start
                        x1, y1 = lx, ly
                        rx = int(min(x0, x1))
                        ry = int(min(y0, y1))
                        rw = int(abs(x1 - x0))
                        rh = int(abs(y1 - y0))
                        rw = max(1, min(rw, img_w - rx))
                        rh = max(1, min(rh, img_h - ry))
                        p.rect = pygame.Rect(rx, ry, rw, rh)

        draw_ui()
        clock.tick(60)


# -----------------------------
# Subcommand: gen  (由 YAML 生成 C# PreDraw 片段)
# -----------------------------

CODE_HEADER = (
    "// 生成的 PreDraw 代码片段\n"
    "// 需要: using Microsoft.Xna.Framework; using Microsoft.Xna.Framework.Graphics;\n"
    "// 建议与 MinionAIHelper 一起使用\n"
)


def generate_csharp_from_yaml(cfg: Dict[str, Any]) -> str:
    parts: List[Dict[str, Any]] = cfg.get("parts", [])
    # 纹理去重并分配变量名
    asset_to_var: Dict[str, str] = {}
    texture_lines: List[str] = []
    var_index = 1
    for p in parts:
        asset = p.get("asset_path", "").strip()
        if not asset:
            continue
        if asset not in asset_to_var:
            var_name = f"Tex{var_index}"
            asset_to_var[asset] = var_name
            texture_lines.append(
                f"Texture2D {var_name} = ModContent.Request<Texture2D>(\"{asset}\").Value;"
            )
            var_index += 1

    draw_lines: List[str] = []
    draw_lines.append("// 按顺序绘制各部件")
    for p in parts:
        asset = p.get("asset_path", "")
        var_name = asset_to_var.get(asset, "Tex")
        rect = p.get("rect", [0, 0, 0, 0])
        origin = p.get("origin", [0, 0])
        local_offset = p.get("local_offset", [0.0, 0.0])
        rotation_expr = p.get("rotation_expr", "Projectile.rotation")

        rect_cs = f"new Rectangle({int(rect[0])}, {int(rect[1])}, {int(rect[2])}, {int(rect[3])})"
        origin_cs = f"new Vector2({int(origin[0])}, {int(origin[1])})"
        world_pos = f"MinionAIHelper.ConvertToWorldPos(Projectile, new Vector2({float(local_offset[0])}f, {float(local_offset[1])}f))"

        draw_lines.append(
            "MinionAIHelper.DrawPart(\n"
            f"    Projectile,\n"
            f"    {var_name},\n"
            f"    {world_pos},\n"
            f"    {rect_cs},\n"
            f"    lightColor,\n"
            f"    {rotation_expr},\n"
            f"    {origin_cs}\n"
            ");"
        )

    snippet = []
    snippet.append(CODE_HEADER)
    snippet.append("// 纹理请求")
    snippet.extend(texture_lines)
    snippet.append("")
    snippet.extend(draw_lines)
    snippet.append("")
    snippet.append("return false; // 阻止默认绘制")
    return "\n".join(snippet)


def cmd_gen(args: argparse.Namespace) -> None:
    if yaml is None:
        print("[gen] 需要 PyYAML, 请先安装: pip install pyyaml")
        sys.exit(1)
    yaml_path = args.yaml
    out_cs = args.out_cs
    if not yaml_path or not os.path.isfile(yaml_path):
        print("[gen] 请使用 --yaml 指定存在的 YAML 配置")
        sys.exit(1)
    with open(yaml_path, "r", encoding="utf-8") as f:
        cfg = yaml.safe_load(f)
    code = generate_csharp_from_yaml(cfg)
    write_text(out_cs, code)
    print(f"[gen] 已生成 C# 片段: {out_cs}")


# -----------------------------
# Entrypoint
# -----------------------------

def main():
    parser = argparse.ArgumentParser(description="PreDraw 可视化与代码生成器")
    sub = parser.add_subparsers(dest="cmd")

    p_scan = sub.add_parser("scan", help="扫描含 PreDraw 的 C# 文件")
    p_scan.add_argument("--dir", help="扫描目录(默认 Content/Projectiles/Summon)")

    p_ui = sub.add_parser("ui", help="基于 pygame 的贴图标注器")
    p_ui.add_argument("--image", required=True, help="要标注的贴图路径(本地 png)")
    p_ui.add_argument("--asset_path", required=True, help="ModContent 资源路径(不含扩展名)")
    p_ui.add_argument("--out_yaml", required=True, help="输出 YAML 文件路径")

    p_gen = sub.add_parser("gen", help="由 YAML 生成 C# PreDraw 代码片段")
    p_gen.add_argument("--yaml", required=True, help="YAML 配置文件")
    p_gen.add_argument("--out_cs", required=True, help="输出 .cs.txt 片段文件")

    args = parser.parse_args()

    if args.cmd == "scan":
        cmd_scan(args)
    elif args.cmd == "ui":
        cmd_ui(args)
    elif args.cmd == "gen":
        cmd_gen(args)
    else:
        parser.print_help()


if __name__ == "__main__":
    main()



