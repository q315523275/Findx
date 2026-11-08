# Findx 逐包分析与升级计划

> 分析时间：2026-10-14
> 分析范围：`framework/` 22 个包、`components/` 4 个包、`samples/`、`modules/`
> 核实方式：全部结论均经源码复核，每条给出文件路径与行号

> 相关文档：各包 API 参考见 [`../docs/`](../docs/)；项目说明见 [README](../README.md)

## 一、项目基线现状

| 维度 | 现状 | 影响 |
|---|---|---|
| **SDK** | 本机并存 6.0.428 / 8.0.416 / 10.0.100；`global.json` 锁 `8.0.0` 但 `rollForward: latestMajor` | 版本不确定，实际可能拉到任意大版本 |
| **目标框架** | 5 种写法混用：26 个包 `net6.0;net8.0;net10.0`，3 个仍是 `net9.0`，`Findx.Net` 缺 `net10.0` | 条件编译分支易漏，多目标验证成本高 |
| **CI/CD** | **完全没有** | 无构建验证、无自动发版、无回归拦截 |
| **测试** | **一个测试项目都没有** | 890+ 源文件零回归保障，改动风险不可控 |
| **版本管理** | 26 处 `PackageVersion` 硬编码在 csproj，无 `Directory.Build.props` / `Directory.Packages.props` | 发版需逐文件改，极易漏改 |
| **构建健康度** | 核心包 net8.0 构建成功，6 个 warning（1 个 Obsolete + 5 个 CS1591 缺注释） | 基础可编译，但缺少持续验证 |
| **仓库卫生** | 根目录残留空壳目录 `Findx.IP/`（真实包在 `framework/` 下）；`.DS_Store` 入库 | 误导新人，冗余提交 |
| **提交节奏** | 292 commits，单人主导（tianliang 278 次），2021-01 至今 | 知识集中在一人，bus factor = 1 |

---

## 二、逐包分析

### 1. Findx（核心包，442 文件）

**定位**：所有包的公共底座，提供接口定义 + 默认实现。

**已核实的架构性问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| AES 默认密钥硬编码 | `Utilities/EncryptUtility.cs:84` | **Critical** | `private const string AesKey = "MDEyMzQ1Njc4OWFiY2RlZg==";`，且 `AesEncrypt(content, key = AesKey)` 把密钥做成了**默认参数**——调用方不传密钥时静默使用这个公开常量 |
| `ServiceLocator` 静态服务定位器 | `Common/`、`ServiceLocator.cs` | **High** | 全仓大量使用（`AuditOperationAttribute`、`FreeSqlModule`、`TraceIdentifierLayoutRenderer`、`RabbitConsumerBuilder` 等），本质是隐藏的全局单例，无法在单测中隔离 |
| 可写静态属性 | `Findx/Common/ClaimTypes.cs` | **Medium** | 所有 `ClaimTypes.UserId` 等均为 `public static ... { set; }`，运行期可被任意代码改写，影响全局令牌语义 |
| 三个「空实现」陷阱 | `NullUnitOfWorkManager`、`NullIpGeolocation`、`NullSmsSender` | **Medium** | 前两者方法全抛 `NotImplementedException`（不装对应包直接崩），`NullSmsSender` 反向——**静默返回成功**，验证码/短信接口会「假通过」 |
| 授权判定空桩 | `Security/FunctionAuthorizationBase.cs:139-145` | **High** | `AuthorizeAuthorityLimit` 直接 `return AuthorizationStatus.Ok;`，注释 `// Todo 自行实现资源的验证`。意味着 `[PreAuthorize(Authority="...")]` **完全不生效**，`RoleAuthorityLimit`（第 164 行）又依赖它，等于两档权限形同虚设 |
| 有序 Guid 内置实现 | `NewId/SequentialGuidGenerator.cs` | **Low** | ABP 风格「6 字节时间戳 + 10 字节随机」，仅有序不连续。已有 `Findx.Guids.NewId` 提供更快替代 |
| 实体审计弱实现 | `FreeSql/FreeSqlAuditEntityReport.cs:19-42` | **Medium** | 两个 `AuditEntity` 方法体**全部被注释掉**（`// Todo 当前弱实现`），实际只有 `AuditEntityProperty` 生效，即审计日志拿不到实体级变更快照 |

**升级建议（按优先级）**：
1. `AesKey` 移除默认参数值，改为强制传入；保留旧方法并 `[Obsolete]` 标注一个版本后删除
2. 补齐 `AuthorizeAuthorityLimit`——框架内提供基于 `IFunctionStore` 的默认实现（查角色-权限关联表），而不是让用户继承重写
3. `NullSmsSender` 改为**抛异常**（或至少返回失败结果），避免「假成功」
4. `ServiceLocator` 划出边界：仅限 `StartupModule.ConfigureServices` 与日志渲染器等启动期场景，业务代码全面改注入

---

### 2. Findx.AspNetCore（40 文件）

**定位**：ASP.NET Core 宿主集成，MVC 过滤器、管道扩展、Crud 基类。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **`GetClientIp` 无条件信任 XFF** | `Extensions/Extensions.HttpRequest.cs:56` | **Critical** | `Headers["X-Forwarded-For"].Split(',')[0]` 作为首选来源，任何客户端都能伪造。而这个 IP 被 `InternalNetworkLimiterAttribute`（内网限定）、`RateLimiterAttribute`（限速）、`IpHashSelector`（负载均衡）依赖 —— **伪造 XFF 即可绕过内网限制、操纵限速键、把流量固定打到某个实例** |
| `ListAsync` 分页硬编码 | `Mvc/QueryControllerBase.cs:115` | **Medium** | `if (req.PageSize == 20) req.PageSize = 2000;` 带 TODO 注释，前端传默认 20 时被静默改成 2000，全量拉取 |
| 已废弃 API 仍暴露 | `Extensions.ApplicationBuilder.cs` | **Low** | `UseFindx(this IApplicationBuilder)` 标 `[Obsolete]` 且**方法体为空**；`MapControllersWithAreaRoute` 亦已废弃。README 第 113-115 行仍在用这两个 |
| `GetClientIp` 可能抛异常 | 同上 `:68` | **Medium** | `Check.NotNullOrWhiteSpace(ip)` 在所有来源都取不到 IP 时抛出，而限速/防重过滤器**每次请求都调它** |
| 限速/防重仅支持方法级标注 | `Mvc/Filters/RateLimiterAttribute.cs` | **Low** | `[AttributeUsage]` 只有 `Method`，不能标注在 Controller 上 |

**升级建议**：
1. **优先修 `GetClientIp`**：改为可配置的可信代理列表（`KnownProxies`），不在可信列表内的来源忽略 XFF；这是本包最高优先级改动
2. 删除 `ListAsync` 的 `PageSize` 魔法数字，或改为 `MaxPageSize` 配置项显式声明上限
3. 清理已废弃 API，同步修正 README 启动示例

---

### 3. Findx.Security（17 文件）

**定位**：JWT / Cookie 认证 + 接口级功能权限。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| 权限资源校验完全失效 | 核心包 `Security/FunctionAuthorizationBase.cs:139` | **High** | 见上，`AuthorityLimit` / `RoleAuthorityLimit` 两档返回 `Ok`，等于只有「角色」一档真正生效 |
| 两个模块开关易漏 | `AuthenticationModule` / `AuthorizationModule` | **Medium** | 读 `GetValue<bool>("Findx:Authentication:Enabled")` / `Findx:Authorization:Enabled`，**缺省 false**，未显式配置则整块不注册 |
| `IFunctionStore` 仅内存 | `MvcFunctionStore` | **Medium** | 功能清单只在内存，进程重启丢失，且不自动持久化（需自行 `SyncToDatabaseAsync`） |
| 超级管理员后门 | `FunctionAuthorizationBase` | **Low** | `SuperRoleName` 命中直接 `Ok`，需确保该角色不可被普通用户自行授予 |

**升级建议**：
1. 实现 `AuthorizeAuthorityLimit` 默认实现（查 `RolesResources` 关联），把「权限」维度真正落地
2. 认证/授权开关合并为单一 `Findx:Security:Enabled`，减少配置遗漏
3. 提供 `IDistributedCache` 版 `IFunctionStore` 实现，或明确文档化持久化契约

---

### 4. Findx.FreeSql（12 文件）

**定位**：FreeSql ORM 集成，仓储 / 工作单元 / 多租户 / 分表。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| 实体审计空实现 | `FreeSqlAuditEntityReport.cs:19-42` | **Medium** | `AuditEntity` 两个重载方法体全注释，`AuditEntity` 配置开启后仍无实体级记录 |
| 实体变更前值不可得 | 同上 `AuditValue` AOP | **Medium** | `AuditEntityPropertyEntry.OriginalValue` 未赋值，审计只能记「新值」，无法回答「改之前是什么」 |
| `CountAsync` 溢出风险 | `RepositoryWithTypedId.Async.cs` | **Low** | 内部先 `.To<int>()` 再隐式转 `long`，结果 > `int.MaxValue` 时溢出 |
| `UnitOfWork.InternalSaveChangesAsync` 空实现 | `UnitOfWork.cs` | **Low** | 返回 `Task.CompletedTask`。FreeSql 无变更跟踪所以不影响 CRUD，但对使用者有误导性 |
| `Enabled` 默认 true 但需显式配 | `FreeSqlModule` | **Low** | 实际读的是 Options 绑定值（与其他包不同），行为正确但与其他包不一致 |

**升级建议**：
1. 用 FreeSql `AuditValue` 的 `OldValue` 补齐 `OriginalValue`，让审计能记录变更前后
2. `CountAsync` 改为直接 `long` 转换
3. 移除或明确文档化 `InternalSaveChangesAsync` 的空实现

---

### 5. Findx.Redis（10 文件）

**定位**：Redis 客户端 + 分布式锁 + Redis 缓存实现。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **同步/异步接口签名不一致** | `Findx/Redis/IRedisClient.cs:991 / 1108` | **Medium** | `ZRangeByScore<T>(..., long count = -1, ...)` 而 `ZRangeByScoreAsync<T>(..., long count = 0, ...)`。默认参数不同意味着**同步返回全部、异步默认不返回**，调用方极易踩坑 |
| `HKeys` 缺异步版本 | 同上 | **Low** | 接口不对称 |
| `Enabled` 必须显式配置 | `RedisModule.cs` | **Medium** | 读原始 `GetValue<bool>`，`RedisOptions.Enabled` 属性默认 true 不起作用，不写即整个模块不注册 |
| `SearchKeys` 阻塞风险 | `RedisClient` 实现 | **Medium** | 内部 `server.Keys(pattern)`，大数据量下阻塞 Redis 单线程 |
| 注入 `ICache`/`ILock` 被静默替换 | `RedisModule` | **Medium** | 装本包后 `ICache` 变 `RedisCache`、`ILock` 变 `RedisLock`，与不装时的行为完全不同，无编译期提示 |

**升级建议**：
1. 统一同步/异步默认参数（建议都默认 `-1` 或都默认 `0`，但必须一致），并在文档中标注
2. `Enabled` 判定改用 Options 绑定，与 `Findx.FreeSql` 保持一致的模式
3. `SearchKeys` 加保护：文档明确标注禁止在生产主库使用，或改用 `SCAN` 分批

---

### 6. Findx.Discovery（28 文件）

**定位**：服务发现抽象 + 4 种负载均衡 + HttpClient 端点改写。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **最少连接算法实际未生效** | `HttpMessageHandlers/DiscoveryLeastConnectHttpMessageHandler.cs` | **Medium** | 传给 `GetAsync` 的是 `LoadBalancerType.RoundRobin` 而非 `LeastConnection`，`LeastConnectionSelector` 在此路径下永远不会被创建。API 名称与行为不符 |
| 服务名直接取自 URL Host | 4 个 Handler 共用逻辑 | **Medium** | `GetAsync(current.Host)` 意味着若误把真实 IP 写进 BaseAddress，会去查一个不存在的服务名，报错信息不直观 |
| `Enabled` 必须显式配置 | `DiscoveryCoreModule` | **Medium** | 同 Redis，无默认值的 `GetValue<bool>` |
| `IpHashSelector` 强依赖静态定位 | `LoadBalancer/Selectors/IpHashSelector.cs` | **Low** | 取客户端 IP 走 `ServiceLocator.GetService<IThreadCurrentClientIpAccessor>()`，与第 1 点的 XFF 伪造问题叠加 |

**升级建议**：
1. 修复 `DiscoveryLeastConnectHttpMessageHandler` 传入的枚举（或明确改名为 `AddRoundRobinLoadBalancer` 的别名并标注）
2. 服务名解析失败时抛出可诊断的异常（提示「检测到 IP 而非服务名，请检查 BaseAddress」）
3. 与 AspNetCore 的可信代理改造联动，保证负载均衡键可信

---

### 7. Findx.Discovery.Consul（10 文件）

**定位**：Consul 作为服务发现后端。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| Consul 客户端不可热更新 | `ConsulDiscoveryModule.cs` | **Medium** | `IConsulClient` 单例在注册时按 `IOptionsMonitor.CurrentValue` 创建，运行期改 `Findx:Consul` 配置**不重建客户端**，需重启 |
| 开关路径不一致 | 同上 | **Low** | 配置在 `Findx:Consul` 下，但开关读 `Findx:Discovery:Enabled`，容易误解 |
| 无重连/重试策略 | `ConsulServiceRegistry` | **Low** | 4 个方法直接调用 Consul API，失败即返回 false，无退避重试 |

**升级建议**：
1. `IConsulClient` 改为工厂 + `IOptionsMonitor` 委托，支持配置热更新
2. 注册/注销失败增加指数退避重试

---

### 8. Findx.RabbitMQ（26 文件）

**定位**：RabbitMQ 封装，特性标注消费者 + 连接/通道池。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **发布端固定走默认连接** | `RabbitMqPublisher.cs` | **Medium** | `PublishAsync` 内部 `GetAsync(ct)` 无 `connectionName` 参数，多连接配置（如示例中的 `Order`）在**发布时无法指定**，只能全部发到 Default |
| **topic 模式不支持** | `RabbitConsumerBuilder.cs` | **Medium** | 路由按 `eventArgs.RoutingKey` **精确匹配**，源码注释明确 `topic 模式暂不支持`。声明 `[RabbitConsumer(..., "topic", ...)]` 后通配符订阅不生效，且**无任何警告** |
| 异常也 Ack 导致消息丢失 | `RabbitMqConsumer.cs` | **Medium** | 处理异常后仍 `BasicAck`，注释写「避免重复投递」，但业务失败的消息被永久消费，无法重试或进死信 |
| 通道池独占阻塞 | `ChannelPool.cs` | **Low** | `Acquire()` 用 `Monitor.Wait` 阻塞，高并发下队头阻塞；销毁时 10s 等待预算 |
| `Enabled` 必须显式配置 | `FindxRabbitMqModule` | **Medium** | 同上模式 |

**升级建议**：
1. `PublishAsync` 增加 `connectionName` 参数（**注意这是破坏性变更**，建议新增重载 + `[Obsolete]` 旧方法）
2. topic 通配符：至少在检测到 `topic` 交换机 + 含 `*`/`#` 的 routingKey 时抛异常或打 Warning，别静默失效
3. 异常处理策略改为可配置：`Ack` / `Nack(requeue)` / `Nack(死信)`，默认至少进死信队列而非静默丢弃

---

### 9. Findx.WebSocketCore（23 文件）

**定位**：WebSocket 服务端 + 客户端。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **无 Token 即放行** | `Implementation/DefaultWebSocketAuthorization.cs:59` | **High** | 无 `Authorization` 头时 `return Task.FromResult(true)` —— 设计上是「给浏览器用」，但浏览器 WebSocket API 又**无法自定义请求头**，实际生产中几乎必然走这条放行路径，等于无鉴权 |
| **身份可 query 参数冒充** | `Middlewares/WebSocketMiddleware.cs:161` | **High** | `GetUserName` 第二优先级读 `Request.Query["userName"]`，任何人都能自称任意用户，进而接收该用户的定向消息 |
| **无 Origin 校验** | 同文件 | **High** | 整个 Middleware 无 `Origin` 头校验，跨站 WebSocket 劫持（CSWSH）成立：恶意页面可让用户浏览器连上本服务 |
| 分组只存 userName | `WebSocketSessionManager` | **Medium** | 同一用户多端连接时，组内无法精确区分某条连接 |
| `RemoveSessionAsync(session)` 不清分组 | 同上 | **Low** | 分组中残留已断开的用户名 |

**升级建议（这三个 High 建议合并成一次改造）**：
1. 增加 **Origin 白名单**配置（`Findx:WebSocketCore:AllowedOrigins`），默认拒绝跨域
2. 身份获取**只信任已认证主体**；query 传身份降级为「显式开关 + 开发环境默认开、生产默认关」
3. 支持 `Sec-WebSocket-Protocol` 或 query 传 token（校验后仍走 JWT 验证），让浏览器端也能鉴权

---

### 10. Findx.Configuration（10 文件）

**定位**：配置中心客户端，HTTP 长轮询热更新。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **签名仅 MD5 且无时间戳** | `ConfigClient.cs` `GenerateApiUrl` | **High** | `sign = Md5By32($"{AppId}{AppSecret}{Environment}{CurrentDataVersion}")` —— 全是静态值，签名可长期重放。若配置中心接口未额外鉴权（服务端 `ConfigController` 无 `[Authorize]`），任何人都能拉到配置内容（含密钥类配置） |
| 无参重载会提前 Build 配置 | `Extensions/ConfigurationBuilderExtension.cs` | **Medium** | `configurationBuilder.Build()` 后再读 `Findx:Config` 节，此时 `AddJsonFile` 等尚未全部添加，配置源不完整 |
| 轮询异常间隔与注释不符 | `ConfigClient.cs` | **Low** | 注释写 60 秒，实际 `Task.Delay(30 * 1000)` |
| 内存 + 本地文件双写 | `ConfigClient.cs` | **Low** | `IsRecovery` 开启时变更写 `appsettings.ConfigCache.json`，该文件含配置明文 |

**升级建议**：
1. 签名改 HMAC-SHA256 并加入时间戳 + nonce，服务端校验时间窗
2. `ConfigController` 增加 `[Authorize]` 或服务间 Token
3. 无参重载改为延迟解析（`IConfigurationBuilder` 的 source 在 Build 时才求值）

---

### 11. Findx.Swagger（7 文件）

**定位**：Swagger 文档生成与中文优化。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| 标签重排/分组功能未启用 | `SwaggerModule.bak.cs` | **Low** | 整份 net10 `AddOpenApi` 方案被注释备份；标签重排与分组需用户自行补 `Configure<SwaggerGenOptions>` |
| `SwaggerEndpoint.Servers` 未消费 | `SwaggerEndpoint.cs` | **Low** | 字段存在但无任何读取处，属预留配置 |

**升级建议**：低优先级。清理 `.bak.cs` 备份文件；把标签重排/分组作为可选扩展方法内置。

---

### 12. Findx.ImageSharp（6 文件）

**定位**：图片处理 + 验证码。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **内置字体无 CJK 字形** | `fonts/Actionj.ttf`、`Sketch-3D-1.otf` | **Medium** | 两个字体都不含中日韩字形，中文文字水印会渲染成豆腐块/空白。文档已说明，但**默认体验是坏的** |
| `ImageResizeMode.Default` 必抛异常 | `ImageProcessor.ResizeAsync` | **Medium** | `ResizeModeMap` 不含 `Default(7)`，未显式传 mode 直接抛 `FindxException` |
| PNG 压缩忽略 quality | `CompressAsync` | **Low** | 硬编码最佳压缩级别，传 `quality` 无效 |
| 灰度/二值化 CPU 开销高 | `GrayAsync` / `BinaryAsync` | **Low** | O(W×H) 逐像素 C# 循环，大图阻塞线程池线程（未异步化） |

**升级建议**：
1. 打包一个开源 CJK 字体（如 Noto Sans SC 子集化），或内置字体加载 + 文档给出三条回退路径
2. `ResizeModeMap` 补 `Default` 映射为 `Max`，或把 `Default` 设为 `Max` 的别名
3. 灰度/二值化改用 ImageSharp 的像素操作批处理，或至少加 `Task.Run` 隔离

---

### 13. Findx.DinkToPdf（32 文件）

**定位**：wkhtmltopdf 封装，HTML → PDF。

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **仅支持 Linux x86-64** | `libwkhtmltox.so`（ELF 64-bit x86-64，约 43MB） | **High** | Windows / macOS / ARM64 全部不可用，且包内**无 `DllImportResolver`** 做解析重定向。开发机（macOS ARM）根本跑不了，CI 也无法跨平台验证 |
| 转换完全串行 | `SynchronizedConverter.cs` | **Medium** | 所有转换投递到单一专用线程串行执行（wkhtmltopdf `init/deinit` 线程亲和要求），并发请求排队 |
| 无平台探测与友好报错 | `PdfTools.Load()` | **Medium** | 缺库时抛 `DllNotFoundException`，用户需自行反推平台/架构问题 |
| `ConvertAsync` 忽略 CancellationToken | `DinkToPdfConverter.cs` | **Low** | 参数收了但没用，长文档转换无法取消 |

**升级建议（优先级取决于是否要保留该组件）**：
1. **方案 A（推荐）**：考虑用纯托管方案替换（如 `QuestPDF`/`PdfSharp` + `HtmlRenderer`），彻底摆脱原生库
2. **方案 B**：保留 wkhtmltopdf，补 `DllImportResolver` 按平台加载 `libwkhtmltox.dll`/`.dylib`/`.so`，并在加载前做平台探测 + 明确异常
3. 若选 A，本包可标记为 `[Obsolete]` 并给出迁移指引

---

### 14. Findx.Configuration → 见 10 ｜ 15. Findx.NLog（4 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| `BeginScope` 返回 null | `NLogLogger.cs` | **Low** | 不支持日志作用域，`using (_logger.BeginScope(...))` 不产生上下文分组 |
| 开关路径为根级 `Logging:Enabled` | `NLogModule.cs` | **Low** | 与其他 `Findx:*` 配置节风格不一致，且缺省 false |

**升级建议**：低优先级。可考虑接入 `Microsoft.Extensions.Logging` 的 `ActivityTrackingOptions` 实现自动 scope。

---

### 16. Findx.Castle（6 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| 需手动 `MakeGenericType` + `CreateInterfaceProxyWithoutTarget` | 使用方 | **Medium** | 无自动注入拦截器到接口的语法糖，每个接口都要写 7 行注册代码 |
| `Aspect.md` 中 `Invocation.ReturnValue = true` 与接口返回类型不一定匹配 | `InterceptorBase` | **Low** | 文档示例强转风险 |

**升级建议**：提供 `[Intercept(typeof(XxxInterceptor))]` 接口特性 + 自动代理的 `AddIntercept<TInterface>()` 扩展，把注册样板降到一行。

---

### 17. Findx.Mapster（2 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| `MapperExtensions` 静态单例 | `Mapping/MapperExtensions.cs` | **Medium** | 静态字段保存 mapper，全局单例语义，单元测试无法隔离 |
| `TypeAdapterConfig.GlobalSettings` 全局可变 | `MapsterMapperModule.UseModule` | **Low** | 多模块共享 Mapster 全局配置 |

**升级建议**：`MapperExtensions` 静态入口保留（便利性），但内部改为从 DI 解析 + 提供 `IMapperAccessor` 供测试替换。

---

### 18. Findx.Guids.NewId（2 文件）

**已核实的问题**：无实质技术债，实现干净（2 文件、一行委托）。

**升级建议**：仅需在文档中明确「装此包即替换默认有序 Guid」的隐式行为。

---

### 19. Findx.IP（2 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| 离线库需定期更新 | `ip2region.xdb` | **Low** | 内置 xdb 是快照，长期不更新导致新 IP 段定位不准 |
| `samples/Findx.WebHost.csproj` 引用路径错误 | 项目文件 | **Low** | 写的是 `..\..\Findx.IP\Findx.IP.csproj`，实际在 `framework/` 下 |

**升级建议**：低优先级。xdb 增加版本号并在启动时打日志提示。

---

### 20. Findx.MailKit（3 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| **TLS 证书校验被完全关闭** | `MailKitEmailSender.cs:55` 与 `:73` | **High** | `client.ServerCertificateValidationCallback = (s,c,h,e) => true;` 两处（单发 + 批量）。任何中间人可伪造 SMTP 服务器窃取账号密码 |
| 配置项名为 `UserName` | `EmailSenderOptions.cs` | **Low** | 与常见 `Username`/`User` 拼写不同，易配错 |

**升级建议**：
1. **立即移除证书回调**，或改为可配置（`Findx:Email:SkipCertificateValidation`，默认 `false`）
2. 支持 OAuth2 / 客户端证书等现代 SMTP 认证方式

---

### 21. Findx.RulesEngine（3 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| 规则仅存内存 | `RulesEngineClient.cs` | **Medium** | 应用重启丢失，生产环境需自行在启动时从 DB 加载并注册，无内置持久化 |
| 返回抽象 `Response` 拿不到 Data | `IRulesEngineClient.ExecuteAsync` | **Medium** | 业务结果需强制转型 `OkResponse<IEnumerable<RuleResult>>`，API 设计不友好 |
| `SuccessEvent` 是字符串事件名 | `RuleResult` | **Low** | 无事件订阅机制，该字段实际无法生效 |

**升级建议**：提供 `IWorkflowStore` 扩展点 + 泛型返回 `Task<Response<IEnumerable<RuleResult>>>`（保留旧方法 `[Obsolete]`）。

---

### 22. Findx.WebApiClient（6 文件）

**已核实的问题**：

| 问题 | 位置 | 严重度 | 说明 |
|---|---|---|---|
| Polly 策略实现与 AspNetCore 完全重复 | `Extensions.HttpClientBuilder.cs`（internal）vs `Findx.AspNetCore/Extensions/Extensions.HttpClientBuilder.cs`（public） | **Medium** | 两份约 200 行代码，签名一致（`AddFallbackPolicy` 两个重载行号对应 118/161 vs 116/152）。**修复策略 bug 必须改两处**，已出现漂移风险 |
| 「配置优先」三态语义无文档约束 | `WebApiClientModule.cs` | **Low** | 配置值 `<= 0` 表示「显式关闭」很反直觉 |

**升级建议（收益明确）**：把策略实现抽到 `Findx` 核心包或独立 `Findx.HttpClient` 包，`AspNetCore` 与 `WebApiClient` 共用，内部版改为 `public` 并删除重复实现。这是本轮**最值得优先做的重构**——一次性消除 200 行重复。

---

### 23. Findx.Net（0 文件）

**已核实的问题**：空壳包（仅 csproj），`RootNamespace=Findx.RPC`，目标框架 `net6.0;net8.0;net9.0` **缺 net10.0**，与其余 26 个包不一致。

**升级建议**：二选一——要么补齐 RPC 实现（`IRpcService` / `RpcServiceRouteAttribute` 已在核心包定义但无消费方），要么**从 solution 中移除**并释放包名。当前状态会误导使用者以为存在 RPC 能力。

---

## 三、跨包横切问题（影响多个包）

| 问题 | 涉及包 | 严重度 | 建议 |
|---|---|---|---|
| **`Enabled` 开关模式不一致** | Redis / Discovery / RabbitMQ（读原始 `GetValue<bool>`，缺省 false）、FreeSql / Security / Swagger（读 Options 绑定，缺省 true） | **Medium** | 统一为一种：建议全部读 Options 并给显式默认值，同时在文档统一说明 |
| **Polly 策略重复实现** | AspNetCore（public）/ WebApiClient（internal） | **Medium** | 抽公共包 |
| **模块开关分散难排查** | 全部包 | **Medium** | 增加 `--Diagnose` 模式或 `StartupLogger` 输出「哪些模块被启用/跳过及原因」 |
| **NuGet 版本号硬编码** | 26 处 | **Medium** | 引入 `Directory.Build.props` + `MinVer`/`Nerdbank.GitVersioning` |
| **无任何自动化验证** | 全仓 | **High** | 见下阶段计划 |

---

## 四、建议的分阶段执行顺序

> 排序原则：**安全漏洞 → 会导致线上事故的缺陷 → 基础设施 → 体验优化**。
> 每阶段独立可交付，不做完也能停。

### 阶段 0：止血（建议 1 周内完成，S 级改动为主）

| # | 事项 | 包 | 类型 |
|---|---|---|---|
| 1 | `GetClientIp` 改为可信代理列表，停止无条件信任 XFF | AspNetCore | 安全 |
| 2 | 移除 `AesEncrypt/AesDecrypt` 的 `key = AesKey` 默认参数值 | Findx | 安全 |
| 3 | 移除 MailKit 两处 `ServerCertificateValidationCallback => true` | MailKit | 安全 |
| 4 | WebSocket 增加 Origin 白名单校验 | WebSocketCore | 安全 |
| 5 | `NullSmsSender` 改为抛异常，杜绝「假成功」 | Findx | 正确性 |
| 6 | 修 `SettingController.HistoryAsync` 的 `model != null` 条件反转 NRE | ConfigurationServer | 正确性 |
| 7 | 清理根目录残留 `Findx.IP/` 与 `.DS_Store` | 仓库 | 卫生 |

### 阶段 1：基础设施（建议 2-4 周，L 级投入但一次投入长期受益）

| # | 事项 | 说明 |
|---|---|---|
| 1 | **建 CI**：GitHub Actions，矩阵构建 26 个包的 3 个 TFM | 无 CI 是一切质量问题的根源 |
| 2 | **建测试项目**：优先覆盖 `EncryptUtility`、`CronUtility`、`ConsistentHash`、`LambdaExpressionParser` 等纯函数（无外部依赖，易测） | 零测试 → 有测试 |
| 3 | **统一版本管理**：引入 `Directory.Build.props` + `Directory.Packages.props`，版本号集中到一处 | 发版不再逐文件改 |
| 4 | **引入 `MinVer`/Nerdbank**：版本号从 git tag 自动推导，替代硬编码日期串 | 当前 `11.1.2026.810` 需手改 |
| 5 | **统一 TargetFrameworks**：全部收敛到 `net8.0;net10.0`（评估是否保留 net6.0，它已 EOL） | 减少条件编译分支 |

### 阶段 2：安全与权限深化（建议 3-5 周）

| # | 事项 | 包 |
|---|---|---|
| 1 | 实现 `AuthorizeAuthorityLimit` 默认实现，权限维度真正生效 | Security |
| 2 | 配置中心签名改 HMAC-SHA256 + 时间戳 + nonce；服务端加鉴权 | Configuration |
| 3 | WebSocket 身份改为只信任已认证主体，query 传身份改为默认关闭 | WebSocketCore |
| 4 | 审计补齐实体变更前值（`OriginalValue`），`AuditEntity` 空实现落地 | FreeSql |
| 5 | RabbitMQ 异常消息策略可配置（至少支持进死信而非静默 Ack 丢弃） | RabbitMQ |

### 阶段 3：重复与一致性收敛（建议 2-3 周）

| # | 事项 | 涉及包 |
|---|---|---|
| 1 | **抽 `Findx.HttpClient` 公共包**，消除 200 行 Polly 重复 | AspNetCore + WebApiClient |
| 2 | 统一 `Enabled` 开关模式 | Redis / Discovery / RabbitMQ / FreeSql / Security |
| 3 | 统一同步/异步接口签名（`ZRangeByScore` 等） | Redis |
| 4 | `ServiceLocator` 划定边界，业务代码改注入 | 全仓 |
| 5 | 修 `AddLeastConnectLoadBalancer` 实际走 RoundRobin | Discovery |

### 阶段 4：功能完善与文档（持续进行）

| # | 事项 | 包 |
|---|---|---|
| 1 | `Findx.Net` 二选一：补齐 RPC 实现或从 solution 移除 | Findx.Net |
| 2 | DinkToPdf 跨平台方案（换纯托管 或 补 DllImportResolver） | DinkToPdf |
| 3 | ImageSharp 内置 CJK 字体，修 `ResizeMode.Default` | ImageSharp |
| 4 | 补 `ISmsSender` 实现（当前无内置短信组件） | Findx |
| 5 | 规则引擎加 `IWorkflowStore` + 友好返回类型 | RulesEngine |
| 6 | Castle 加 `[Intercept]` 特性，把注册样板降到一行 | Castle |
| 7 | README 启动示例同步（`MapWebSocketManager` → `MapWebSocket` 等 6 处不一致） | 仓库 |

---

## 五、关于发版节奏的建议

当前版本号形如 `11.1.2026.810`（主版本.次版本.年份.月日），每月可能有多次发版，但**无 CI、无测试、无 changelog 生成**，每次发版全靠人工验证。

建议在阶段 1 完成后建立：

- **语义化版本**：区分 breaking（删默认参数、改接口签名）/ feature / fix，对应主次版本号递增
- **自动 CHANGELOG**：从 commit message 生成
- **Breaking Changes 清单**：由于 API 稳定性是框架的核心价值，建议在每次 breaking 变更时同步更新本文档「逐包分析」中的问题清单，让技术债有台账

---

## 六、明确不建议做的事

| 事项 | 原因 |
|---|---|
| 一次性重写或大重构 | 890+ 源文件、零测试覆盖，任何大重构都不可验证 |
| 立即引入 net9.0 / net11.0 多目标 | net9.0 已 EOL，且会让条件编译分支进一步膨胀；先收敛已有分叉 |
| 为了「现代化」替换掉所有第三方库 | `StackExchange.Redis`、`RabbitMQ.Client` 等选择合理；`FreeSql` 若要换需先提供等价的仓储/工作单元抽象迁移方案 |
| 强行补齐 `Findx.Net` 的 RPC 实现 | `IRpcService` 目前无消费方，先确认是否有真实需求再投入 |

---

## 附：核实说明

本计划所有问题均经源码逐条复核。审计过程中**排除**的两项：

1. 「modules 层密码使用无盐 MD5」—— 在 `modules/` 下未找到任何密码哈希调用（`Md5By32`/`Md5` 与 password 关联的调用为零），无法证实，不纳入计划
2. 「ConfigurationServer 配置接口无鉴权」—— 需结合部署场景判断是否为设计意图（配置中心可能在内网），列为「需确认」而非确认缺陷

[`../docs/`](../docs/) 下已有的 22 个包介绍文档可作为 API 参考，本文档只聚焦**待改进项**。
