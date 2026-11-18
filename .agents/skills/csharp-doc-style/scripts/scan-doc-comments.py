#!/usr/bin/env python3
"""scan-doc-comments.py —— 注释风格审计的补充扫描（SKILL.md 第 3、4 节口径）。

`scripts/scan-remarks.cs` 只查「单段正文超行」，本脚本补它查不到的四类：
  1. `<summary>` 写成两句及以上（第 3 节：summary 始终一句话总述）或其正文超过 2 行；
  2. 中文与内联标签粘连（第 4 节：`是<see …/>` 应为 `是 <see …/>`）；
  3. 注释里的日期/作者落款（含 `@user` 署名）、`<br/><br/>` 造段、`1. 2. 3.` 散文编号（第 3 节）；
  4. 中文语料里的半角标点（第 4 节：中文正文用全角）。

用法：
    scripts/scan-doc-comments.py [目录 ...]        # 缺省扫当前目录
    scripts/scan-doc-comments.py --max-summary-lines 2 src/Foo

退出码：0 = 无命中；1 = 有命中。全部命中项按类别打印，行号定位到注释块起始行。
GB18030 等非 UTF-8 源码自动回退解码，不会因编码报错中断整次扫描。
"""

import argparse
import pathlib
import re
import sys

CJK = r"\u4e00-\u9fa5"
GLUE = re.compile(rf"[{CJK}](<see |<c>|<paramref )|(</see>|</c>|/>)[{CJK}]")
DATED = re.compile(r"lukan,|\b20\d\d\s*年\s*\d+\s*月|\b20\d\d-\d{2}-\d{2}\b|@[A-Za-z_]\w*")
DOUBLE_BR = re.compile(r"<br\s*/?>\s*<br\s*/?>")
PROSE_LIST = re.compile(r"^\s*///\s*\d+\.")
HALF_PUNCT = re.compile(rf"[{CJK}][,.;:?!](?!\w)")


def decode(path: pathlib.Path) -> str:
    raw = path.read_bytes()
    for enc in ("utf-8-sig", "utf-8", "gb18030"):
        try:
            return raw.decode(enc)
        except UnicodeDecodeError:
            continue
    return raw.decode("utf-8", errors="replace")


def scan(root: pathlib.Path, max_lines: int):
    hits = {"summary 多句/超行": [], "中文与标签粘连": [], "落款/双 br/散文编号": [], "中文里的半角标点": []}
    for path in sorted(root.rglob("*.cs")):
        if "/obj/" in str(path) or "/bin/" in str(path):
            continue
        in_summary, body, start = False, [], 0
        for lineno, raw_line in enumerate(decode(path).splitlines(), 1):
            stripped = raw_line.strip()
            if GLUE.search(raw_line):
                hits["中文与标签粘连"].append(f"{path}:{lineno}")
            if DATED.search(raw_line) or DOUBLE_BR.search(raw_line) or PROSE_LIST.search(raw_line):
                hits["落款/双 br/散文编号"].append(f"{path}:{lineno}")
            if HALF_PUNCT.search(raw_line):
                hits["中文里的半角标点"].append(f"{path}:{lineno}")
            if not stripped.startswith("//"):
                in_summary = False
                continue
            text = stripped.lstrip("/").strip()
            if "<summary>" in text:
                in_summary, start = True, lineno
                body = [text.replace("<summary>", "").strip()]
                continue
            if "</summary>" in text and in_summary:
                body.append(text.replace("</summary>", "").strip())
                prose = "".join(body)
                lines = len([b for b in body if b])
                if prose.count("。") > 1 or lines > max_lines:
                    hits["summary 多句/超行"].append(f"{path}:{start} 句数={prose.count('。')} 行={lines}")
                in_summary = False
                continue
            if in_summary:
                body.append(text)
    return hits


def main() -> int:
    parser = argparse.ArgumentParser(description="注释风格审计补充扫描")
    parser.add_argument("roots", nargs="*", default=["."])
    parser.add_argument("--max-summary-lines", type=int, default=2)
    args = parser.parse_args()
    total = 0
    for root in args.roots:
        hits = scan(pathlib.Path(root), args.max_summary_lines)
        print(f"--- {root}")
        for kind, items in hits.items():
            print(f"  {kind}: {len(items)}")
            for item in items:
                print(f"    {item}")
            total += len(items)
    print(f"合计 {total} 处。")
    return 1 if total else 0


if __name__ == "__main__":
    sys.exit(main())
