using System;
using System.Diagnostics;
using System.Threading;

namespace ChaosLib
{
    /// <summary>
    /// Represents a <see cref="Thread"/> that is used to pump <see cref="Dispatcher"/> events.
    /// </summary>
    public class DispatcherThread : IDisposable
    {
        public Dispatcher Dispatcher { get; private set; }

        public int ManagedThreadId => thread.ManagedThreadId;

        private Thread thread;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public DispatcherThread(string name, ThreadStart threadProc = null)
        {
            var manualResetEventSlim = new ManualResetEventSlim(false);

            thread = new Thread(() =>
            {
                Dispatcher = new Dispatcher();

                cts.Token.Register(Dispatcher.BeginInvokeShutdown);

                manualResetEventSlim.Set();

                if (!cts.IsCancellationRequested)
                {
                    if (threadProc != null)
                        threadProc();
                    else
                        Dispatcher.Run(cts.Token);
                }
            });

            //Threads can either be foreground or background threads. Unlike foreground threads, background threads don't
            //don't prevent the process from terminating
            thread.IsBackground = true;
            thread.Name = name;

            thread.Start();

            manualResetEventSlim.Wait();

            manualResetEventSlim.Dispose();
            manualResetEventSlim = null;
        }

        [DebuggerNonUserCode]
        public T Invoke<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            //This is required for funceval
            //https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.debugger.notifyofcrossthreaddependency?view=net-8.0
            //https://learn.microsoft.com/en-us/archive/blogs/eliofek/why-do-we-get-the-function-evaluation-requires-all-threads-to-run

            Debugger.NotifyOfCrossThreadDependency();

            return Dispatcher.Invoke(callback, cancellationToken);
        }

        [DebuggerNonUserCode]
        public void Invoke(Action callback, CancellationToken cancellationToken = default)
        {
            Debugger.NotifyOfCrossThreadDependency();

            Dispatcher.Invoke(callback, cancellationToken);
        }

        public DispatcherOperation InvokeAsync(Action callback, CancellationToken cancellationToken = default) =>
            Dispatcher.InvokeAsync(callback, cancellationToken);

        public DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken = default) =>
            Dispatcher.InvokeAsync(callback, cancellationToken);

        public void Dispose()
        {
            //If the dispatcher thread is not actively pumping messages, attempting to
            //call InvokeShutdown() here will deadlock. As such, we "nicely request" a
            //shutdown if possible, and then wait for the dispatcher thread to terminate
            //(the true sign of a successful shutdown)
            Dispatcher.BeginInvokeShutdown();

            if (Thread.CurrentThread.ManagedThreadId != thread.ManagedThreadId)
                thread.Join();

            cts.Cancel();
        }
    }
}
