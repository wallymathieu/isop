using System.Collections;

namespace Isop.Infrastructure;
internal sealed class PeekEnumerable<T>(IReadOnlyList<T> buffer) : IEnumerable<T>
{
    private int _currentIndex = -1;

    public T Current()
    {
        if (_currentIndex < buffer.Count)
        {
            return buffer[_currentIndex];
        }
        throw new InvalidOperationException();
    }
    public bool HasMore() { return _currentIndex + 1 < buffer.Count; }
    public T Next()
    {
        _currentIndex++;
        return Current();
    }

    public T? Peek()
    {
        var idx = _currentIndex + 1;
        return idx < buffer.Count ? buffer[idx] : default;
    }
    public IEnumerator<T> GetEnumerator() => buffer.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
