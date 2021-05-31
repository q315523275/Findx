namespace Findx.Utils
{
    /// <summary>
    /// 机器信息
    /// </summary>
    public class MachineInfo
    {
        #region 属性
        /// <summary>
        /// 系统名称
        /// </summary>
        public string OSName { get; set; }

        /// <summary>
        /// 系统版本
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// 产品名称。制造商
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// 处理器型号
        /// </summary>
        public string Processor { get; set; }

        /// <summary>
        /// 处理器序列号
        /// </summary>
        public string CpuID { get; set; }

        /// <summary>
        /// 硬件唯一标识
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// 系统标识
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 磁盘序列号
        /// </summary>
        public string DiskID { get; set; }

        /// <summary>
        /// 内存总量
        /// </summary>
        public long Memory { get; set; }

        /// <summary>
        /// 可用内存
        /// </summary>
        public long AvailableMemory { get; private set; }

        /// <summary>
        /// CPU占用率
        /// </summary>
        public double CpuRate { get; private set; }

        /// <summary>
        /// 网络上行速度。字节每秒，初始化后首次读取为0
        /// </summary>
        public long UplinkSpeed { get; set; }

        /// <summary>
        /// 网络下行速度。字节每秒，初始化后首次读取为0
        /// </summary>
        public long DownlinkSpeed { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// 电池剩余
        /// </summary>
        public double Battery { get; set; }
        #endregion
    }
}
