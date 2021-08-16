using System.Collections.Generic;

namespace Findx.Configuration
{
    internal class Response
    {
        public int Version { set; get; }
        public List<RspConfigDTO> Data { set; get; }
    }
    internal class RspConfigDTO
    {
        public VaultType VaultType { set; get; }
        public string VaultKey { set; get; }
        public string Vault { set; get; }
    }
    internal enum VaultType
    {
        Text = 0,
        Json = 1
    }
}
