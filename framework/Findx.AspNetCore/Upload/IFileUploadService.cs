using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.AspNetCore.Upload.Params;
using Findx.Common;
using Microsoft.AspNetCore.Http;

namespace Findx.AspNetCore.Upload;

/// <summary>
///     文件上传服务
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    ///     上传文件。单文件
    /// </summary>
    /// <param name="param">参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<FileSpec> UploadAsync(SingleFileUploadParam param, CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件。多文件
    /// </summary>
    /// <param name="param">参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IEnumerable<FileSpec>> UploadAsync(MultipleFileUploadParam param, CancellationToken cancellationToken = default);

    /// <summary>
    ///     保存文件
    /// </summary>
    /// <param name="formFile">表单文件</param>
    /// <param name="savePath">保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SaveAsync(IFormFile formFile, string savePath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     保存文件并返回文件MD5值
    /// </summary>
    /// <param name="formFile">表单文件</param>
    /// <param name="savePath">保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<string> SaveWithMd5Async(IFormFile formFile, string savePath, CancellationToken cancellationToken = default);
}