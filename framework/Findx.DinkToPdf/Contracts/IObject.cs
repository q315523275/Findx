namespace Findx.DinkToPdf.Contracts;

public interface IObject : ISettings
{
    byte[] GetContent();
}