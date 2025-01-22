## 审计日志

审计日志，也称为审计追踪或审计记录，是在计算机系统、网络设备、数据库或其他IT环境中自动或手动生成的一系列记录。这些记录详细地追踪和描述了系统中的各种活动和事件，对于确保安全合规性、故障排查、性能分析以及潜在的非法访问或操作的调查至关重要。

>框架实现层面主要记录一下信息


1. 请求人相关信息，如 用户编号、用户名、昵称、租户编号
2. 请求信息，如 方法、客户端Ip、UserAgent、请求时间、返回时间、请求结果、异常信息
3. 报文信息，如 http.url、http.path、http.method、http.status_code、http.request.body、http.response.body
4. 执行SQL信息，如 执行的SQL语句、语句参数、执行耗时
5. 实体信息，如 实体路径、实例操作属性集合及属性值


>使用说明

由于不使用实体状态追踪，所以无法获取实体属性变更之前的值。

系统通过AuditOperationAttribute来进行触发记录。

可以将AuditOperationAttribute放在Controller类上或者Action上，也可以进行全局配置。

当进行全局配置时，可以通过DisableAuditingAttribute来控制关闭某些接口的审计。

>使用示例

```js
// 全局配置
builder.Services.AddControllers().AddMvcFilter<AuditOperationAttribute>()

// Controller或者Action
[HttpGet("update"), AuditOperation]
public async Task<Guid> UpdateAsync()

// 禁用
[HttpGet("metrics"), Description("系统指标")]
[DisableAuditing]
public async Task<CommonResult> MetricsAsync()

// Json配置
{
    "Findx": {
        "Auditing": {
           "Enabled": true, // 是否开启
           "ExtractRequestBody": false, // 是否提取请求报文
           "ExtractResponseBody": false // 是否提取返回报文
        },
        "FreeSql": {
           "Enabled": true,
           "Primary": "system",
           "Strict": true,
           "DataSource": {
              "system": {
                 "ConnectionString": "xxxx",
                 "DbType": "MySql",
                 "PrintSql": true,
                 "OutageDetection": true,
                 "OutageDetectionInterval": 1,
                 "SoftDeletable": true,
                 "AuditEntity": true, // 实体审计
                 "AuditSqlRaw": true  // Sql语句审计
             }
          }
        }
    }
}
```
>存储扩展

- 由于存储需要使用存储介质，每个项目可能都不同，可以自实现IAuditStore里的SaveAsync方法。
建议使用异步方式进行记录。
- 系统也内置了数据库的存储，可以查看扩展包 Findx.Extensions.AuditLogs
