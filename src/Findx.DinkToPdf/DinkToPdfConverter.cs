using DinkToPdf;
using DinkToPdf.Contracts;
using Findx.Pdf;
using System.Threading.Tasks;

namespace Findx.DinkToPdf
{
    public class DinkToPdfConverter : IPdfConverter
    {
        private readonly IConverter _converter;

        public DinkToPdfConverter(IConverter converter)
        {
            _converter = converter;
        }

        public Task<byte[]> ConvertAsync(string htmlString)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlString,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            byte[] pdf = _converter.Convert(doc);
            return Task.FromResult(pdf);
        }
    }
}
