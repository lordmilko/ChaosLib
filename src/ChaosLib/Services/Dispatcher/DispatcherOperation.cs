using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ChaosLib
{
    public class DispatcherOperation
    {
        public Dispatcher Dispatcher { get; }

        public DispatcherOperationStatus Status { get; internal set; }

        public Task Task => GetTask();

        public WaitHandle WaitHandle => ((IAsyncResult) Task).AsyncWaitHandle;

        public string Name => method.Method.DeclaringType + "." + method.Method.Name;

        private object result;

        public object Result
        {
            get
            {
                Wait();

                if (Status == DispatcherOperationStatus.Completed || Status == DispatcherOperationStatus.Aborted)
                {
                    Task.GetAwaiter().GetResult();
                }

                return result;
            }
        }

        private EventHandler aborted;

        public event EventHandler Aborted
        {
            add
            {
                lock (Dispatcher.objLock)
                    aborted += value;
            }
            remove
            {
                lock (Dispatcher.objLock)
                    aborted -= value;
            }
        }

        private EventHandler completed;

        public event EventHandler Completed
        {
            add
            {
                lock (Dispatcher.objLock)
                    completed += value;
            }
            remove
            {
                lock (Dispatcher.objLock)
                    completed -= value;
            }
        }

        protected Delegate method;

        [Obsolete]
        private TaskCompletionSource<object> taskSource;
        private Exception exception;

        internal DispatcherOperation(Dispatcher dispatcher, Delegate method) : this(dispatcher, method, true)
        {
        }

        protected DispatcherOperation(Dispatcher dispatcher, Delegate method, bool createTaskSource)
        {
            Dispatcher = dispatcher;
            this.method = method;

#pragma warning disable CS0612
            if (createTaskSource)
                taskSource = new TaskCompletionSource<object>(this);
#pragma warning restore CS0612
        }

#pragma warning disable CS0612

        protected virtual Task GetTask() => taskSource.Task;
        internal virtual void CancelTask() => taskSource.SetCanceled();
        protected virtual void SetTaskException(Exception ex) => taskSource.SetException(ex);
        protected virtual void SetTaskResult(object value) => taskSource.SetResult(value);
#pragma warning restore CS0612

        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        public DispatcherOperationStatus Wait()
        {
            if (Status == DispatcherOperationStatus.Pending || Status == DispatcherOperationStatus.Executing)
            {
                if (Dispatcher.Thread == Thread.CurrentThread)
                {
                    if (Status == DispatcherOperationStatus.Executing)
                        throw new InvalidOperationException("A thread cannot wait on operations already running on the same thread.");

                    Dispatcher.Push(this);
                }
                else
                {
                    var manualResetEvent = new ManualResetEvent(false);
                    var eventClosed = false;

                    void OnCompletedOrAborted(object sender, EventArgs e)
                    {
                        lock (Dispatcher.objLock)
                        {
                            if (!eventClosed)
                                manualResetEvent.Set();
                        }
                    }

                    lock (Dispatcher.objLock)
                    {
                        Aborted += OnCompletedOrAborted;
                        Completed += OnCompletedOrAborted;

                        if (Status != DispatcherOperationStatus.Pending && Status != DispatcherOperationStatus.Executing)
                            manualResetEvent.Set();
                    }

                    manualResetEvent.WaitOne(-1, false);

                    lock (Dispatcher.objLock)
                    {
                        if (!eventClosed)
                        {
                            Aborted -= OnCompletedOrAborted;
                            Completed -= OnCompletedOrAborted;

                            manualResetEvent.Close();

                            eventClosed = true;
                        }
                    }
                }
            }

            if (Status == DispatcherOperationStatus.Completed || Status == DispatcherOperationStatus.Aborted)
            {
                Task.GetAwaiter().GetResult();
            }

            return Status;
        }

        public bool Abort()
        {
            bool removed = false;

            if (Dispatcher != null)
            {
                removed = Dispatcher.Abort(this);

                if (removed)
                {
                    CancelTask();

                    //Bypassing the lock and storing whatever the current value is
                    var handler = this.aborted;
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }

            return removed;
        }

        internal void Invoke()
        {
            //This method is invoked for both normal and async methods

            Status = DispatcherOperationStatus.Executing;

            var oldCtx = SynchronizationContext.Current;

            try
            {
                var newCtx = new DispatcherSynchronizationContext(Dispatcher);

                SynchronizationContext.SetSynchronizationContext(newCtx);

                try
                {
                    result = InvokeDelegateCore();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldCtx);
            }

            EventHandler handler;

            lock (Dispatcher.objLock)
            {
                if (exception is OperationCanceledException)
                {
                    handler = this.aborted;
                    Status = DispatcherOperationStatus.Aborted;
                }
                else
                {
                    handler = this.completed;
                    Status = DispatcherOperationStatus.Completed;
                }
            }

            handler?.Invoke(this, EventArgs.Empty);

            InvokeCompletions();
        }

        protected virtual object InvokeDelegateCore()
        {
            ((Action) method)();
            return null;
        }

        private void InvokeCompletions()
        {
            switch (Status)
            {
                case DispatcherOperationStatus.Aborted:
                    CancelTask();
                    break;

                case DispatcherOperationStatus.Completed:
                    if (exception != null)
                        SetTaskException(exception);
                    else
                        SetTaskResult(result);
                    break;

                default:
                    throw new NotImplementedException($"Don't know how to handle {nameof(DispatcherOperationStatus)} '{Status}'.");
            }
        }
    }
}