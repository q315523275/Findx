using System;
using Findx.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.Guids
{
    /// <summary>
    /// 与ABP对接使用
    /// </summary>
    public class SequentialGuidGenerator : IGuidGenerator, ISingletonDependency
    {
        private readonly IOptions<SequentialGuidOptions> _options;

        public SequentialGuidGenerator(IOptions<SequentialGuidOptions> options)
        {
            _options = options;
        }

        public Guid Create()
        {
            var sequentialGuidType = _options.Value.SequentialGuidType ?? Utils.SequentialGuidType.SequentialAsString;

            return Utils.SequentialGuid.Instance.Create(sequentialGuidType);
        }
    }
}

