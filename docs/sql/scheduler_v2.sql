-- ============================================================
-- Findx 分布式调度升级（DB-only 版本）建表脚本
--   MySQL 8.0+ / utf8mb4
--   说明：
--     1. 组件运行依赖这三张表；仓储未声明索引，唯一键与查询索引必须由本脚本显式建立。
--     2. 表名与代码一致：scheduler_master / scheduler_worker / scheduler_execution。
--     3. 主键列名为 id（代码中 [Column(name = "id")]），其余列沿用实体属性名（PascalCase），
--        与仓库其它 FreeSql 实体保持一致。
--     4. 三张表随宿主 DataSource="jobs" 使用；多环境共库时可在 scheduler_master 的 LockKey 上区分集群。
-- ============================================================

-- ------------------------------------------------------------
-- 调度端租约（单行表，Id 固定为 1）
--   并发抢占靠条件 UPDATE：WHERE id=1 AND (Owner=@me OR LeaseUntil<=@now)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `scheduler_master`
(
    `id`               bigint       NOT NULL COMMENT '固定主键，恒为 1',
    `LockKey`          varchar(100) NOT NULL DEFAULT 'findx:scheduler:master' COMMENT '租约键，多集群隔离用',
    `Owner`            varchar(200)          DEFAULT NULL COMMENT '当前持有者：{WorkerId}#{ProcessId}',
    `LeaseUntil`       datetime(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '租约到期时刻，读侧据此判活',
    `UpdatedAt`        datetime(3)           DEFAULT NULL COMMENT '最后更新时刻（排障用）',
    `CreatorId`        bigint                DEFAULT NULL,
    `Creator`          varchar(64)           DEFAULT NULL,
    `CreatedTime`      datetime(3)           DEFAULT NULL,
    `LastUpdaterId`    bigint                DEFAULT NULL,
    `LastUpdater`      varchar(64)           DEFAULT NULL,
    `LastUpdatedTime`  datetime(3)           DEFAULT NULL,
    PRIMARY KEY (`id`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4 COMMENT ='调度主节点租约（只决定谁扫描，不承载正确性）';

-- ------------------------------------------------------------
-- Worker 节点注册（执行端 HTTP 心跳写入）
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `scheduler_worker`
(
    `id`               bigint       NOT NULL AUTO_INCREMENT,
    `WorkerId`         varchar(64)  NOT NULL COMMENT '节点稳定编号（由 NodeName 推导）',
    `Name`             varchar(128)          DEFAULT NULL COMMENT '节点显示名',
    `Address`          varchar(200) NOT NULL COMMENT '节点对外地址 http://host:port',
    `Weight`           int          NOT NULL DEFAULT 1 COMMENT '分发权重（预留）',
    `Capabilities`     text                  DEFAULT NULL COMMENT '能力清单 JSON（IJob 实现类全名数组）',
    `HeartbeatAt`      datetime(3)  NOT NULL COMMENT '最近心跳时刻',
    `CreatorId`        bigint                DEFAULT NULL,
    `Creator`          varchar(64)           DEFAULT NULL,
    `CreatedTime`      datetime(3)           DEFAULT NULL,
    `LastUpdaterId`    bigint                DEFAULT NULL,
    `LastUpdater`      varchar(64)           DEFAULT NULL,
    `LastUpdatedTime`  datetime(3)           DEFAULT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_worker_id` (`WorkerId`),
    KEY `idx_worker_heartbeat` (`HeartbeatAt`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4 COMMENT ='调度执行节点注册表';

-- ------------------------------------------------------------
-- 执行账本（一次触发一行，状态机 Claimed->Dispatched->Succeeded/Failed/Cancelled）
--   uk_exec_trace：结果回调按 TraceId 定位并做幂等终结
--   uk_exec_key  ：jobId:period:attempt，同周期同尝试不可重复写入
--   idx_exec_state_dispatch：超时兜底按 (State, DispatchAt) 扫描
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `scheduler_execution`
(
    `id`               bigint       NOT NULL AUTO_INCREMENT,
    `TraceId`          varchar(64)  NOT NULL COMMENT '执行链路号，回调幂等键',
    `ExecutionKey`     varchar(200) NOT NULL COMMENT 'jobId:period:attempt',
    `JobId`            bigint       NOT NULL,
    `JobName`          varchar(200)          DEFAULT NULL,
    `HandlerType`      varchar(300)          DEFAULT NULL,
    `Parameter`        text                  DEFAULT NULL,
    `Period`           varchar(100)          DEFAULT NULL COMMENT '周期标识（到期时刻或 R: 前缀重试）',
    `Attempt`          int          NOT NULL DEFAULT 0 COMMENT '尝试序号，0=首次',
    `State`            tinyint      NOT NULL COMMENT '1=Claimed 2=Dispatched 3=Succeeded 4=Failed 5=Cancelled',
    `WorkerAddress`    varchar(200)          DEFAULT NULL,
    `DispatchAt`       datetime(3)           DEFAULT NULL COMMENT '领取/分发时刻，超时判定依据',
    `FinishAt`         datetime(3)           DEFAULT NULL,
    `ElapsedMs`        bigint                DEFAULT NULL,
    `Error`            text                  DEFAULT NULL COMMENT '失败原因',
    `Message`          text                  DEFAULT NULL COMMENT '执行结果消息（JobResult.Message）',
    `JobLog`           mediumtext            DEFAULT NULL COMMENT '执行过程日志（执行端采集，超出上限保留尾部）',
    `CreatorId`        bigint                DEFAULT NULL,
    `Creator`          varchar(64)           DEFAULT NULL,
    `CreatedTime`      datetime(3)           DEFAULT NULL,
    `LastUpdaterId`    bigint                DEFAULT NULL,
    `LastUpdater`      varchar(64)           DEFAULT NULL,
    `LastUpdatedTime`  datetime(3)           DEFAULT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_exec_trace` (`TraceId`),
    UNIQUE KEY `uk_exec_key` (`ExecutionKey`),
    KEY `idx_exec_state_dispatch` (`State`, `DispatchAt`),
    KEY `idx_exec_job` (`JobId`, `Id`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4 COMMENT ='调度执行账本（幂等/审计/超时兜底）';

-- ------------------------------------------------------------
-- 执行日志明细（一次执行多行，执行端按分片追加；账本行只保留尾部摘要）
--   idx_log_trace：按 TraceId + 主键游标分页
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `scheduler_execution_log`
(
    `id`               bigint       NOT NULL AUTO_INCREMENT,
    `TraceId`          varchar(64)  NOT NULL COMMENT '执行链路号',
    `JobId`            bigint       NOT NULL DEFAULT 0,
    `Sequence`         int          NOT NULL DEFAULT 0 COMMENT '同一次执行内的分片序号',
    `Content`          mediumtext            DEFAULT NULL COMMENT '分片内容（多行文本）',
    `IsFinal`          tinyint(1)   NOT NULL DEFAULT 0 COMMENT '是否最后一片',
    `CreatorId`        bigint                DEFAULT NULL,
    `Creator`          varchar(64)           DEFAULT NULL,
    `CreatedTime`      datetime(3)           DEFAULT NULL,
    `LastUpdaterId`    bigint                DEFAULT NULL,
    `LastUpdater`      varchar(64)           DEFAULT NULL,
    `LastUpdatedTime`  datetime(3)           DEFAULT NULL,
    PRIMARY KEY (`id`),
    KEY `idx_log_trace` (`TraceId`, `id`),
    KEY `idx_log_job` (`JobId`, `id`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4 COMMENT ='执行过程日志明细';

-- ------------------------------------------------------------
-- 存量任务表补充索引（可选，用于到期扫描；表由组件首次运行时自动建）
--   scheduler_jobs 由 ScheduledJob 实体自动同步，若已存在可直接执行：
-- ------------------------------------------------------------
-- ALTER TABLE `scheduler_jobs`
--     ADD KEY `idx_job_status_next` (`Status`, `NextRunTime`),
--     ADD KEY `idx_job_status_retry` (`Status`, `RetryAtTime`);

-- ------------------------------------------------------------
-- 存量执行账本补充结果与日志列（V2.1 起；新库由上面的建表语句直接包含）
-- ------------------------------------------------------------
-- ALTER TABLE `scheduler_execution`
--     ADD COLUMN `Message` text DEFAULT NULL COMMENT '执行结果消息（JobResult.Message）',
--     ADD COLUMN `JobLog` mediumtext DEFAULT NULL COMMENT '执行过程日志（执行端采集）';
