using Findx.Data;

namespace Findx.Module.EleAdmin.DTO;

public class QueryLoginRecordRequest: PageBase
{
    /// <summary>
    /// 账号
    /// </summary>
    public string UserName { set; get; }
		
    /// <summary>
    /// 用户名
    /// </summary>
    public string Nickname { set; get; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? CreatedTimeStart { set; get; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? CreatedTimeEnd { set; get; }
}