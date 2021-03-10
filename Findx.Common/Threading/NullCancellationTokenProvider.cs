using System.Threading;

namespace Findx.Threading
{
    public class NullCancellationTokenProvider : ICancellationTokenProvider
    {
        public CancellationToken Token => CancellationToken.None;
    }
}
