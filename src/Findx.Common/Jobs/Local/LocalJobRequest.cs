using System;
using Findx.Messaging;

namespace Findx.Jobs.Local
{
	internal class LocalJobRequest: IApplicationEvent
	{
		public JobInfo Detail { set; get; }
	}
}

