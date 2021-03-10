namespace Findx.Data
{
    public class PagedResult<TList>
    {
        private int PageSize { get; }
        private int PageNumber { get; }
        private TList Data { get; }
        public PagedResult() { }

        public PagedResult(int pageSize, int pageNumber, TList data)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            Data = data;
        }
    }
}
