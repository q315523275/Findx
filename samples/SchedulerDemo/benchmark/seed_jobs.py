#!/usr/bin/env python3
"""压测前播种 N 个 stress-* 任务，周期 */10s，处理器指向 BenchmarkJob。

用法：
    python3 seed_jobs.py 500          # 任务立即返回
    python3 seed_jobs.py 500 50       # 每个任务模拟 50ms 业务耗时
随后把生成的 SQL 灌给 MySQL（run_tier.sh 已自动完成）。
"""
import datetime
import sys

N = int(sys.argv[1])
DELAY = int(sys.argv[2]) if len(sys.argv) > 2 else 0
PARAM = f"'{DELAY}'" if DELAY > 0 else 'NULL'
BASE_ID = 810000000000000000

now = datetime.datetime.now().replace(microsecond=0)
step = (10 - now.second % 10) % 10 or 10
nxt = now + datetime.timedelta(seconds=step)

rows = []
for i in range(N):
    name = f"stress-{i:04d}"
    rows.append(
        f"({BASE_ID + i},'{name}','Findx.Samples.SchedulerWorker.Jobs.BenchmarkJob',"
        f"'*/10 * * * * ?',{PARAM},'Normal',NULL,'{nxt:%Y-%m-%d %H:%M:%S}.000',NULL,0,'stress')"
    )

header = ("SELECT COUNT(*) FROM scheduler_execution;")
sql = (
    "USE findx_scheduler_demo;\n"
    "DELETE FROM scheduler_execution_log WHERE TraceId IN "
    "(SELECT TraceId FROM scheduler_execution WHERE JobName LIKE 'stress-%');\n"
    "DELETE FROM scheduler_execution WHERE JobName LIKE 'stress-%';\n"
    "DELETE FROM scheduler_jobs WHERE JobName LIKE 'stress-%';\n"
    "INSERT INTO scheduler_jobs "
    "(Id,JobName,HandlerType,CronExpression,Parameter,Status,LastRunTime,NextRunTime,RetryAtTime,TryCount,Remark) VALUES\n"
    + ",\n".join(rows)
    + ";\n"
)

with open("/tmp/stress_seed.sql", "w", encoding="utf-8") as f:
    f.write(sql)

print(f"seeded {N} jobs, first run at {nxt}")
