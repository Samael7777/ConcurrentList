using System.ComponentModel;

namespace Samael.Collections
{
    /// <summary>
    /// Provides data for the <see cref="INotifyCollectionItemChanged" /> event.
    /// </summary>
    public class CollectionItemChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemChangedEventArgs" /> class.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propertyName">The name of the property that changed</param>
        public CollectionItemChangedEventArgs(object item, string propertyName) : base(propertyName)
        {
            Item = item;
        }

        /// <summary>
        /// Element of collection, that been changed
        /// </summary>
        public virtual object Item { get; }
    }
}


