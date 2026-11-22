#!/bin/bash
# xxl-job 档位压测：tier.sh <任务数> <采样秒数>
set -u
N=${1:-500}
DUR=${2:-40}
# PHASES=1：所有任务同相位（全在 0 秒，一波全到期）；PHASES=10：按 n%10 摊到 10 个秒位
PHASES=${3:-1}
PERIOD=${4:-10}
END=$((N+1))
MY="docker exec -i mysql8 mysql -uroot -p123456"
LOG=/tmp/xxlbench/run.tsv

if [ "$PHASES" = "1" ]; then CRON_EXPR="'0/$PERIOD * * * * ?'"; else CRON_EXPR="CONCAT((n % $PHASES), '/$PERIOD * * * * ?')"; fi

echo "== 清理日志与任务 =="
$MY -e "delete from xxl_job.xxl_job_log; delete from xxl_job.xxl_job_info where id>1;" 2>&1 | grep -v Using

echo "== 播种 $N 个任务（cron 0/10 * * * * ?） =="
$MY -e "
SET SESSION cte_max_recursion_depth = 200000;
INSERT INTO xxl_job.xxl_job_info
(id,job_group,job_desc,add_time,update_time,author,alarm_email,schedule_type,schedule_conf,
 misfire_strategy,executor_route_strategy,executor_handler,executor_param,executor_block_strategy,
 executor_timeout,executor_fail_retry_count,glue_type,glue_source,glue_remark,glue_updatetime,child_jobid,
 trigger_status,trigger_last_time,trigger_next_time)
WITH RECURSIVE seq AS (SELECT 2 AS n UNION ALL SELECT n+1 FROM seq WHERE n < $END)
SELECT n,1,CONCAT('bench-',n),NOW(),NOW(),'bench','','CRON',$CRON_EXPR,
 'DO_NOTHING','ROUND','demoJobHandler','','DISCARD_LATER',
 0,0,'BEAN','','bench',NOW(),'',
 1,0,FLOOR(UNIX_TIMESTAMP(NOW(3))/$PERIOD+1)*$PERIOD*1000 + (n % $PHASES)*1000
FROM seq;
SELECT COUNT(*) AS seeded FROM xxl_job.xxl_job_info WHERE id>1;" 2>&1 | grep -v Using

echo "== 预热 30s =="
sleep 30

echo "== 采样 ${DUR}s =="
T0=$(python3 -c 'import time;print(time.time_ns()//1000000)')
sleep "$DUR"
T1=$(python3 -c 'import time;print(time.time_ns()//1000000)')

python3 - "$T0" "$T1" "$N" << 'PY'
import sys
T0,T1,N=int(sys.argv[1]),int(sys.argv[2]),int(sys.argv[3])
ts=[int(l) for l in open('/tmp/xxlbench/run.tsv') if l.strip()]
win=[t for t in ts if T0<=t<T1]
b={}
for t in win: b.setdefault(t//10000,[]).append(t)
expected=N*(T1-T0)//10000
print(f"采样跨度 {(T1-T0)/1000:.1f}s  到达 {len(win)} 条  理论 {expected} 条")
full=[]
for k,v in sorted(b.items()):
    span=max(v)-k*10000
    print(f"  波 k={k} 条数={len(v):5d} 跨度={span:5d}ms")
    if k*10000+10000<=T1 and k*10000>=T0: full.append(span)
if full:
    full.sort()
    p=lambda q: full[min(len(full)-1,int(len(full)*q))]
    print(f"单波下发完成跨度: p50={p(.5)}ms p95={p(.95)}ms max={full[-1]}ms  (完整波 {len(full)} 个)")
PY

echo "== DB 侧交叉验证 =="
A=$(TZ=Asia/Shanghai date -r $((T0/1000)) +"%Y-%m-%dT%H:%M:%S")
B=$(TZ=Asia/Shanghai date -r $((T1/1000)) +"%Y-%m-%dT%H:%M:%S")
echo "窗口内 misfire=$(docker logs --since "$A" --until "$B" ${ADMIN_C:-xxl-admin} 2>&1 | grep -c 'schedule misfire')  executeTooFast=$(docker logs --since "$A" --until "$B" ${ADMIN_C:-xxl-admin} 2>&1 | grep -c 'execute too fast')  reject=$(docker logs --since "$A" --until "$B" ${ADMIN_C:-xxl-admin} 2>&1 | grep -ci 'RejectedExecution')"
$MY -e "select count(*) as log_rows, min(trigger_time) as first_t, max(trigger_time) as last_t,
 sum(case when trigger_code=200 then 1 else 0 end) as ok,
 sum(case when trigger_code<>200 then 1 else 0 end) as bad
 from xxl_job.xxl_job_log;" 2>&1 | grep -v Using
$MY -e "select trigger_code, count(*) from xxl_job.xxl_job_log group by trigger_code order by 2 desc limit 6;" 2>&1 | grep -v Using
