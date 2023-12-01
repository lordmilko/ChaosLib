namespace ChaosLib.TypedData
{
    public interface IDbgRemoteValue
    {
        /// <summary>
        /// Gets the inner value contained in this <see cref="IDbgRemoteValue"/>.<para/>
        /// If this <see cref="IDbgRemoteValue"/> is a <see cref="DbgRemoteObject"/>, this value
        /// is the same value as this instance. Otherwise, this value is a primitive CLR value.
        /// </summary>
        object Value { get; }

        IDbgRemoteValue this[string name] { get; }
    }
}