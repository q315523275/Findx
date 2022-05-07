using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Messaging;

namespace Findx.Jobs.Local
{
	internal class LocalJobHandler: IApplicationEventHandler<JobInfo>
    {
        private readonly IServiceProvider _provider;

        private readonly IJobListener _listener;

        public LocalJobHandler(IServiceProvider provider, IJobListener listener)
        {
            _provider = provider;
            _listener = listener;
        }

        public Task HandleAsync(JobInfo message, CancellationToken cancellationToken = default)
        {
            Check.NotNull(message, nameof(message));

            var context = new JobContext(_provider, message.Id, message.Id, message.FullName)
            {
                Parameter = (message.JsonParam ?? "{}").ToObject<Dictionary<string, string>>(),
                JobName = message.Name
            };

            return _listener.JobToRunAsync(context, cancellationToken);
        }
    }
}

