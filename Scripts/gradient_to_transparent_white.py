#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
gradient_to_transparent_white.py

将「黑白渐变」纹理转为「透明-白渐变」：
- 原图中黑色/深灰 → 完全透明
- 原图中白色/亮部 → 不透明白色
- 中间灰度 → 按亮度作为 alpha，颜色统一为白色

依赖: Pillow (pip install Pillow)

使用示例:
  # 默认：读取 Assets/Textures/Vertexes/SwordTail4.png，输出到同目录 _out.png
  python Scripts/gradient_to_transparent_white.py

  # 指定输入输出
  python Scripts/gradient_to_transparent_white.py -i Assets/Textures/Vertexes/SwordTail4.png -o Assets/Textures/Vertexes/SwordTail4_transparent.png

  # 可选：亮度阈值，低于此值的像素视为完全透明（0-255，默认 0）
  python Scripts/gradient_to_transparent_white.py -i xxx.png -o yyy.png --threshold 5
"""

import argparse
import os
import sys

try:
    from PIL import Image
except ImportError:
    print("请先安装 Pillow: pip install Pillow", file=sys.stderr)
    sys.exit(1)

WORKSPACE_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir))


def luminance(r: int, g: int, b: int) -> int:
    """标准亮度公式 (ITU-R BT.601)。"""
    return int(0.299 * r + 0.587 * g + 0.114 * b)


def convert_black_white_to_transparent_white(
    input_path: str,
    output_path: str,
    threshold: int = 0,
) -> None:
    """
    将黑白渐变图转为透明-白渐变图。
    - 每个像素的亮度作为新 alpha
    - 新颜色统一为白色 (255,255,255)
    - 亮度低于 threshold 的像素 alpha 强制为 0
    """
    img = Image.open(input_path).convert("RGBA")
    pixels = img.load()
    w, h = img.size

    for y in range(h):
        for x in range(w):
            r, g, b, a_old = pixels[x, y]
            lum = luminance(r, g, b)
            if lum <= threshold:
                new_alpha = 0
            else:
                new_alpha = lum
            # 透明-白：颜色为白，透明度由原亮度决定
            pixels[x, y] = (255, 255, 255, new_alpha)

    img.save(output_path, "PNG")
    print(f"已保存: {output_path}")


def main() -> None:
    default_input = os.path.join(
        WORKSPACE_ROOT,
        "Assets", "Textures", "Vertexes", "SwordTail4.png",
    )
    default_output = os.path.join(
        WORKSPACE_ROOT,
        "Assets", "Textures", "Vertexes", "SwordTail4_transparent.png",
    )

    parser = argparse.ArgumentParser(
        description="将黑白渐变纹理转为透明-白渐变（黑边变透明）",
    )
    parser.add_argument(
        "-i", "--input",
        default=default_input,
        help=f"输入 PNG 路径 (默认: {default_input})",
    )
    parser.add_argument(
        "-o", "--output",
        default=default_output,
        help=f"输出 PNG 路径 (默认: {default_output})",
    )
    parser.add_argument(
        "--threshold",
        type=int,
        default=0,
        metavar="0-255",
        help="亮度低于此值的像素强制完全透明 (默认 0)",
    )
    args = parser.parse_args()

    inp = os.path.normpath(os.path.join(WORKSPACE_ROOT, args.input) if not os.path.isabs(args.input) else args.input)
    out = os.path.normpath(os.path.join(WORKSPACE_ROOT, args.output) if not os.path.isabs(args.output) else args.output)

    if not os.path.isfile(inp):
        print(f"错误: 输入文件不存在: {inp}", file=sys.stderr)
        sys.exit(1)

    os.makedirs(os.path.dirname(out) or ".", exist_ok=True)
    convert_black_white_to_transparent_white(inp, out, threshold=args.threshold)


if __name__ == "__main__":
    main()
