#if NETFRAMEWORK
namespace ChaosLib.Dynamic
{
    /// <summary>
    /// Represents a duck type proxy.
    /// </summary>
    public interface IDuckProxy
    {
        /// <summary>
        /// Gets the value contained in the proxy.
        /// </summary>
        /// <returns>The value contained in the proxy.</returns>
        object GetInner();
    }

    /// <summary>
    /// Represents a duck type proxy.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the proxy</typeparam>
    public interface IDuckProxy<T> : IDuckProxy
    {
        /// <summary>
        /// Gets the value contained in the proxy.
        /// </summary>
        /// <returns>The value contained in the proxy.</returns>
        new T GetInner();
    }
}
#endif