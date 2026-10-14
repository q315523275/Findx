# Findx.MailKit

MailKit 邮件发送器组件，基于 MailKit 实现邮件的发送功能，重点解决中文邮件编码问题。

> Nuget 包引用

```
Findx.MailKit
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `MailKit`（net6.0 为 4.8.0，net8.0/net10.0 为 4.17.0）。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 MailKitModule
```

`MailKitModule`（`ModuleLevel.Framework`，`Order = 70`）通过 `Replace` 覆盖核心包的默认实现：

```csharp
services.Replace(new ServiceDescriptor(
    typeof(IEmailSender), typeof(MailKitEmailSender), ServiceLifetime.Singleton));
```

> 为什么需要本包

核心包默认实现基于 `System.Net.Mail.SmtpClient`，该组件已被 .NET 标记为不推荐，且不支持 OAuth2、不支持 STARTTLS 精细控制、中文邮件编码处理不佳。生产环境建议引入本包。

## Json 配置

配置节为 **`Findx:Email`**，与核心包共用。

```json
{
    "Findx": {
        "Email": {
            "Host": "smtp.exmail.qq.com",
            "Port": 465,
            "UserName": "no-reply@example.com",
            "Password": "your-password",
            "EnableSsl": true,
            "FromDisplayName": "Findx 系统",
            "FromAddress": "no-reply@example.com"
        }
    }
}
```

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Host` | `string` | 无 | SMTP 主机 |
| `Port` | `int` | `25` | 端口 |
| `UserName` | `string` | 无 | 用户名 |
| `Password` | `string` | 无 | 密码 |
| `EnableSsl` | `bool` | `false` | 是否启用 SSL |
| `FromDisplayName` | `string` | 无 | 发件人显示名 |
| `FromAddress` | `string` | 无 | 发件地址 |

> 注意事项

键名是 `UserName`（大写 N），不是常见的 `Username` 或 `User`。

## 使用

> 简单用法

```csharp
[HttpGet("send")]
public async Task<string> EmailSend([FromServices] IEmailSender sender,
    [Required] string mailAddress, [Required] string subject, [Required] string body,
    CancellationToken cancellationToken)
{
    await sender.SendAsync(mailAddress, subject, body, false, cancellationToken);
    return "ok";
}
```

`isBodyHtml` 参数默认为 `true`，纯文本需显式传 `false`。

> HTML 邮件与附件

```csharp
using var mail = new MailMessage
{
    Subject = "报表",
    Body = "<h1>周报</h1>",
    IsBodyHtml = true,
    HeadersEncoding = Encoding.UTF8,
    SubjectEncoding = Encoding.UTF8,
    BodyEncoding = Encoding.UTF8
};

mail.To.Add("a@example.com");
mail.CC.Add("b@example.com");

var stream = File.OpenRead("report.xlsx");
mail.Attachments.Add(new Attachment(stream, "报表.xlsx"));

await _emailSender.SendAsync(mail);
```

> 批量发送

```csharp
await _emailSender.SendAsync(new[] { mail1, mail2 });   // 复用一条 SMTP 连接
```

`normalize` 参数默认为 `true`，会自动补齐 `From` 并设置 UTF-8 编码。

## 实现要点

`MailKitEmailSender` 继承核心包的 `EmailSenderBase`，构造函数注入 `IOptionsMonitor<EmailSenderOptions>` 并订阅 `OnChange`，因此 **SMTP 配置支持热更新**（配合配置中心尤其实用）。

| 项 | 说明 |
|---|---|
| 消息转换 | 使用自定义 `ToMimeMessage()` 而非 `MimeMessage.CreateFromMailMessage` |
| 中文附件名 | 使用 `GB18030` 字符集写入 `name` / `filename` 参数，`Rfc2047` 编码 |
| 证书校验 | `ServerCertificateValidationCallback` **无条件返回 true**，即跳过服务端证书校验 |
| 批量发送 | 单条连接串行发送，逐封复用 |

## 注意事项

- **服务端证书校验被无条件放行**，内网 SMTP 场景可接受，公网场景建议继承 `MailKitEmailSender` 重写连接逻辑以启用证书校验。
- 缺少 `Host` 配置时不会在构造期报错，而是在实际发送时抛 `ArgumentNullException`。
- 单封发送有 Debug 日志，批量发送重载没有日志输出。