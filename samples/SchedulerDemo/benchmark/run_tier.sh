#!/bin/bash
# 分布式调度压测：N 个任务 / 1 调度端 / 多执行端，采样 DUR 秒。
#
# 用法：
#   ./run_tier.sh 500 60                  # 默认：2 执行端、4 分发线程、领取批量 50、在飞 64
#   ./run_tier.sh 3000 60 4 8 50          # 4 执行端、8 分发线程、领取批量 50
#   ./run_tier.sh 5000 40 4 8 50 64 16384 # 第 6 参 = 在飞请求上限，第 7 参 = 分发队列容量
#   ./run_tier.sh 5000 40 4 8 50 64 16384 100  # 第 8 参 = 每任务模拟 100ms 业务耗时
#
# 前置：MySQL 容器 mysql8 内已有 findx_scheduler_demo 库（docs/sql/scheduler_v2.sql）。
# 注意：每次运行前会清空 scheduler_worker，避免上一轮已关闭的节点在 TTL 内仍被选中，
#      导致投递打空端口、每条约耗满 DispatchHttpTimeoutSeconds（5s），把延迟数据污染成 5s 台阶。
set -u

N=${1:?任务数}
DUR=${2:-60}
WORKERS=${3:-2}
DISPATCH_THREADS=${4:-4}
BATCH=${5:-50}
IN_FLIGHT=${6:-64}
QUEUE_CAP=${7:-16384}
DELAY=${8:-0}   # 每个任务模拟的业务耗时（毫秒），0=立即返回
EXEC_THREADS=${9:-8}    # 单个执行端并发执行线程数
EXEC_QUEUE=${10:-1024}  # 单个执行端执行队列容量
SHIP_QUEUE=${11:-4096}  # 单个执行端结果上报队列容量

ROOT=$(cd "$(dirname "$0")/../../.." && pwd)
cd "$ROOT"

M() { docker exec -i mysql8 mysql -uroot -p123456 -N -B -e "USE findx_scheduler_demo; $1" 2>/dev/null; }

# 1) 停旧进程
for P in 7100 7101 7102 7103 7104; do
  PID=$(lsof -nP -iTCP:$P -sTCP:LISTEN 2>/dev/null | awk 'NR==2{print $2}')
  [ -n "${PID:-}" ] && kill -9 $PID
done
sleep 1

# 2) 播种
python3 "$ROOT/samples/SchedulerDemo/benchmark/seed_jobs.py" "$N" "$DELAY" >/dev/null
docker exec -i mysql8 mysql -uroot -p123456 < /tmp/stress_seed.sql 2>/dev/null
M "delete from scheduler_worker" >/dev/null
M "select count(*) from scheduler_jobs where JobName like 'stress-%'"

# 3) 调度端 + 执行端
Findx__Scheduler__DispatchWorkerCount=$DISPATCH_THREADS \
  Findx__Scheduler__DispatchClaimBatchSize=$BATCH \
  Findx__Scheduler__DispatchMaxInFlight=$IN_FLIGHT \
  Findx__Scheduler__DispatchQueueCapacity=$QUEUE_CAP \
  Findx__Scheduler__SchedulerScanIntervalSeconds=1 \
  nohup dotnet run --no-build --project samples/SchedulerServer > /tmp/stress-server.log 2>&1 &
for i in $(seq 1 "$WORKERS"); do
  PORT=$((7100 + i))
  Urls=http://127.0.0.1:$PORT Findx__Scheduler__NodeName=stress-worker-$i \
    Findx__Scheduler__NodeAddress=http://127.0.0.1:$PORT \
    Findx__Scheduler__ExecutionWorkerCount=$EXEC_THREADS \
    Findx__Scheduler__ExecutionQueueCapacity=$EXEC_QUEUE \
    Findx__Scheduler__ResultShipQueueCapacity=$SHIP_QUEUE \
    nohup dotnet run --no-build --project samples/SchedulerWorker > /tmp/stress-worker$i.log 2>&1 &
done

# 4) 预热 30s（选主 + 窗口加载 + 节点注册），然后清空账本获得干净基线
sleep 30
M "delete from scheduler_execution_log where TraceId in (select TraceId from scheduler_execution where JobName like 'stress-%')" >/dev/null
M "delete from scheduler_execution where JobName like 'stress-%'" >/dev/null
Q0=$(M "show global status like 'Questions'" | awk '{print $2}')

# 5) 采样
for i in $(seq 1 $((DUR / 5))); do
  sleep 5
  echo "t=$((i*5))s exec=$(M "select count(*) from scheduler_execution where JobName like 'stress-%'") q=$(M "show global status like 'Questions'" | awk '{print $2}')" \
    >> "/tmp/stress-sample-$N.log"
done
Q1=$(M "show global status like 'Questions'" | awk '{print $2}')

# 6) 导出明细（账本即事实：Period / DispatchAt 用于计算延迟）
M "select ExecutionKey,Period,State,ifnull(date_format(DispatchAt,'%Y-%m-%dT%H:%i:%s.%f'),'') from scheduler_execution where JobName like 'stress-%'" \
  > "/tmp/stress-rows-$N.tsv"

echo "questions_delta=$((Q1 - Q0))"
echo "saturated=$(grep -c '分发队列已饱和' /tmp/stress-server.log || true)"
echo "slowloop=$(grep -c '调度循环单轮耗时' /tmp/stress-server.log || true)"

M "select ifnull(date_format(DispatchAt,'%Y-%m-%dT%H:%i:%s.%f'),''), ifnull(date_format(FinishAt,'%Y-%m-%dT%H:%i:%s.%f'),''), ifnull(ElapsedMs,0), State, Attempt from scheduler_execution where JobName like 'stress-%'" > "/tmp/stress-exec-$N.tsv"
echo "worker队列满拒绝受理=$(cat /tmp/stress-worker*.log 2>/dev/null | grep -c '执行队列已满')"
python3 "$ROOT/samples/SchedulerDemo/benchmark/summarize_exec.py" "$N"

python3 "$ROOT/samples/SchedulerDemo/benchmark/summarize.py" "$N"
