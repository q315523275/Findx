#!/usr/bin/env python3
"""汇总压测结果：读取 /tmp/stress-rows-N.tsv（ExecutionKey / Period / State / DispatchAt）。

延迟口径：DispatchAt - Period。Period 为 cron 触发的理论时刻，DispatchAt 为调度端领取并落库的时刻，
两者之差即「该周期晚投递了多久」。R: 前缀表示重试锚点，单独处理。
"""
import collections
import datetime
import sys


def naive(text):
    text = text.strip()
    if text.startswith("R:"):
        text = text[2:]
    if not text:
        return None
    try:
        d = datetime.datetime.fromisoformat(text)
    except ValueError:
        return None
    return d.replace(tzinfo=None) if d.tzinfo else d


def quantile(values, p):
    if not values:
        return None
    values = sorted(values)
    return round(values[min(len(values) - 1, int(len(values) * p))], 3)


n = int(sys.argv[1])
rows = []
for line in open(f"/tmp/stress-rows-{n}.tsv", encoding="utf-8"):
    parts = line.rstrip("\n").split("\t")
    if len(parts) < 4:
        continue
    rows.append((parts[0], naive(parts[1]), parts[2], naive(parts[3])))

states = collections.Counter(r[2] for r in rows)
lags = [(r[3] - r[1]).total_seconds() for r in rows if r[1] and r[3]]
waves = collections.Counter(r[1] for r in rows if r[1])

print(f"tier={n} rows={len(rows)} states={dict(states)} dupKey={len(rows) - len({r[0] for r in rows})}")
print(f"waves={len(waves)} rowsPerWave={min(waves.values())}~{max(waves.values())}")
print(f"lag p50={quantile(lags, .5)} p95={quantile(lags, .95)} p99={quantile(lags, .99)} max={round(max(lags), 3) if lags else None}")
