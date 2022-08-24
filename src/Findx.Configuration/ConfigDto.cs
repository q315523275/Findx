using System.Collections.Generic;

namespace Findx.Configuration
{
    internal class ConfigDto
    {
        public int Version { set; get; }
        public List<KeyValueDto> Data { set; get; }
    }
    /// <summary>
    /// KeyValueDto
    /// </summary>
    internal class KeyValueDto
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
