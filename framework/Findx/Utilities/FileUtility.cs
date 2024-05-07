using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using Findx.Common;

namespace Findx.Utilities;

/// <summary>
///     文件辅助操作类
/// </summary>
public static class FileUtility
{
    /// <summary>
    ///     文件是否存在
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool Exists(string fileName)
    {
        return File.Exists(fileName);
    }

    /// <summary>
    ///     创建文件，如果文件不存在
    /// </summary>
    /// <param name="fileName">要创建的文件</param>
    public static void CreateIfNotExists(string fileName)
    {
        fileName.ThrowIfNull();
        
        if (File.Exists(fileName)) return;

        var dir = Path.GetDirectoryName(fileName);
        if (dir != null) DirectoryUtility.CreateIfNotExists(dir);
        // ReSharper disable once AssignNullToNotNullAttribute
        File.Create(fileName);
    }

    /// <summary>
    ///     删除指定文件
    /// </summary>
    /// <param name="fileName">要删除的文件名</param>
    public static void DeleteIfExists(string fileName)
    {
        if (!File.Exists(fileName)) return;
        File.Delete(fileName);
    }

    /// <summary>
    ///     复制指定文件
    /// </summary>
    /// <param name="sourceFileName"> 源文件名 </param>
    /// <param name="destFileName"> 目标文件名 </param>
    /// <param name="overwrite"> 是否覆盖 </param>
    public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
    {
        if (!File.Exists(sourceFileName)) return;
        File.Copy(sourceFileName, destFileName, overwrite);
    }

    /// <summary>
    ///     移动文件
    /// </summary>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="destFileName">目标文件名</param>
    public static void Move(string sourceFileName, string destFileName)
    {
        if (!File.Exists(sourceFileName)) return;
        File.Move(sourceFileName, destFileName);
    }

    /// <summary>
    ///     设置或取消文件的指定<see cref="FileAttributes" />属性
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="attribute">要设置的文件属性</param>
    /// <param name="isSet">true为设置，false为取消</param>
    public static void SetAttribute(string fileName, FileAttributes attribute, bool isSet)
    {
        var fi = new FileInfo(fileName);
        if (!fi.Exists) throw new FileNotFoundException("要设置属性的文件不存在。", fileName);
        if (isSet)
            fi.Attributes = fi.Attributes | attribute;
        else
            fi.Attributes = fi.Attributes & ~attribute;
    }

    /// <summary>
    ///     获取文件大小
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static long GetLength(string path)
    {
        if (!Exists(path))
            return -1;

        return new FileInfo(path).Length;
    }

    /// <summary>
    ///     获取文件创建时间
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DateTime GetCreationTime(string path)
    {
        return File.GetCreationTime(path);
    }

    /// <summary>
    ///     获取最后访问时间
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DateTime GetLastAccessTime(string path)
    {
        return File.GetLastAccessTime(path);
    }

    /// <summary>
    ///     获取最后修改时间
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DateTime GetLastWriteTime(string path)
    {
        return File.GetLastWriteTime(path);
    }

    /// <summary>
    ///     获取文件版本号
    /// </summary>
    /// <param name="fileName"> 完整文件名 </param>
    /// <returns> 文件版本号 </returns>
    public static string GetVersion(string fileName)
    {
        if (File.Exists(fileName))
        {
            var fvi = FileVersionInfo.GetVersionInfo(fileName);
            return fvi.FileVersion;
        }
        return null;
    }

    /// <summary>
    ///     获取文件的MD5值
    /// </summary>
    /// <param name="fileName"> 文件名 </param>
    /// <returns> 32位MD5 </returns>
    public static string GetFileMd5(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        const int bufferSize = 1024 * 1024;
        var buffer = new byte[bufferSize];
        using var md5 = MD5.Create();
        md5.Initialize();
        long offset = 0;
        while (offset < fs.Length)
        {
            long readSize = bufferSize;
            if (offset + readSize > fs.Length)
            {
                readSize = fs.Length - offset;
            }

            // ReSharper disable once MustUseReturnValue
            fs.Read(buffer, 0, (int)readSize);
            if (offset + readSize < fs.Length)
            {
                md5.TransformBlock(buffer, 0, (int)readSize, buffer, 0);
            }
            else
            {
                md5.TransformFinalBlock(buffer, 0, (int)readSize);
            }

            offset += bufferSize;
        }

        fs.Close();
        var result = md5.Hash;
        if (result == null)
        {
            return null;
        }
        md5.Clear();

        return Convert.ToHexString(result);
    }

    /// <summary>
    ///     获取文本文件的编码方式
    /// </summary>
    /// <param name="fileName"> 文件名 例如：path = @"D:\test.txt"</param>
    /// <returns>返回编码方式</returns>
    public static Encoding GetEncoding(string fileName)
    {
        return GetEncoding(fileName, Encoding.Default);
    }

    /// <summary>
    ///     获取文本流的编码方式
    /// </summary>
    /// <param name="fs">文本流</param>
    /// <returns>返回系统默认的编码方式</returns>
    public static Encoding GetEncoding(FileStream fs)
    {
        // Encoding.Default 系统默认的编码方式
        return GetEncoding(fs, Encoding.Default);
    }

    /// <summary>
    ///     获取一个文本流的编码方式
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(string fileName, Encoding defaultEncoding)
    {
        using var fs = File.Open(fileName, FileMode.Open);
        return GetEncoding(fs, defaultEncoding);
    }

    /// <summary>
    ///     获取一个文本流的编码方式
    /// </summary>
    /// <param name="fs">文本流</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(FileStream fs, Encoding defaultEncoding)
    {
        var targetEncoding = defaultEncoding;
        if (fs is { Length: >= 2 })
        {
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            byte b4 = 0;

            // ReSharper disable once UnusedVariable
            var originalPostion = fs.Seek(0, SeekOrigin.Begin);
            fs.Seek(0, SeekOrigin.Begin);

            b1 = Convert.ToByte(fs.ReadByte());
            b2 = Convert.ToByte(fs.ReadByte());
            if (fs.Length > 2)
            {
                b3 = Convert.ToByte(fs.ReadByte());
            }
            if (fs.Length > 3)
            {
                b4 = Convert.ToByte(fs.ReadByte());
            }

            // 根据文件流的前4个字节判断Encoding
            // Unicode {0xFF, 0xFE};
            // BE-Unicode {0xFE, 0xFF};
            // UTF8 = {0xEF, 0xBB, 0xBF};
            if (b1 == 0xFE && b2 == 0xFF) // UnicodeBe
            {
                targetEncoding = Encoding.BigEndianUnicode;
            }
            if (b1 == 0xFF && b2 == 0xFE && b3 != 0xFF) // Unicode
            {
                targetEncoding = Encoding.Unicode;
            }
            if (b1 == 0xEF && b2 == 0xBB && b3 == 0xBF) // UTF8
            {
                targetEncoding = Encoding.UTF8;
            }

            fs.Seek(0, SeekOrigin.Begin);
        }

        return targetEncoding;
    }

    /// <summary>
    ///     是否为图片文件
    /// </summary>
    /// <param name="fileExt">文件扩展名，不含“.”</param>
    public static bool IsImage(string fileExt)
    {
        var al = new ArrayList
        {
            "bmp",
            "jpeg",
            "jpg",
            "gif",
            "png"
        };
        return al.Contains(fileExt.ToLower());
    }

    /// <summary>
    ///     检查文件地址是否文件服务器地址
    /// </summary>
    /// <param name="url">文件地址</param>
    public static bool IsExternalIpAddress(string url)
    {
        var uri = new Uri(url);
        switch (uri.HostNameType)
        {
            case UriHostNameType.Dns:
                var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                foreach (var ipAddress in ipHostEntry.AddressList)
                {
                    if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (!NetUtility.IsInternalIp(ipAddress))
                        return true;
                }

                break;

            case UriHostNameType.IPv4:
                return !NetUtility.IsInternalIp(IPAddress.Parse(uri.DnsSafeHost));
        }

        return false;
    }
}