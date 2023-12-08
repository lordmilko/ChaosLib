using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ChaosLib
{
    public class DispatcherOperation<TResult> : DispatcherOperation
    {
#pragma warning disable CS0612
        [Obsolete]
        private TaskCompletionSource<TResult> taskSource;

        internal DispatcherOperation(Dispatcher dispatcher, Func<TResult> func) : base(dispatcher, func, false)
        {
            taskSource = new TaskCompletionSource<TResult>(this);
        }

        protected override Task GetTask() => taskSource.Task;
        internal override void CancelTask() => taskSource.SetCanceled();
        protected override void SetTaskException(Exception ex) => taskSource.SetException(ex);
        protected override void SetTaskResult(object value) => taskSource.SetResult((TResult) value);

        public new Task<TResult> Task => taskSource.Task;
#pragma warning restore CS0612

        public new TaskAwaiter<TResult> GetAwaiter() => Task.GetAwaiter();

        public new TResult Result => (TResult) base.Result;

        protected override object InvokeDelegateCore() => ((Func<TResult>) method)();
    }
}
