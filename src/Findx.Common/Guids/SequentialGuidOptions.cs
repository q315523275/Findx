using System;
using Findx.Utils;
using Microsoft.Extensions.Options;

namespace Findx.Guids
{
    public class SequentialGuidOptions : IOptions<SequentialGuidOptions>
    {
        public SequentialGuidOptions Value => this;

        public SequentialGuidType? SequentialGuidType { set; get; }
    }
}

