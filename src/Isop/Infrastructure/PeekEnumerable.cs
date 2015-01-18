using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Isop.Infrastructure
{
    public class PeekEnumerable<T>:IEnumerable<T>
    {
        public PeekEnumerable(IEnumerable<T> enumerable)
        {
            _buffer = enumerable.ToList();
        }
        private int _currentIndex = -1;
        private readonly List<T> _buffer;
        public T Current()
        {
            if (_currentIndex < _buffer.Count())
            {
                return _buffer[_currentIndex];
            }
            throw new ArgumentOutOfRangeException();
        }
        public bool HasMore() { return _currentIndex+1<_buffer.Count(); }
        public T Next()
        {
            _currentIndex++;
            return Current();
        }

        public T Peek()
        {
             var idx = _currentIndex+1; 
             return idx<_buffer.Count() ? _buffer[idx] : default(T);
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
    