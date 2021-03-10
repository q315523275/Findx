using System.Collections.Generic;

namespace Findx.Configuration
{
    public class Response
    {
        public int Version { set; get; }
        public List<RspConfigDTO> Data { set; get; }
    }
    public class RspConfigDTO
    {
        public VaultType VaultType { set; get; }
        public string VaultKey { set; get; }
        public string Vault { set; get; }
    }
    public enum VaultType
    {
        Text = 0,
        Json = 1
    }
}
