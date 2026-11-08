namespace Findx.Machine.Disk;

/// <summary>
///     磁盘信息
/// </summary>
public class DiskInfo
{
    private DiskInfo(DriveInfo info)
    {
        DriveInfo = info;
    }

    /// <summary>
    ///     获取磁盘类
    /// </summary>
    public DriveInfo DriveInfo { get; }

    /// <summary>
    ///     驱动器名称
    ///     <para>ex: C:\</para>
    /// </summary>
    public string Id => DriveInfo.Name;

    /// <summary>
    ///     磁盘名称
    ///     <para>
    ///         ex:<br />
    ///         Windows:    system<br />
    ///         Linux:  /dev
    ///     </para>
    /// </summary>
    public string Name => DriveInfo.Name;

    /// <summary>
    ///     获取驱动器类型
    /// </summary>
    /// <remarks>获取驱动器类型，如 CD-ROM、可移动、网络或固定</remarks>
    public DriveType DriveType => DriveInfo.DriveType;

    /// <summary>
    ///     文件系统
    ///     <para>
    ///         Windows: NTFS、 CDFS...<br />
    ///         Linux: rootfs、tmpfs、binfmt_misc...
    ///     </para>
    /// </summary>
    public string FileSystem => DriveInfo.DriveFormat;

    /// <summary>
    ///     磁盘剩余容量（以字节为单位）
    /// </summary>
    public long FreeSpace => DriveInfo.AvailableFreeSpace;

    /// <summary>
    ///     磁盘总容量（以字节为单位）
    /// </summary>
    public long TotalSize => DriveInfo.TotalSize;

    /// <summary>
    ///     磁盘剩余可用容量
    /// </summary>
    public long UsedSize => TotalSize - FreeSpace;

    /// <summary>
    ///     磁盘根目录位置
    /// </summary>
    public string RootPath => DriveInfo.RootDirectory.FullName;

    /// <summary>
    ///     获取本地所有磁盘信息
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<DiskInfo> GetDisks()
    {
        return DriveInfo.GetDrives().Select(x => new DiskInfo(x));
    }

    /// <summary>
    ///     获取 Docker 运行的容器其容器文件系统在主机中的存储位置
    /// </summary>
    /// <remarks>程序需要在宿主机运行才有效果，在容器中运行，调用此API获取不到相关信息</remarks>
    /// <returns></returns>
    public static IEnumerable<DiskInfo> GetDockerMerge()
    {
        //  DriveFormat 是文件系统名（如 overlay），不可能同时包含 docker；
        //  Docker 容器 overlay 挂载点的宿主机路径通常为 /var/lib/docker/...，
        //  故改为对驱动器挂载路径（Name）判断是否包含 docker
        return DriveInfo.GetDrives()
            .Where(x => x.IsReady &&
                        x.DriveFormat.Equals("overlay", StringComparison.OrdinalIgnoreCase) &&
                        x.Name.Contains("docker")).Select(x => new DiskInfo(x));
    }

    /// <summary>
    ///     筛选出真正能够使用的磁盘
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<DiskInfo> GetRealDisk()
    {
        //  未就绪驱动器（空光驱、未格式化分区）直接访问 TotalSize/DriveFormat 会抛 DriveNotFoundException，先判 IsReady
        var disks = DriveInfo.GetDrives()
            .Where(x => x.IsReady && x.DriveType == DriveType.Fixed && x.TotalSize != 0 && x.DriveFormat != "overlay");
        return disks.Select(x => new DiskInfo(x)).Distinct(new DiskInfoEquality());
    }

    /// <summary>
    ///     筛选重复项
    /// </summary>
    private class DiskInfoEquality : IEqualityComparer<DiskInfo>
    {
        public bool Equals(DiskInfo x, DiskInfo y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(DiskInfo obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}