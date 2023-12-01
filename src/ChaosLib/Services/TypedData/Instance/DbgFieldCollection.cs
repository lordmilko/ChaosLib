using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChaosLib.TypedData
{
    internal sealed class DbgRemoteFieldCollectionDebugView
    {
        private DbgRemoteFieldCollection list;

        public DbgRemoteFieldCollectionDebugView(DbgRemoteFieldCollection list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DbgRemoteField[] Items
        {
            get
            {
                var items = list.ToArray();
                return items;
            }
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DbgRemoteFieldCollectionDebugView))]
    public class DbgRemoteFieldCollection : IEnumerable<DbgRemoteField>
    {
        public static readonly DbgRemoteFieldCollection Empty = new DbgRemoteFieldCollection(Array.Empty<DbgRemoteField>());

        private IList<DbgRemoteField> fields;

        public int Count => fields.Count;

        public DbgRemoteFieldCollection(IList<DbgRemoteField> fields)
        {
            this.fields = fields;
        }

        public DbgRemoteField this[string name]
        {
            get
            {
                var match = fields.SingleOrDefault(f => f.Name == name);

                return match;
            }
        }

        public IEnumerator<DbgRemoteField> GetEnumerator() => fields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}