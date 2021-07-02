using System.Threading.Tasks;

namespace Findx.Pdf
{
    /// <summary>
    /// Pdf转换器
    /// </summary>
    public interface IPdfConverter
    {
        /// <summary>
        /// Html字符串转换为Pdf二进制数据
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        Task<byte[]> ConvertAsync(string htmlString);
    }
}
