using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    public class SysDictDataQuery : PageBase
    {
        public long TypeId { set; get; }

        public string Value { set; get; }

        public string Code { set; get; }
    }
}
