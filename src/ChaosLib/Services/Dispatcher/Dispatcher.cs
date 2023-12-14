using System;
using System.Threading;

namespace ChaosLib
{
    //https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/Dispatcher.cs
    //https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherOperation.cs

    /// <summary>
    /// Provides services for dispatching messages from one thread to another.<para/>
    /// Modelled on System.Windows.Threading.Dispatcher.
    /// </summary>
    public class Dispatcher
    {
        /// <summary>
        /// Gets the thread that the dispatcher was created on.
        /// </summary>
        public Thread Thread { get; }

        public bool HasShutdownStarted { get; private set; }

        public bool HasShutdownFinished { get; private set; }

        public event EventHandler ShutdownStarted;

        public event EventHandler ShutdownFinished;

        private LinkedQueue<DispatcherOperation> queue;

        private bool startingShutdown;
        internal object objLock = new object();
        CancellationTokenSource cts = new CancellationTokenSource();

        public EventHandler OperationQueued;

        /// <summary>
        /// Gets whether the current thread is the same thread that the dispatcher was created on.
        /// </summary>
        /// <returns>True if the caller is on the same thread that the dispatcher was created on, otherwise false.</returns>
        public bool CheckAccess() => Thread == Thread.CurrentThread;

        /// <summary>
        /// Asserts that the current thread is the same thread that the dispatcher was created on.
        /// </summary>
        /// <exception cref="InvalidOperationException">The current thread is not the same thread that the dispatcher was created on.</exception>
        public void VerifyAccess()
        {
            if (!CheckAccess())
                throw new InvalidOperationException($"Attempted to perform an operation on the {nameof(Dispatcher)} that is restricted to the dispatcher thread. Current thread: {Thread.CurrentThread.ManagedThreadId}. Required thread: {Thread.ManagedThreadId}.");
        }

        public Dispatcher()
        {
            Thread = Thread.CurrentThread;
            queue = new LinkedQueue<DispatcherOperation>();
        }

        public void BeginInvokeShutdown() => InvokeAsync(ShutdownCallbackInternal);

        public void InvokeShutdown() => Invoke(ShutdownCallbackInternal);

        private void ShutdownCallbackInternal()
        {
            StartShutdownImpl();
        }

        private void StartShutdownImpl()
        {
            if (!startingShutdown)
            {
                startingShutdown = true;

                ShutdownStarted?.Invoke(this, EventArgs.Empty);

                HasShutdownStarted = true;

                cts.Cancel();

                //If any callbacks are currently running, we'll need to defer shutting down
                //until after those callbacks finish executing
                if (queue.Count != 0)
                {
                    ShutdownImpl();
                }
            }
        }

        private void ShutdownImpl()
        {
            if (!HasShutdownFinished)
            {
                ShutdownFinished?.Invoke(this, EventArgs.Empty);

                lock (objLock)
                    HasShutdownFinished = true;

                while (queue.Count > 0)
                {
                    var item = queue.Take();

                    item.Abort();
                }
            }
        }

        internal bool Abort(DispatcherOperation operation)
        {
            lock (objLock)
            {
                var result = queue.Remove(operation);

                if (result)
                    operation.Status = DispatcherOperationStatus.Aborted;

                return result;
            }
        }

        /* System.Windows.Threading.Dispatcher contains a number of "Legacy" Invoke methods
         * that take a raw Delegate. These methods are extremely dangerous to use, as they dictate
         * that the DispatcherOperation should NOT guarantee that it will catch any exceptions
         * that occur within the user's callback method. If an exception occurs in user code,
         * this legacy code path won't catch the exception, the exception will go unhandled and
         * the dispatcher thread will terminate. We do not support this legacy mechanism to prevent
         * you from shooting yourself in the foot */

        #region Action/Func

        public void Invoke(Action callback, CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.IsCancellationRequested && CheckAccess())
            {
                var oldCtx = SynchronizationContext.Current;

                try
                {
                    var newCtx = new DispatcherSynchronizationContext(this);

                    SynchronizationContext.SetSynchronizationContext(newCtx);

                    callback();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(oldCtx);
                }
            }
            else
            {
                var operation = new DispatcherOperation(this, callback);

                InvokeImpl(operation, cancellationToken);
            }
        }

        public TResult Invoke<TResult>(Func<TResult> callback, CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.IsCancellationRequested && CheckAccess())
            {
                var oldCtx = SynchronizationContext.Current;

                try
                {
                    var newCtx = new DispatcherSynchronizationContext(this);

                    SynchronizationContext.SetSynchronizationContext(newCtx);

                    return callback();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(oldCtx);
                }
            }
            else
            {
                var operation = new DispatcherOperation<TResult>(this, callback);

                return (TResult) InvokeImpl(operation, cancellationToken);
            }
        }

        private object InvokeImpl(DispatcherOperation operation, CancellationToken cancellationToken)
        {
            object result = null;

            if (!cancellationToken.IsCancellationRequested)
            {
                InvokeAsyncImpl(operation, cancellationToken);

                operation.Wait();

                result = operation.Result;
            }

            return result;
        }

        #endregion
        #region Async Action/Func

        //This is equivalent to "BeginInvoke"

        public DispatcherOperation InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            var operation = new DispatcherOperation(this, callback);

            InvokeAsyncImpl(operation, cancellationToken);

            return operation;
        }

        public DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken = default)
        {
            var operation = new DispatcherOperation<TResult>(this, callback);

            InvokeAsyncImpl(operation, cancellationToken);

            return operation;
        }

        private void InvokeAsyncImpl(DispatcherOperation operation, CancellationToken cancellationToken)
        {
            var succeeded = false;

            lock (objLock)
            {
                if (!cancellationToken.IsCancellationRequested && !HasShutdownFinished && !Environment.HasShutdownStarted)
                {
                    queue.Add(operation);
                    OperationQueued?.Invoke(this, EventArgs.Empty);

                    succeeded = true;
                }
            }

            if (succeeded)
            {
                if (cancellationToken.CanBeCanceled)
                {
                    var registration = cancellationToken.Register(o => ((DispatcherOperation) o).Abort(), operation);

                    operation.Aborted += (s, e) => registration.Dispose();
                    operation.Completed += (s, e) => registration.Dispose();
                }
            }
            else
            {
                operation.Status = DispatcherOperationStatus.Aborted;
                operation.CancelTask();
            }
        }

        #endregion

        public void Run(CancellationToken cancellationToken)
        {
            VerifyAccess();

            var merged = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (HasShutdownStarted)
                {
                    ShutdownImpl();
                    break;
                }

                WaitHandle.WaitAny(new[] {merged.Token.WaitHandle, queue.WaitHandle});

                if (merged.Token.IsCancellationRequested)
                    break;

                var operation = queue.Take();

                if (HasShutdownStarted)
                {
                    operation.Abort();
                    ShutdownImpl();
                    break;
                }

                operation.Invoke();
            }
        }

        public void DrainQueue()
        {
            while (queue.TryTake(out var op))
                op.Invoke();
        }

        internal void Push(DispatcherOperation operation)
        {
            var completedEvent = new ManualResetEventSlim(false);

            void OnCompletedOrAborted(object sender, EventArgs e)
            {
                completedEvent.Set();
            }

            operation.Completed += OnCompletedOrAborted;
            operation.Aborted += OnCompletedOrAborted;

            while (!cts.IsCancellationRequested)
            {
                WaitHandle.WaitAny(new[]{cts.Token.WaitHandle, completedEvent.WaitHandle, queue.WaitHandle});

                if (cts.IsCancellationRequested)
                    break;

                if (completedEvent.IsSet)
                    break;

                var op = queue.Take();

                op.Invoke();
            }

            operation.Completed -= OnCompletedOrAborted;
            operation.Aborted -= OnCompletedOrAborted;
        }
    }
}
