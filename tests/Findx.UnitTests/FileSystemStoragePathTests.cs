using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Findx.Storage;

namespace Findx.UnitTests;

/// <summary>
///     FileSystemStorage 路径限定回归测试：
///     默认严格模式限定在存储根目录内（拒绝 ../ 越界与绝对路径），AllowOutsideRoot=true 时放开
/// </summary>
public class FileSystemStoragePathTests : IDisposable
{
    private readonly string _root;
    private readonly string _outside;

    public FileSystemStoragePathTests()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "findx-storage-test-" + Guid.NewGuid().ToString("N"));
        _root = Path.Combine(baseDir, "root");
        _outside = Path.Combine(baseDir, "outside.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(_outside, "outside");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path.GetDirectoryName(_root)!, true);
        }
        catch
        {
            //  清理失败不影响测试结论
        }
    }

    private FileSystemStorage CreateStorage(bool allowOutsideRoot = false)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Findx:Storage:Folder:DefaultFolder"] = _root,
                ["Findx:Storage:Folder:AllowOutsideRoot"] = allowOutsideRoot.ToString()
            })
            .Build();

        //  配置恒定提供 DefaultFolder，app/serializer 在路径校验路径上不会使用
        return new FileSystemStorage(null, null, NullLogger<FileSystemStorage>.Instance, config);
    }

    // ---------- 严格模式（默认） ----------

    [Fact]
    public async Task ExistsAsync_ParentTraversal_Throws()
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.ExistsAsync("../outside.txt"));
    }

    [Fact]
    public async Task ExistsAsync_AbsolutePath_Throws()
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.ExistsAsync(_outside));
    }

    [Fact]
    public async Task ExistsAsync_InsideRoot_NotThrows()
    {
        var storage = CreateStorage();
        Assert.False(await storage.ExistsAsync("sub/a.txt"));
    }

    [Fact]
    public async Task SaveFileAsync_InsideRoot_WritesUnderRoot()
    {
        var storage = CreateStorage();
        var ok = await storage.SaveFileAsync("sub/a.txt", new byte[] { 1, 2, 3 });

        Assert.True(ok);
        Assert.True(File.Exists(Path.Combine(_root, "sub", "a.txt")));
    }

    [Fact]
    public async Task SaveFileAsync_ParentTraversal_Throws()
    {
        var storage = CreateStorage();
        //  ResolvePath 位于 try 之外，越界直接抛而非静默返回 false
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            storage.SaveFileAsync("../escape.txt", new byte[] { 9 }));
        Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(_root)!, "escape.txt")));
    }

    [Fact]
    public async Task DeleteFilesAsync_ParentTraversal_Throws()
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.DeleteFilesAsync("../*"));
    }

    [Fact]
    public async Task GetFileStreamAsync_AbsolutePath_Throws()
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.GetFileStreamAsync(_outside));
    }

    // ---------- 显式放开 ----------

    [Fact]
    public async Task AllowOutsideRoot_AbsolutePathAccessible()
    {
        var storage = CreateStorage(allowOutsideRoot: true);
        Assert.True(await storage.ExistsAsync(_outside));
    }

    [Fact]
    public async Task AllowOutsideRoot_ParentTraversalAccessible()
    {
        var storage = CreateStorage(allowOutsideRoot: true);
        Assert.True(await storage.ExistsAsync("../outside.txt"));
    }

    // ---------- 边界：前缀误判 ----------

    [Fact]
    public async Task SiblingDirectoryWithSamePrefix_IsRejected()
    {
        //  root 的兄弟目录 root2/ 不得因字符串前缀相似被放行
        var siblingDir = _root + "2";
        Directory.CreateDirectory(siblingDir);
        File.WriteAllText(Path.Combine(siblingDir, "f.txt"), "x");

        var storage = CreateStorage();
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.ExistsAsync("../root2/f.txt"));
    }
}
