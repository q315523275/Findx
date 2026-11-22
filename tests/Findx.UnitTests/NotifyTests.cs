using System.Security.Cryptography;
using System.Text;
using Findx.Extensions.Notify.Channels;
using Findx.Extensions.Notify.Entity;
using Findx.Extensions.Notify.Enums;
using Findx.Extensions.Notify.Models;
using Findx.Extensions.Notify.Options;
using Findx.Extensions.Notify.Scheduling;
using Findx.Extensions.Notify.ServiceDefaults;
using Findx.Extensions.Notify.Storage;
using Findx.Extensions.Notify.Support;
using Findx.NewId;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Findx.UnitTests;

/// <summary>
///     通知组件测试：钉钉加签、通道路由、广播、消息复制、配置默认值
/// </summary>
public class NotifyTests
{
    /// <summary>
    ///     测试用通道桩：记录调用次数与消息
    /// </summary>
    private sealed class FakeChannel : INotifyChannel
    {
        private readonly bool _success;

        public FakeChannel(NotifyChannel channel, bool success = true)
        {
            Channel = channel;
            _success = success;
        }

        public NotifyChannel Channel { get; }

        public int CallCount { get; private set; }

        public NotifyMessage LastMessage { get; private set; }

        public Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastMessage = message;
            return Task.FromResult(_success ? NotifyResult.Ok(Channel) : NotifyResult.Fail(Channel, "fake fail"));
        }
    }

    /// <summary>
    ///     测试用递增主键生成器：值可控可预测，比雪花更适合断言
    /// </summary>
    private sealed class SequentialKeyGenerator : IKeyGenerator<long>
    {
        private long _current;

        public long Create() => Interlocked.Increment(ref _current);
    }

    private static readonly IKeyGenerator<long> KeyGenerator = new SequentialKeyGenerator();

    private static NotifySender BuildSender(NotifyOptions options, params INotifyChannel[] channels)
    {
        var dispatcher = new NotifyChannelDispatcher(Options.Create(options), channels,
            NullLogger<NotifyChannelDispatcher>.Instance);
        var logStore = new NotifyLogStore(new EmptyServiceProvider(), KeyGenerator, NullLogger<NotifyLogStore>.Instance);
        var queue = new NotifyDispatchQueue(Options.Create(options), dispatcher, logStore,
            NullLogger<NotifyDispatchQueue>.Instance);
        return new NotifySender(Options.Create(options), dispatcher, queue, logStore, KeyGenerator,
            NullLogger<NotifySender>.Instance);
    }

    /// <summary>
    ///     构造带异步队列的发送器（测试里不 Start 后台服务，只验证入队行为）
    /// </summary>
    private static (NotifySender Sender, FakeChannel Channel, NotifyDispatchQueue Queue) BuildQueuedSender(NotifyOptions options)
    {
        var channel = new FakeChannel(NotifyChannel.WeCom);
        var dispatcher = new NotifyChannelDispatcher(Options.Create(options), new[] { channel },
            NullLogger<NotifyChannelDispatcher>.Instance);
        var logStore = new NotifyLogStore(new EmptyServiceProvider(), KeyGenerator, NullLogger<NotifyLogStore>.Instance);
        var queue = new NotifyDispatchQueue(Options.Create(options), dispatcher, logStore,
            NullLogger<NotifyDispatchQueue>.Instance);
        var sender = new NotifySender(Options.Create(options), dispatcher, queue, logStore, KeyGenerator,
            NullLogger<NotifySender>.Instance);
        return (sender, channel, queue);
    }

    /// <summary>
    ///     空服务提供者：日志落库会因取不到仓储而失败，被 NotifyLogStore 内部吞掉（测试用）
    /// </summary>
    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }

    /// <summary>
    ///     固定返回新 HttpClient 的工厂（订阅消息通道参数校验用，不会真的发请求）
    /// </summary>
    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    [Fact]
    public void MiniProgram_Limits_ShouldMatchWeChatRules()
    {
        Assert.Equal(20, MiniProgramDataRules.GetLimit("thing1"));
        Assert.Equal(20, MiniProgramDataRules.GetLimit("thing12"));
        Assert.Equal(5, MiniProgramDataRules.GetLimit("phrase01"));
        Assert.Equal(10, MiniProgramDataRules.GetLimit("name2"));
        Assert.Equal(32, MiniProgramDataRules.GetLimit("character_string3"));
        Assert.Equal(12, MiniProgramDataRules.GetLimit("amount1"));
        //  时间/日期类不限制长度，未知类型也不处理
        Assert.Equal(0, MiniProgramDataRules.GetLimit("time1"));
        Assert.Equal(0, MiniProgramDataRules.GetLimit("date2"));
        Assert.Equal(0, MiniProgramDataRules.GetLimit("unknown1"));
    }

    [Fact]
    public void MiniProgram_Truncate_ShouldCutByFieldType()
    {
        var thing = new string('警', 22);
        var cut = MiniProgramDataRules.Truncate("thing1", thing, out var truncated);
        Assert.True(truncated);
        Assert.Equal(20, cut.Length);

        var phrase = new string('审', 8);
        Assert.Equal(5, MiniProgramDataRules.Truncate("phrase1", phrase, out _).Length);

        //  不限制类型的字段原样返回
        var raw = new string('a', 40);
        Assert.Equal(raw, MiniProgramDataRules.Truncate("date1", raw, out var notCut));
        Assert.False(notCut);

        //  未超长不截断
        Assert.Equal("审核通过", MiniProgramDataRules.Truncate("thing1", "审核通过", out var noCut));
        Assert.False(noCut);
    }

    [Fact]
    public void MiniProgram_Truncate_ShouldKeepSurrogatePairIntact()
    {
        //  19 个汉字 + emoji（emoji 占 2 个 char）总长 21，限制 20 时应截到 19 而不是留下半个代理对
        var value = new string('警', 19) + "😀";

        var cut = MiniProgramDataRules.Truncate("thing1", value, out var truncated);

        Assert.True(truncated);
        Assert.Equal(19, cut.Length);
        Assert.False(char.IsHighSurrogate(cut[^1]));
    }

    [Fact]
    public async Task MiniProgramSender_ShouldFail_WhenCredentialMissing()
    {
        var options = new NotifyOptions { LogEnabled = false };
        var channel = new WeChatMiniProgramNotifyChannel(Options.Create(options), new FakeHttpClientFactory(),
            NullLogger<WeChatMiniProgramNotifyChannel>.Instance);

        var result = await channel.SendAsync(NotifyMessage.Create(NotifyChannel.WeChatMiniProgram, "标题", "正文"));

        Assert.False(result.Success);
        Assert.Contains("未配置小程序参数", result.Message);
    }

    [Fact]
    public async Task MiniProgramSender_ShouldFail_WhenTemplateMissing()
    {
        var options = new NotifyOptions { LogEnabled = false };
        options.WeChatMiniProgram = new WeChatMiniProgramOptions { AppId = "wx-test", AppSecret = "secret" };
        var channel = new WeChatMiniProgramNotifyChannel(Options.Create(options), new FakeHttpClientFactory(),
            NullLogger<WeChatMiniProgramNotifyChannel>.Instance);

        var message = new NotifyMessage
        {
            Channel = NotifyChannel.WeChatMiniProgram,
            To = new[] { "openid-1" },
            Data = new Dictionary<string, object> { ["thing1"] = "已发货" }
        };
        var result = await channel.SendAsync(message);

        Assert.False(result.Success);
        Assert.Contains("未指定模板", result.Message);
    }

    [Fact]
    public async Task MiniProgramSender_ShouldFail_WhenReceiverMissing()
    {
        var options = new NotifyOptions { LogEnabled = false };
        options.WeChatMiniProgram = new WeChatMiniProgramOptions
        {
            AppId = "wx-test",
            AppSecret = "secret",
            TemplateId = "tpl"
        };
        var channel = new WeChatMiniProgramNotifyChannel(Options.Create(options), new FakeHttpClientFactory(),
            NullLogger<WeChatMiniProgramNotifyChannel>.Instance);

        var result = await channel.SendAsync(NotifyMessage.Create(NotifyChannel.WeChatMiniProgram, "标题", "正文"));

        Assert.False(result.Success);
        Assert.Contains("接收人不能为空", result.Message);
    }

    [Fact]
    public void Clone_ShouldKeepBusinessKeyAndPage()
    {
        var message = new NotifyMessage
        {
            Channel = NotifyChannel.WeChatMiniProgram,
            Page = "pages/order/detail?id=1",
            BusinessKey = "SO20261114001",
            To = new[] { "openid-1" }
        };

        var copy = message.Clone();

        Assert.Equal("pages/order/detail?id=1", copy.Page);
        Assert.Equal("SO20261114001", copy.BusinessKey);
        Assert.Equal(NotifyChannel.WeChatMiniProgram, copy.Channel);
    }

    [Fact]
    public void DingTalkSign_ShouldMatchHmacSha256_AndBeUrlEncoded()
    {
        const long timestamp = 1700000000000L;
        const string secret = "SEC_test_secret";

        var sign = DingTalkNotifyChannel.ComputeSign(timestamp, secret);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = Uri.EscapeDataString(
            Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}\n{secret}"))));

        Assert.Equal(expected, sign);
        //  URL 编码后不应残留 base64 的裸字符
        Assert.DoesNotContain("+", sign);
        Assert.DoesNotContain("/", sign);
        Assert.DoesNotContain("=", sign);
    }

    [Fact]
    public async Task Sender_ShouldFail_WhenChannelNotRegistered()
    {
        var sender = BuildSender(new NotifyOptions());

        var result = await sender.SendAsync(NotifyMessage.Create(NotifyChannel.WeCom, "标题", "正文"));

        Assert.False(result.Success);
        Assert.Contains("未启用", result.Message);
    }

    [Fact]
    public async Task Sender_ShouldFail_WhenComponentDisabled()
    {
        var options = new NotifyOptions { Enabled = false };
        var sender = BuildSender(options, new FakeChannel(NotifyChannel.WeCom));

        var result = await sender.SendAsync(NotifyMessage.Create(NotifyChannel.WeCom, "标题", "正文"));

        Assert.False(result.Success);
        Assert.Contains("未启用", result.Message);
    }

    [Fact]
    public async Task Sender_Broadcast_ShouldHitEachChannelOnce()
    {
        var weCom = new FakeChannel(NotifyChannel.WeCom);
        var ding = new FakeChannel(NotifyChannel.DingTalk);
        var sender = BuildSender(new NotifyOptions(), weCom, ding);

        var results = await sender.SendAsync(
            new[] { NotifyChannel.WeCom, NotifyChannel.DingTalk, NotifyChannel.WeCom },   //  重复通道应去重
            NotifyMessage.Create(NotifyChannel.WeCom, "标题", "正文"));

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
        Assert.Equal(1, weCom.CallCount);
        Assert.Equal(1, ding.CallCount);
        //  每个通道收到的是自己的通道标识，原消息未被改写
        Assert.Equal(NotifyChannel.DingTalk, ding.LastMessage.Channel);
    }

    [Fact]
    public async Task Sender_Broadcast_ShouldNotAffectOthers_WhenOneChannelFails()
    {
        var ok = new FakeChannel(NotifyChannel.WeCom);
        var fail = new FakeChannel(NotifyChannel.DingTalk, false);
        var sender = BuildSender(new NotifyOptions(), ok, fail);

        var results = await sender.SendAsync(new[] { NotifyChannel.WeCom, NotifyChannel.DingTalk },
            NotifyMessage.Create(NotifyChannel.WeCom, "标题", "正文"));

        Assert.Equal(2, results.Count);
        Assert.True(results.Single(x => x.Channel == NotifyChannel.WeCom).Success);
        Assert.False(results.Single(x => x.Channel == NotifyChannel.DingTalk).Success);
    }

    [Fact]
    public void Message_Clone_ShouldKeepAllFields()
    {
        var message = new NotifyMessage
        {
            Channel = NotifyChannel.WeChat,
            To = new[] { "openid-1" },
            Title = "标题",
            Content = "正文",
            IsMarkdown = true,
            TemplateId = "tpl",
            Data = new Dictionary<string, object> { ["k"] = "v" },
            Url = "https://example.com",
            AtMobiles = new[] { "13800000000" },
            AtAll = true
        };

        var clone = message.Clone();

        Assert.NotSame(message, clone);
        Assert.Equal(message.Channel, clone.Channel);
        Assert.Equal(message.To, clone.To);
        Assert.Equal(message.Title, clone.Title);
        Assert.Equal(message.Content, clone.Content);
        Assert.Equal(message.IsMarkdown, clone.IsMarkdown);
        Assert.Equal(message.TemplateId, clone.TemplateId);
        Assert.Equal(message.Url, clone.Url);
        Assert.Equal(message.AtMobiles, clone.AtMobiles);
        Assert.Equal(message.AtAll, clone.AtAll);
        Assert.Equal("v", clone.Data["k"]);
    }

    [Fact]
    public void Options_Defaults_ShouldBeSafe()
    {
        var options = new NotifyOptions();

        Assert.True(options.Enabled);
        Assert.Equal(10, options.TimeoutSeconds);
        //  通道默认关闭：未显式开启不注册，避免漏配参数时"已启用但发不出去"
        Assert.False(options.WeCom.Enabled);
        Assert.False(options.DingTalk.Enabled);
        Assert.False(options.Email.Enabled);
        Assert.False(options.WeChat.Enabled);
        Assert.True(options.Email.IsBodyHtml);
    }

    [Fact]
    public async Task QueueAsync_ShouldOnlyEnqueue_WithoutSendingOnCaller()
    {
        var options = new NotifyOptions { LogEnabled = false, QueueCapacity = 8 };
        var (sender, channel, _) = BuildQueuedSender(options);

        var logId = await sender.QueueAsync(NotifyMessage.Create(NotifyChannel.WeCom, "标题", "正文"));

        Assert.True(logId > 0);
        //  后台消费未启动，通道一次都没被调用 —— 证明业务线程没有同步发送
        Assert.Equal(0, channel.CallCount);
    }

    [Fact]
    public async Task QueueAsync_ShouldFallbackToSync_WhenQueueFull()
    {
        var options = new NotifyOptions { LogEnabled = false, QueueCapacity = 1 };
        var (sender, channel, queue) = BuildQueuedSender(options);

        //  先把容量为 1 的队列塞满
        Assert.True(queue.TryEnqueue(NotifyLog.From(NotifyMessage.Create(NotifyChannel.WeCom, "占位", "先塞满"), null, KeyGenerator.Create())));

        var logId = await sender.QueueAsync(NotifyMessage.Create(NotifyChannel.WeCom, "标题", "正文"));

        Assert.True(logId > 0);
        //  队列满时降级同步发送，消息不丢
        Assert.Equal(1, channel.CallCount);
    }

    [Fact]
    public void NotifyLog_ShouldRoundTripMessage()
    {
        var message = new NotifyMessage
        {
            Channel = NotifyChannel.WeChat,
            To = new[] { "openid-1" },
            Title = "标题",
            Content = "正文",
            TemplateId = "tpl",
            Url = "https://example.com",
            Data = new Dictionary<string, object> { ["k"] = "v" },
            BusinessKey = "SO20261112001"
        };

        var log = NotifyLog.From(message, "node-1", KeyGenerator.Create());

        Assert.True(log.Id > 0);
        Assert.Equal("openid-1", log.Receivers);
        Assert.Equal("SO20261112001", log.BusinessKey);
        Assert.Equal("node-1", log.NodeName);
        Assert.Equal(NotifyLogStatus.Queued, log.Status);

        var restored = log.ToMessage();
        Assert.Equal(NotifyChannel.WeChat, restored.Channel);
        Assert.Equal("openid-1", restored.To[0]);
        Assert.Equal("tpl", restored.TemplateId);
        Assert.Equal("https://example.com", restored.Url);
        Assert.Equal("SO20261112001", restored.BusinessKey);
    }

    [Fact]
    public void NotifyLog_Apply_ShouldSetStatus()
    {
        var log = NotifyLog.From(NotifyMessage.Create(NotifyChannel.DingTalk, "标题", "正文"), null, KeyGenerator.Create());

        log.Apply(NotifyResult.Ok(NotifyChannel.DingTalk), DateTime.Now, 12);
        Assert.True(log.Success);
        Assert.Equal(NotifyLogStatus.Success, log.Status);
        Assert.Equal(12, log.ElapsedMs);
        Assert.NotNull(log.SendTime);
        Assert.Null(log.ErrorMessage);

        log.Apply(NotifyResult.Fail(NotifyChannel.DingTalk, "接口超时"), DateTime.Now, 10);
        Assert.False(log.Success);
        Assert.Equal(NotifyLogStatus.Failed, log.Status);
        Assert.Equal("接口超时", log.ErrorMessage);
    }
}
