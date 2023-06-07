using System;
using System.Collections.Generic;
using System.Threading;

namespace Samael.Collections
{
    public abstract class ConcurrentBase<T>
    {
        private readonly IList<T> _internalList;
        protected readonly ReaderWriterLockSlim Lock;

        protected ConcurrentBase(IList<T> list)
        {
            _internalList = list;
            Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        protected IEnumerator<T> GetConcurrentEnumerator() => new ConcurrentEnumerator<T>(_internalList, Lock);

        protected void ConcurrentAction(
            Action<ReaderWriterLockSlim> enter,
            Action<ReaderWriterLockSlim> exit,
            Action<IList<T>> action)
        {
            try
            {
                enter(Lock);
                action(_internalList);
            }
            finally
            {
                exit(Lock);
            }
        }

        protected TResult ConcurrentFunc<TResult>(
            Action<ReaderWriterLockSlim> enter,
            Action<ReaderWriterLockSlim> exit,
            Func<IList<T>, TResult> func)
        {
            try
            {
                enter(Lock);
                return func(_internalList);
            }
            finally
            {
                exit(Lock);
            }
        }

        protected TResult ConcurrentFunc<TResult>(Func<IList<T>, TResult> func) =>
            ConcurrentFunc(l => l.EnterReadLock(), l => l.ExitReadLock(), func);

        protected void ConcurrentAction(Action<IList<T>> action) =>
            ConcurrentAction(l => l.EnterWriteLock(), l => l.ExitWriteLock(), action);
    }
}


