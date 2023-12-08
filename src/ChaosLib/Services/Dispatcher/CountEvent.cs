using System.Threading;

namespace ChaosLib
{
    class CountEvent
    {
        private ManualResetEventSlim manualEvent = new ManualResetEventSlim(false);

        public int CurrentCount => currentCount;

        private int currentCount;

        public bool IsSet => CurrentCount > 0;

        public WaitHandle WaitHandle => manualEvent.WaitHandle;

        public void Add()
        {
            var spinWait = new SpinWait();

            while (true)
            {
                var count = CurrentCount;

                if (Interlocked.CompareExchange(ref this.currentCount, count + 1, count) != count)
                    spinWait.SpinOnce();
                else
                    break;
            }

            if (CurrentCount > 0)
                manualEvent.Set();
        }

        public void Set()
        {
            var spinWait = new SpinWait();

            while (true)
            {
                var count = CurrentCount;

                if (Interlocked.CompareExchange(ref this.currentCount, count - 1, count) != count)
                    spinWait.SpinOnce();
                else
                    break;
            }

            if (CurrentCount == 0)
                manualEvent.Reset();
        }
    }
}