using System.Diagnostics;
using System.Threading;

namespace ChaosLib
{
    //Represents an event that wakes up when there's work to do

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

                Debug.Assert(currentCount >= 0, "Set should never be called when count is already 0");

                //In Release builds, just break out
                if (count == 0)
                    break;

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