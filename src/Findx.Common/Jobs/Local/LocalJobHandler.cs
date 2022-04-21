using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Messaging;

namespace Findx.Jobs.Local
{
	internal class LocalJobHandler: IApplicationEventHandler<LocalJobRequest>
	{
        private readonly IServiceProvider _provider;

        private readonly IJobListener _listener;

        public LocalJobHandler(IServiceProvider provider, IJobListener listener)
        {
            _provider = provider;
            _listener = listener;
        }

        public Task HandleAsync(LocalJobRequest message, CancellationToken cancellationToken = default)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(message.Detail, nameof(message.Detail));

            var context = new JobContext(_provider, message.Detail.Id, message.Detail.Id, message.Detail.FullName)
            {
                Parameter = (message.Detail.JsonParam ?? "{}").ToObject<Dictionary<string, string>>(),
                JobName = message.Detail.Name
            };

            return _listener.JobToRunAsync(context, cancellationToken);
        }
    }
}

