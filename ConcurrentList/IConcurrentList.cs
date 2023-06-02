using System.Collections.Generic;

namespace ConcurrentList
{
    public interface IConcurrentList<T> : IList<T>
    {
        void AddRange(IEnumerable<T> collection);
    }
}