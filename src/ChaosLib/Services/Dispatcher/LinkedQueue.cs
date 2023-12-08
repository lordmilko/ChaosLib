using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ChaosLib
{
    //We need to be able to remove items from an arbitrary position in the queue when an item is aborted.
    //Thus, we have a custom queue based on a linked list
    class LinkedQueue<T> : IProducerConsumerCollection<T>
    {
        private LinkedList<T> list = new LinkedList<T>();
        private object objLock = new object();

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            lock (objLock)
            {
                list.AddLast(new LinkedListNode<T>(item));
                return true;
            }
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            lock (objLock)
            {
                if (list.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = list.First.Value;
                list.RemoveFirst();
                return true;
            }
        }

        public bool Remove(T item)
        {
            lock (objLock)
                return list.Remove(item);
        }

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        int ICollection.Count
        {
            get
            {
                lock (objLock)
                    return list.Count;
            }
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => throw new NotSupportedException();

        #endregion
        #region IProducerConsumerCollection

        void IProducerConsumerCollection<T>.CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        T[] IProducerConsumerCollection<T>.ToArray()
        {
            throw new NotImplementedException();
        }

        #endregion
        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
