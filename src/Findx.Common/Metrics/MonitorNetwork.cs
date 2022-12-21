using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Findx.Threading;

namespace Findx.Metrics;

/// <summary>
/// 网络信息监控
/// </summary>
public class MonitorNetwork
{
    /// <summary>
    /// 上行速率
    /// </summary>
    public string UpSpeed { get; set; }
    
    /// <summary>
    /// 下行速率
    /// </summary>
    public string DownSpeed { get; set; }
    
    /// <summary>
    /// 所有流量
    /// </summary>
    public string AllTraffic { get; set; }
    
    /// <summary>
    /// 网卡信息
    /// </summary>
    private string NetCardDescription { get; set; }
   
    /// <summary>
    /// 建立连接时上传的数据量
    /// </summary>
    private long BaseTraffic { get; set; }
    
    /// <summary>
    /// 最后一次上行速率
    /// </summary>
    private long LastUp { get; set; }

    /// <summary>
    /// 最后一次下行速率
    /// </summary>
    private long LastDown { get; set; }
    
    /// <summary>
    /// 网络接口
    /// </summary>
    private NetworkInterface NetworkInterface { get; set; }
    
    /// <summary>
    /// 定时器
    /// </summary>
    private readonly FindxAsyncTimer _timer;

    /// <summary>
    /// 关闭计算
    /// </summary>
    public void Close()
    { 
        _timer.Stop();  
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="netCardDescription">网卡描述</param>
    public MonitorNetwork(string netCardDescription)
    {
        _timer = new FindxAsyncTimer(null, null)
        {
          Period = 1000, // 1 sec.
          Elapsed = Timer_Elapsed,
          RunOnStart = false
        };

        NetCardDescription = netCardDescription;
    }

    /// <summary>
    /// 开始计算
    /// </summary>
    /// <returns></returns>
    public bool Start()
    {
        NetworkInterface = null;
        var nicks = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var var in nicks)
        {
            if (var.Description.Contains(NetCardDescription))
            {
                NetworkInterface = var;
                break;
            }
        }

        if (NetworkInterface == null)
        {
            return false;
        }

        BaseTraffic = (NetworkInterface.GetIPStatistics().BytesSent + NetworkInterface.GetIPStatistics().BytesReceived);
        LastUp = NetworkInterface.GetIPStatistics().BytesSent;
        LastDown = NetworkInterface.GetIPStatistics().BytesReceived;
        
        _timer.Start();
        
        return true;
    }

    /// <summary>
    /// 速率单位
    /// </summary>
    private readonly string[] _units = { "KB/s", "MB/s", "GB/s" };
 
    /// <summary>
    /// 计算上行流量
    /// </summary>
    private void CalcUpSpeed()
    {
        var nowValue = NetworkInterface.GetIPStatistics().BytesSent;  
        var num = 0;
        var value = (nowValue - LastUp) / 1024.0;
        while (value > 1023)
        {
            value = (value / 1024.0);
            num++;
        }  
        UpSpeed = value.ToString("0.0") + _units[num];  
        LastUp = nowValue;  
    }
   
    /// <summary>
    /// 计算下行流量
    /// </summary>
    private void CalcDownSpeed()
    {
        var nowValue = NetworkInterface.GetIPStatistics().BytesReceived;  
        var num = 0;
        var value = (nowValue - LastDown) / 1024.0;   
        while (value > 1023)
        {
            value = (value / 1024.0);
            num++;
        }  
        DownSpeed = value.ToString("0.0") + _units[num];
        LastDown = nowValue;  
    }
   
    /// <summary>
    /// 流量单位
    /// </summary>
    private readonly string[] _unitAlls = new string[] { "KB", "MB", "GB" ,"TB"};
   
    /// <summary>
    /// 计算流量
    /// </summary>
    private void CalcAllTraffic()
    {
        var nowValue = LastDown + LastUp;  
        var num = 0;
        var value = (nowValue - BaseTraffic) / 1024.0;
        while (value > 1023)
        {
            value = (value / 1024.0);
            num++;
        }  
        AllTraffic = value.ToString("0.0") + _unitAlls[num];
    }

    /// <summary>
    /// 定时执行方法
    /// </summary>
    /// <param name="timer"></param>
    private Task Timer_Elapsed(FindxAsyncTimer timer)
    {
       CalcUpSpeed();
       CalcDownSpeed();
       CalcAllTraffic();
       return Task.CompletedTask;
    }
}