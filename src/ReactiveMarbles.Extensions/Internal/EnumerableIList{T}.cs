using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ReactiveMarbles.Extensions.Internal
{
    internal readonly struct EnumerableIList<T> : IEnumerableIList<T>, IList<T>
    {
        private readonly IList<T> _list;

        public EnumerableIList(IList<T> list)
        {
            _list = list;
        }

        public static EnumerableIList<T> Empty { get; }

        /// <inheritdoc />
        public int Count => _list.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _list.IsReadOnly;

        /// <inheritdoc />
        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public static implicit operator EnumerableIList<T>(List<T> list) => new(list);

        public static implicit operator EnumerableIList<T>(T[] array) => new(array);

        public EnumeratorIList<T> GetEnumerator() => new(_list);

        /// <inheritdoc />
        public void Add(T item) => _list.Add(item);

        /// <inheritdoc />
        public void Clear() => _list.Clear();

        /// <inheritdoc />
        public bool Contains(T item) => _list.Contains(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public int IndexOf(T item) => _list.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, T item) => _list.Insert(index, item);

        /// <inheritdoc />
        public bool Remove(T item) => _list.Remove(item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    }
}
