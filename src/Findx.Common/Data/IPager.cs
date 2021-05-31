namespace Findx.Data
{
    public interface IPager
    {
        int PageNo { get; set; }
        int PageSize { get; set; }
        string Order { get; set; }
        bool Asc { get; set; }
    }
}
