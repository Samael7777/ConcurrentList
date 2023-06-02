using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentList
{
    internal class ConcurrentEnumerator<T> : IEnumerator<T>
    {
        private readonly ReaderWriterLockSlim _lock;
        private readonly IEnumerator<T> _enumerator;

        public ConcurrentEnumerator(IEnumerable<T>source, ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _lock.EnterReadLock();
            _enumerator = source.GetEnumerator();
        }

        public bool MoveNext() => _enumerator.MoveNext();

        public void Reset() => _enumerator.Reset();

        public T Current => _enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerator.Dispose();
            _lock.ExitReadLock();
        }
    }
}