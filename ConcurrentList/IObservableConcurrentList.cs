using System.Collections.Specialized;

namespace Samael.Collections
{
    public delegate void NotifyCollectionItemChangedEventHandler(object sender, CollectionItemChangedEventArgs e);

    public interface IObservableConcurrentList<T> : IConcurrentList<T>, INotifyCollectionChanged
    {
        event NotifyCollectionItemChangedEventHandler CollectionItemChanged;
    }
}