using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentList
{
    public class ConcurrentList<T> : IConcurrentList<T>
    {
        private readonly IList<T> _internalList;
        protected readonly ReaderWriterLockSlim Lock;

        #region Constructors

        private ConcurrentList(IList<T> list)
        {
            _internalList = list;
            Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }
        
        public ConcurrentList() : this(new List<T>())
        { }
        
        public ConcurrentList(int capacity) : this(new List<T>(capacity))
        { }

        public ConcurrentList(IEnumerable<T> list) : this(new List<T>())
        {
            foreach (var item in list)
            {
                _internalList.Add(item);
            }
        }
        #endregion

        ///<inheritdoc cref="List{T}.AddRange"/>
        public virtual void AddRange(IEnumerable<T> collection)
        {
            ConcurrentAction(list =>
            {
                foreach (var item in collection)
                {
                    _internalList.Add(item);
                }
            });
        }

        public virtual T this[int index]
        {
            get => ConcurrentFunc(list => list[index]);
            set => ConcurrentAction(list => { list[index] = value; });
        }

        public int Count => 
            ConcurrentFunc(list => list.Count);

        public bool IsReadOnly => 
            ConcurrentFunc(list => list.IsReadOnly);

        public virtual void Add(T item) => 
            ConcurrentAction(list => list.Add(item));
       
        public virtual void Clear() => 
            ConcurrentAction(list=>list.Clear());

        public bool Contains(T item) => 
            ConcurrentFunc(list => list.Contains(item));

        public void CopyTo(T[] array, int arrayIndex) => 
            ConcurrentAction(l => l.EnterReadLock(),
            l => l.EnterReadLock(), 
            list => list.CopyTo(array, arrayIndex));

        public int IndexOf(T item) => 
            ConcurrentFunc(list => list.IndexOf(item));

        public virtual void Insert(int index, T item) => 
            ConcurrentAction(list => list.Insert(index, item));

        public virtual bool Remove(T item) =>
            ConcurrentFunc(l => l.EnterWriteLock(),
                l => l.ExitWriteLock(),
                list => list.Remove(item));
        
        public virtual void RemoveAt(int index) => 
            ConcurrentAction(list => list.RemoveAt(index));

        public IEnumerator<T> GetEnumerator() => new ConcurrentEnumerator<T>(_internalList, Lock);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Concurrent

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

        #endregion
    }
}