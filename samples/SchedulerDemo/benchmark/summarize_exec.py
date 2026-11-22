#!/usr/bin/env python3
"""执行侧统计：状态分布、执行耗时、受理到完成的时延（含队列排队）。

用法：
    python3 summarize_exec.py 5000
输入：/tmp/stress-exec-<N>.tsv，列：DispatchAt / FinishAt / ElapsedMs / State / Attempt
"""
import sys
from datetime import datetime

N = sys.argv[1]
PATH = f"/tmp/stress-exec-{N}.tsv"


def pct(vals, q):
    if not vals:
        return None
    vals = sorted(vals)
    idx = min(len(vals) - 1, int(len(vals) * q))
    return vals[idx]


def ts(s):
    s = s.strip()
    if not s:
        return None
    try:
        return datetime.fromisoformat(s.replace(" ", "T"))
    except ValueError:
        return None


states = {}
elapsed = []
turnaround = []
attempts = {}

for line in open(PATH, encoding="utf-8"):
    parts = line.rstrip("\n").split("\t")
    if len(parts) < 4:
        continue
    d, f, e, st = parts[0], parts[1], parts[2], parts[3]
    att = int(parts[4]) if len(parts) > 4 and parts[4].strip().isdigit() else 1
    states[st] = states.get(st, 0) + 1
    attempts[att] = attempts.get(att, 0) + 1

    if e and e.strip().isdigit():
        elapsed.append(int(e))

    dt, ft = ts(d), ts(f)
    if dt and ft:
        turnaround.append(int((ft - dt).total_seconds() * 1000))

total = sum(states.values())
print(f"== 执行侧（{N} 任务，样本 {total} 行） ==")
print("  State: " + "  ".join(f"{k}={v}" for k, v in sorted(states.items())))

if elapsed:
    print(f"  执行耗时 ElapsedMs: p50={pct(elapsed,.5)}  p95={pct(elapsed,.95)}  "
          f"p99={pct(elapsed,.99)}  max={max(elapsed)}")

if turnaround:
    print(f"  受理→完成(含排队): p50={pct(turnaround,.5)}ms  p95={pct(turnaround,.95)}ms  "
          f"p99={pct(turnaround,.99)}ms  max={max(turnaround)}ms")

if attempts and max(attempts) > 1:
    print("  重试分布 Attempt: " + "  ".join(f"{k}={v}" for k, v in sorted(attempts.items())))

succ = states.get("Succeeded", 0)
if total:
    print(f"  成功率: {succ}/{total} = {succ * 100.0 / total:.1f}" + "%")
