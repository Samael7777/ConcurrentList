using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Samael.Collections
{
    public class ObservableConcurrentList<T> : ConcurrentBase<T>, IObservableConcurrentList<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollectionItemChangedEventHandler CollectionItemChanged;

        #region Constructors
        
        public ObservableConcurrentList() : base(new List<T>())
        { }

        public ObservableConcurrentList(int capacity) : base(new List<T>(capacity))
        { }

        public ObservableConcurrentList(IEnumerable<T> collection) : this()
        {
            AddRange(collection);
        }
        #endregion

        ///<inheritdoc cref="List{T}.AddRange"/>
        public void AddRange(IEnumerable<T> collection)
        {
            ConcurrentAction(list =>
            {
                foreach (var item in collection)
                {
                    list.Add(item);
                    AddListener(item);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
            });
        }

        public virtual T this[int index]
        {
            get => ConcurrentFunc(list => list[index]);
            set => ConcurrentAction(list =>
            {
                var oldItem = list[index];

                if (ReferenceEquals(oldItem, value)) 
                    return;
                    
                RemoveListener(oldItem);
                AddListener(value);
                list[index] = value;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));

            });
        }

        public int Count =>
            ConcurrentFunc(list => list.Count);

        public bool IsReadOnly =>
            ConcurrentFunc(list => list.IsReadOnly);

        public virtual void Add(T item) =>
            ConcurrentAction(list =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                list.Add(item);
                AddListener(item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item));
            });

        public virtual void Clear() =>
            ConcurrentAction(list =>
            {
                foreach (var item in list)
                {
                    RemoveListener(item);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });

        public bool Contains(T item) =>
            ConcurrentFunc(list => list.Contains(item));

        public void CopyTo(T[] array, int arrayIndex) =>
            ConcurrentAction(l => l.EnterReadLock(),
            l => l.EnterReadLock(),
            list => list.CopyTo(array, arrayIndex));

        public int IndexOf(T item) =>
            ConcurrentFunc(list => list.IndexOf(item));

        public virtual void Insert(int index, T item) =>
            ConcurrentAction(list =>
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                AddListener(item);
                list.Insert(index, item);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, index));
            });

        public virtual bool Remove(T item) =>
            ConcurrentFunc(l => l.EnterWriteLock(),
                l => l.ExitWriteLock(),
                list =>
                {
                    if (item == null || !list.Contains(item)) 
                        return false;

                    RemoveListener(item);
                    var result = list.Remove(item);
                    
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                    return result;
                });

        public virtual void RemoveAt(int index) =>
            ConcurrentAction(list =>
            {
                var oldItem = list[index];
                RemoveListener(oldItem);
                list.RemoveAt(index);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, oldItem, index));
            });

        public IEnumerator<T> GetEnumerator() => GetConcurrentEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        private void OnCollectionItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            var args = new CollectionItemChangedEventArgs(sender, e.PropertyName);
            CollectionItemChanged?.Invoke(this, args);
        }

        private void AddListener(T item)
        {
            if (item is INotifyPropertyChanged observable)
            {
                observable.PropertyChanged += OnCollectionItemChanged;
            }
        }

        private void RemoveListener(T item)
        {
            if (item is INotifyPropertyChanged observable)
            {
                observable.PropertyChanged -= OnCollectionItemChanged;
            }
        }
    }
}