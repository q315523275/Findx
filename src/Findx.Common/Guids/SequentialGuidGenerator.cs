using System;
using Findx.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.Guids
{
    /// <summary>
    /// 与ABP对接使用
    /// </summary>
    public class SequentialGuidGenerator : IGuidGenerator
    {
        private readonly IOptions<SequentialGuidOptions> _options;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        public SequentialGuidGenerator(IOptions<SequentialGuidOptions> options)
        {
            _options = options;
        }

        /// <summary>
        /// 创建有序Guid
        /// </summary>
        /// <returns></returns>
        public Guid Create()
        {
            var sequentialGuidType = _options.Value.SequentialGuidType ?? Utils.SequentialGuidType.SequentialAsString;

            return Utils.SequentialGuid.Instance.Create(sequentialGuidType);
        }
    }
}

