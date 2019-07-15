using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Isop.Infrastructure
{
    internal class PeekEnumerable<T>:IEnumerable<T>
    {
        public PeekEnumerable(IReadOnlyList<T> buffer)
        {
            _buffer = buffer;
        }
        private int _currentIndex = -1;
        private readonly IReadOnlyList<T> _buffer;
        public T Current()
        {
            if (_currentIndex < _buffer.Count())
            {
                return _buffer[_currentIndex];
            }
            throw new InvalidOperationException();
        }
        public bool HasMore() { return _currentIndex+1<_buffer.Count; }
        public T Next()
        {
            _currentIndex++;
            return Current();
        }

        public T Peek()
        {
             var idx = _currentIndex+1; 
             return idx<_buffer.Count ? _buffer[idx] : default;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
    