using Findx.Data;
using System;

namespace Findx.SqlSugar
{
    internal class SoftDeletable : ISoftDeletable
    {
        public DateTime? DeletedTime { get; set; }
        public bool Deleted { get; set; }
    }
}
