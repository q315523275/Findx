using Findx.Setting;

namespace Findx.UnitTests;

/// <summary>
///     DefaultSettingProvider 回归测试（修复：provider 缺失/值为 null 时向下游兜底，不再 NRE）
/// </summary>
public class DefaultSettingProviderTests
{
    private sealed class FakeProvider : ISettingValueProvider
    {
        private readonly string _key;
        private readonly object _value;

        public FakeProvider(string key, object value)
        {
            _key = key;
            _value = value;
        }

        public string Name => "fake";
        public int Order => 1;
        public bool ContainsKey(string key) => string.Equals(key, _key);
        public T GetValue<T>(string key) => (T)_value;
        public T GetObject<T>(string key) where T : new() => (T)_value;
    }

    private sealed class SettingDto
    {
        public string Value { get; set; }
    }

    [Fact]
    public void GetObject_NoProvider_ReturnsDefault()
    {
        var provider = new DefaultSettingProvider(Array.Empty<ISettingValueProvider>());

        // 修复前：空提供器时枚举/取值路径 NRE；修复后：返回 default 兜底
        Assert.Null(provider.GetObject<SettingDto>("missing"));
    }

    [Fact]
    public void GetObject_Hit_ReturnsValue()
    {
        var provider = new DefaultSettingProvider(new ISettingValueProvider[] { new FakeProvider("k", new SettingDto { Value = "v" }) });

        Assert.Equal("v", provider.GetObject<SettingDto>("k").Value);
    }

    [Fact]
    public void GetObject_Miss_ReturnsDefault()
    {
        var provider = new DefaultSettingProvider(new ISettingValueProvider[] { new FakeProvider("k", new SettingDto { Value = "v" }) });

        Assert.Null(provider.GetObject<SettingDto>("missing"));
    }

    [Fact]
    public void GetValue_Hit_ReturnsValue()
    {
        var provider = new DefaultSettingProvider(new ISettingValueProvider[] { new FakeProvider("k", "v") });

        Assert.Equal("v", provider.GetValue<string>("k"));
    }
}
