using System.Threading;
using System.Threading.Tasks;
using Findx.DinkToPdf.Contracts;
using Findx.DinkToPdf.Documents;
using Findx.DinkToPdf.Settings;
using Findx.DinkToPdf.Utils;
using Findx.Pdf;

namespace Findx.DinkToPdf;

public class DinkToPdfConverter : IPdfConverter
{
    private readonly IConverter _converter;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="converter"></param>
    public DinkToPdfConverter(IConverter converter)
    {
        _converter = converter;
    }

    /// <summary>
    ///     转换
    /// </summary>
    /// <param name="htmlString"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<byte[]> ConvertAsync(string htmlString, CancellationToken cancellationToken = default)
    {
        var doc = new HtmlToPdfDocument
        {
            GlobalSettings =
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = htmlString,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        var pdf = _converter.Convert(doc);
        return Task.FromResult(pdf);
    }
}