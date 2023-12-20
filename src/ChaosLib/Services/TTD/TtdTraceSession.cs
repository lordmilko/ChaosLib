using System;
using System.Threading;

namespace ChaosLib.TTD
{
    public class TtdTraceSession
    {
        private int processId;

        public int ProcessId
        {
            get
            {
                if (processId == -1)
                    throw new InvalidOperationException($"Cannot get {nameof(ProcessId)} when process was started without launch and attach.");

                return processId;
            }
        }

        private DispatcherOperation op;
        private Action<TtdDiagnosticMessage> callback;

        public WaitHandle WaitHandle => ((IAsyncResult) op.Task).AsyncWaitHandle;

        public TtdTraceSession(int processId, DispatcherOperation op, Action<TtdDiagnosticMessage> callback)
        {
            this.processId = processId;
            this.op = op;
            this.callback = callback;
        }

        //I don't know what exactly this does; I couldn't see any events, breakpoints or data model items
        //in WinDbg that would let me get at the mark. It does say that the mark is being applied successfully
        public void Mark(string value) =>
            ttd.Execute(new TtdOptions {Mark = new TtdMarkOption(ProcessId, value)}, callback);

        public void Wait() => op.Wait();

        public void Stop()
        {
            ttd.Execute(new TtdOptions
            {
                Stop = ProcessId.ToString()
            }, callback);
        }
    }
}