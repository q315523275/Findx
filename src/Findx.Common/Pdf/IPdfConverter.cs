using System.Threading.Tasks;

namespace Findx.Pdf
{
    public interface IPdfConverter
    {
        Task<byte[]> ConvertAsync(string htmlString);
    }
}
