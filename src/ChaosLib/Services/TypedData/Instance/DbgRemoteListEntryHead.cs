using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChaosLib.TypedData
{
    //Represents the head list entry
    public class DbgRemoteListEntryHead : DbgRemoteObject, IEnumerable<DbgRemoteObject>
    {
        public DbgRemoteListEntryHead(long address, DbgRemoteType type, ITypedDataProvider provider) : base(address, type, provider)
        {
        }

        public List<DbgRemoteObject> ToList(string elementType, string pointerField)
        {
            var list = new List<DbgRemoteObject>();

            var enumerator = new TypedListEntryEnumerator(this, elementType, pointerField);

            while (enumerator.MoveNext())
                list.Add(enumerator.Current);

            return list;
        }

        public DbgRemoteObject[] ToArray(string elementType, string pointerField) => ToList(elementType, pointerField).ToArray();

        public IEnumerator<DbgRemoteObject> GetEnumerator() => new ListEntryEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region ListEntryEnumerator

        private class ListEntryEnumerator : IEnumerator<DbgRemoteObject>
        {
            private DbgRemoteListEntryHead head;

            public ListEntryEnumerator(DbgRemoteListEntryHead head)
            {
                this.head = head;
            }

            public bool MoveNext()
            {
                DbgRemoteObject next;

                //The head of the list is literally just a LIST_ENTRY; but every other LIST_ENTRY* we point to is in fact a member in some struct
                if (Current == null)
                    next = (DbgRemoteObject) head["Flink"];
                else
                    next = (DbgRemoteObject) Current["Flink"];

                //Observe how this check is performed regardless of whether we're currently processing the head's Flink or not. If the head points to itself, the list is empty!
                if (next.Address == head.Address)
                    return false;

                Current = next;
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public DbgRemoteObject Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        #endregion
        #region TypedListEntryEnumerator

        private class TypedListEntryEnumerator : IEnumerator<DbgRemoteObject>
        {
            private DbgRemoteListEntryHead head;
            private DbgRemoteType elementType;
            private long offset;

            public TypedListEntryEnumerator(DbgRemoteListEntryHead head, string elementType, string pointerField)
            {
                this.head = head;
                this.elementType = DbgRemoteType.New(elementType, head.provider);

                var field = this.elementType.Fields.SingleOrDefault(f => f.Name == pointerField);

                if (field == null)
                    throw new ArgumentException($"Could not find field '{pointerField}' on type '{elementType}'");

                offset = field.Offset;
            }

            public bool MoveNext()
            {
                DbgRemoteObject entry;

                if (Current == null)
                    entry = (DbgRemoteObject) head["Flink"];
                else
                    entry = (DbgRemoteObject) currentEntry["Flink"];

                //Observe how this check is performed regardless of whether we're currently processing the head's Flink or not. If the head points to itself, the list is empty!
                if (entry.Address == head.Address)
                    return false;

                currentEntry = entry;
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            //LIST_ENTRY
            private DbgRemoteObject currentEntry;

            public DbgRemoteObject Current
            {
                get
                {
                    if (currentEntry == null)
                        return null;

                    return new DbgRemoteObject(currentEntry.Address - offset, elementType, head.provider);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        #endregion
    }
}