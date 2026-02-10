#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
原版 Terraria 物品 ID 查询脚本
数据来源: https://terraria.wiki.gg/wiki/Item_IDs
支持通过 ID、Name、Internal name 三项中的任意一项查询完整词条。
"""

import re
import urllib.request
import urllib.error


WIKI_URL = "https://terraria.wiki.gg/wiki/Item_IDs"
# 表格行正则：匹配 | 数字 | [名称](链接) | `InternalName` |
ROW_PATTERN = re.compile(
    r"^\s*(\d+)\s*\|\s*\[([^\]]+)\]\([^)]+\)\s*\|\s*`([^`]+)`\s*\|?\s*$",
    re.MULTILINE,
)


def fetch_wiki_page() -> str:
    """从 Wiki 获取 Item IDs 页面内容（Markdown 格式）。"""
    req = urllib.request.Request(
        WIKI_URL,
        headers={"User-Agent": "TerrariaModScript/1.0 (Item ID Search)"},
    )
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            return resp.read().decode("utf-8", errors="replace")
    except urllib.error.URLError as e:
        raise SystemExit(f"无法获取 Wiki 页面: {e}")


def parse_item_table(html: str) -> list[dict]:
    """
    从页面文本中解析物品表格。
    页面可能返回 HTML，这里尝试从正文中匹配 Markdown 风格的表格行。
    """
    items = []
    # 兼容 Markdown 风格（部分爬虫/API 会转成这种格式）
    for m in ROW_PATTERN.finditer(html):
        item_id, name, internal_name = m.groups()
        items.append({
            "id": int(item_id),
            "name": name.strip(),
            "internal_name": internal_name.strip(),
        })
    if items:
        return items
    # 若上面没匹配到，从 Wiki 的 HTML 表格解析
    # 格式: <tr><td>1</td><td><a ...>Name</a></td><td><code>InternalName</code></td></tr>
    html_row = re.compile(
        r"<tr[^>]*>\s*<td[^>]*>\s*(\d+)\s*</td>\s*"
        r"<td[^>]*>\s*<a[^>]*>([^<]+)</a>\s*</td>\s*"
        r"<td[^>]*>\s*<code>([^<]*)</code>\s*</td>\s*</tr>",
        re.IGNORECASE,
    )
    for m in html_row.finditer(html):
        item_id, name, internal_name = m.groups()
        items.append({
            "id": int(item_id),
            "name": name.strip(),
            "internal_name": internal_name.strip(),
        })
    return items


def search_items(items: list[dict], search_type: str, query: str) -> list[dict]:
    """根据查询类型和关键词搜索物品。"""
    query = query.strip()
    if not query:
        return []
    results = []
    if search_type == "id":
        try:
            q = int(query)
            for it in items:
                if it["id"] == q:
                    results.append(it)
                    break
        except ValueError:
            pass
    elif search_type == "name":
        q_lower = query.lower()
        for it in items:
            if q_lower in it["name"].lower():
                results.append(it)
    elif search_type == "internal_name":
        q_lower = query.lower()
        for it in items:
            if q_lower in it["internal_name"].lower():
                results.append(it)
    return results


def print_entry(item: dict) -> None:
    """打印一条完整词条。"""
    print(f"  ID:            {item['id']}")
    print(f"  Name:         {item['name']}")
    print(f"  Internal name: {item['internal_name']}")
    print()


def main() -> None:
    print("原版 Terraria 物品 ID 查询（数据来源: Item IDs - Terraria Wiki）")
    print("正在加载数据...")
    raw = fetch_wiki_page()
    items = parse_item_table(raw)
    if not items:
        print("未能从页面解析到任何物品数据，请检查网络或页面结构是否变化。")
        return
    print(f"已加载 {len(items)} 条物品数据。\n")

    while True:
        print("请选择输入的是哪一项：")
        print("  1 - ID（数字）")
        print("  2 - Name（显示名称）")
        print("  3 - Internal name（内部名称）")
        print("  q - 退出")
        choice = input("请输入 1 / 2 / 3 / q: ").strip().lower()
        if choice == "q":
            print("再见。")
            break
        if choice not in ("1", "2", "3"):
            print("无效输入，请重新选择。\n")
            continue

        if choice == "1":
            search_type = "id"
            prompt = "请输入物品 ID（数字）: "
        elif choice == "2":
            search_type = "name"
            prompt = "请输入物品 Name（支持模糊）: "
        else:
            search_type = "internal_name"
            prompt = "请输入物品 Internal name（支持模糊）: "

        query = input(prompt).strip()
        if not query:
            print("未输入内容，跳过。\n")
            continue

        results = search_items(items, search_type, query)
        if not results:
            print("未找到匹配的物品。\n")
            continue
        print(f"共找到 {len(results)} 条结果：\n")
        for item in results:
            print_entry(item)


if __name__ == "__main__":
    main()
