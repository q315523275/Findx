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

    public DinkToPdfConverter(IConverter converter)
    {
        _converter = converter;
    }

    public Task<byte[]> ConvertAsync(string htmlString)
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