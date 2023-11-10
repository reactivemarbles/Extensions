using System.Collections;
using System.Collections.Generic;

namespace ReactiveMarbles.Extensions.Internal
{
    internal struct EnumeratorIList<T> : IEnumerator<T>
    {
        private readonly IList<T> _list;

        private int _index;

        public EnumeratorIList(IList<T> list)
        {
            _index = -1;
            _list = list;
        }

        public T Current => _list[_index];

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;

            return _index < _list.Count;
        }

        public void Dispose()
        {
        }

        public void Reset() => _index = -1;
    }
}
