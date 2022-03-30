using Findx.Data;
using System;

namespace Findx.SqlSugar
{
    internal class SoftDeletable : ISoftDeletable
    {
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
