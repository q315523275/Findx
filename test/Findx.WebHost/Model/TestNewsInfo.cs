﻿using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.WebHost.Model
{
    public class TestNewsInfo: IEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
    }
}
