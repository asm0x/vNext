using System;
using System.Threading;

namespace vNext
{
    /// <summary>
    /// Механизм синхронизации потоков в workflow.
    /// </summary>
    public class Sync
    {
        readonly SemaphoreSlim sync = new SemaphoreSlim(1, 1);


        public Section Lock(int wait = Timeout.Infinite)
        {
            if (sync.Wait(wait))
            {
                return new Section(sync);
            }

            throw new TimeoutException();
        }

        public struct Section : IDisposable
        {
            readonly SemaphoreSlim sync;

            public Section(SemaphoreSlim sync)
            {
                this.sync = sync;
            }
            public void Dispose()
            {
                sync.Release();
            }
        }
    }
}
