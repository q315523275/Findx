﻿using Findx.Common;

namespace Findx.Utilities;

/// <summary>
///     目录辅助操作类
/// </summary>
public class DirectoryUtility
{
    /// <summary>
    ///     获取程序根目录
    /// </summary>
    public static string RootPath()
    {
        return Path.GetDirectoryName(typeof(DirectoryUtility).Assembly.Location);
    }

    /// <summary>
    ///     判断目录是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    ///     创建文件夹，如果不存在
    /// </summary>
    /// <param name="directory">要创建的文件夹路径</param>
    public static void CreateIfNotExists(string directory)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
    }

    /// <summary>
    ///     递归删除文件夹及文件
    /// </summary>
    /// <param name="directory"> 目录路径 </param>
    /// <param name="isDeleteRoot"> 是否删除根目录 </param>
    /// <returns> 是否成功 </returns>
    public static bool Delete(string directory, bool isDeleteRoot = true)
    {
        Check.NotNull(directory, nameof(directory));

        var dirPathInfo = new DirectoryInfo(directory);
        if (!dirPathInfo.Exists)
            return false;
        
        // 删除目录下所有文件
        foreach (var fileInfo in dirPathInfo.GetFiles()) 
            fileInfo.Delete();
        
        // 递归删除所有子目录
        foreach (var subDirectory in dirPathInfo.GetDirectories()) 
            Delete(subDirectory.FullName);
        
        // 删除目录
        if (isDeleteRoot)
        {
            dirPathInfo.Attributes = FileAttributes.Normal;
            dirPathInfo.Delete();
        }

        return true;
    }

    /// <summary>
    ///     递归复制指定文件夹及文件夹/文件
    /// </summary>
    /// <param name="sourcePath"> 源文件夹路径 </param>
    /// <param name="targetPath"> 目的文件夹路径 </param>
    /// <param name="searchPatterns"> 要复制的文件扩展名数组 </param>
    public static void Copy(string sourcePath, string targetPath, string[] searchPatterns = null)
    {
        Check.NotNull(sourcePath, nameof(sourcePath));
        Check.NotNull(targetPath, nameof(targetPath));

        if (!Directory.Exists(sourcePath)) 
            throw new DirectoryNotFoundException("递归复制文件夹时源目录不存在。");
        
        if (!Directory.Exists(targetPath)) 
            Directory.CreateDirectory(targetPath);
        
        var dirs = Directory.GetDirectories(sourcePath);
        if (dirs.Length > 0)
        {
            foreach (var dir in dirs)
            {
                Copy(dir, targetPath + dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)));
            }
        }

        if (searchPatterns is { Length: > 0 })
        {
            foreach (var searchPattern in searchPatterns)
            {
                var files = Directory.GetFiles(sourcePath, searchPattern);
                if (files.Length <= 0) 
                    continue;
                foreach (var file in files)
                {
                    File.Copy(file, targetPath + file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)));
                }
            }
        }
        else
        {
            var files = Directory.GetFiles(sourcePath);
            if (!files.Any()) return;
            foreach (var file in files)
            {
                var targetFilePath = targetPath + file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal));
                File.Copy(file, targetFilePath, true);
            }
        }
    }

    /// <summary>
    ///     将文件或目录及其内容移到新位置
    /// </summary>
    /// <param name="sourceDirName"> 源文件夹路径 </param>
    /// <param name="destDirName"> 目的文件夹路径,必须不存在 </param>
    public static void Move(string sourceDirName, string destDirName)
    {
        if (!Directory.Exists(sourceDirName))
            throw new DirectoryNotFoundException("转移文件夹时源目录不存在。");
        
        // 文件夹移动不能同名目录
        var di = new DirectoryInfo(sourceDirName);
        di.MoveTo(destDirName);
    }
    
    /// <summary>
    ///     获取指定目录下的文件信息集合
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="searchPattern"></param>
    /// <param name="topDirectoryOnly"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static FileInfo[] GetFiles(string directoryPath, string searchPattern, bool topDirectoryOnly = true)
    {
        if (!Directory.Exists(directoryPath)) 
            throw new DirectoryNotFoundException("获取文件清单时源目录不存在。");

        var dir = new DirectoryInfo(directoryPath);
        return dir.GetFiles(searchPattern, topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
    }

    /// <summary>
    ///     列出指定文件夹下匹配的文件的完整名称（包括路径）集合
    /// </summary>
    /// <param name="directoryPath"> 文件夹路径 </param>
    /// <param name="searchPattern"> 匹配文件名搜索字符串:包含有效文本路径和通配符（*和？）的组合字符 </param>
    /// <param name="topDirectoryOnly"> 是否仅展示根目录 </param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, bool topDirectoryOnly = true)
    {
        if (!Directory.Exists(directoryPath)) 
            throw new DirectoryNotFoundException("获取文件清单时源目录不存在。");
        
        return Directory.EnumerateFiles(directoryPath, searchPattern, topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
    }

    /// <summary>
    ///     获取指定目录下的子目录的名称（包括路径）集合
    /// </summary>
    /// <param name="path"> 根目录 </param>
    /// <param name="searchPattern"> 匹配规则 </param>
    /// <param name="topDirectoryOnly"> 是否递归查询 </param>
    /// <returns></returns>
    public static string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
    {
        if (string.IsNullOrEmpty(searchPattern))
            searchPattern = "*";

        return Directory.GetDirectories(path, searchPattern, topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
    }
    
    /// <summary>
    ///     获取指定目录下的子目录的名称（包括路径）集合
    /// </summary>
    /// <param name="path"> 根目录 </param>
    /// <param name="searchPattern"> 匹配规则 </param>
    /// <param name="topDirectoryOnly"> 是否递归查询 </param>
    /// <returns></returns>
    public static IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
    {
        if (string.IsNullOrEmpty(searchPattern))
            searchPattern = "*";

        var directory = new DirectoryInfo(path);
        return directory.EnumerateDirectories(searchPattern, topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
    }

    /// <summary>
    ///     设置或取消目录的<see cref="FileAttributes" />属性。
    /// </summary>
    /// <param name="directory">目录路径</param>
    /// <param name="attribute">要设置的目录属性</param>
    /// <param name="isSet">true为设置，false为取消</param>
    public static void SetAttributes(string directory, FileAttributes attribute, bool isSet)
    {
        Check.NotNull(directory, nameof(directory));

        var di = new DirectoryInfo(directory);
        if (!di.Exists) throw new DirectoryNotFoundException("设置目录属性时指定文件夹不存在");
        if (isSet)
            di.Attributes |= attribute;
        else
            di.Attributes &= ~attribute;
    }
}