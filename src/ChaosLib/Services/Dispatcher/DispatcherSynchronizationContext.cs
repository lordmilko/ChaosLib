using System.Threading;

namespace ChaosLib
{
    /// <summary>
    /// Represents a <see cref="SynchronizationContext"/> that ensures we return to the thread that the <see cref="Dispatcher"/>
    /// is consuming events on in the event an awaited method that it is executing switches us to a different thread.
    /// </summary>
    internal class DispatcherSynchronizationContext : SynchronizationContext
    {
        private Dispatcher dispatcher;

        public DispatcherSynchronizationContext(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override void Send(SendOrPostCallback d, object state) =>
            dispatcher.Invoke(() => d.Invoke(state));

        public override void Post(SendOrPostCallback d, object state) =>
            dispatcher.InvokeAsync(() => d.Invoke(state));

        public override SynchronizationContext CreateCopy() =>
            new DispatcherSynchronizationContext(dispatcher);
    }
}