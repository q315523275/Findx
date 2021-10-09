using Findx.Data;
using System;

namespace Findx.WebHost.Model
{
    public class TestUserInfo : ISoftDeletable
    {
        public int Id { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool Deleted { get; set; }
    }
}
