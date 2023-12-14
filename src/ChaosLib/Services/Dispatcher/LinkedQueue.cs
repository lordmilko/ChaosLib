using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ChaosLib
{
    //We need to be able to remove items from an arbitrary position in the queue when an item is aborted.
    //Thus, we have a custom queue based on a linked list
    class LinkedQueue<T>
    {
        private LinkedList<T> list = new LinkedList<T>();
        private object objLock = new object();
        private CountEvent countEvent = new CountEvent();

        public WaitHandle WaitHandle => countEvent.WaitHandle;

        public T Take()
        {
            if ((TryTake(out var item)))
                return item;

            throw new InvalidOperationException("Attempted to Take when queue was empty.");
        }

        public void Add(T item)
        {
            lock (objLock)
            {
                Debug.Assert(countEvent.CurrentCount == list.Count, "Prior to adding an item, the event and list queue were out of sync");
                list.AddLast(new LinkedListNode<T>(item));
                countEvent.Add();
                Debug.Assert(countEvent.CurrentCount == list.Count, "Upon adding an item, the event and list queue became out of sync");
            }
        }

        public bool TryTake(out T item)
        {
            lock (objLock)
            {
                if (list.Count == 0)
                {
                    item = default;
                    return false;
                }

                Debug.Assert(countEvent.CurrentCount == list.Count, "Prior to removing an item, the event and list queue were out of sync");
                item = list.First.Value;
                list.RemoveFirst();
                countEvent.Set();
                Debug.Assert(countEvent.CurrentCount == list.Count, "Upon removing an item, the event and list queue were out of sync");
                return true;
            }
        }

        public bool Remove(T item)
        {
            lock (objLock)
            {
                var removed = list.Remove(item);

                if (removed)
                    countEvent.Set();

                return removed;
            }
        }

        public int Count
        {
            get
            {
                lock (objLock)
                    return list.Count;
            }
        }
    }
}
