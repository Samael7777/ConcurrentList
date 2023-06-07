using System.Collections.Generic;

namespace Samael.Collections
{
    public interface IConcurrentList<T> : IList<T>
    {
        void AddRange(IEnumerable<T> collection);
    }
}