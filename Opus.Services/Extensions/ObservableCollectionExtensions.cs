using System;
using System.Collections.ObjectModel;

namespace Opus.Services.Extensions
{
    /// <summary>
    /// Extension methods for observablecollections.
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Remove all instances in the collection matching the predicate.
        /// </summary>
        /// <typeparam name="T">Type of the collection instances.</typeparam>
        /// <param name="collection">Collection to search for.</param>
        /// <param name="condition">Condition for removal.</param>
        public static void RemoveAll<T>(
            this ObservableCollection<T> collection,
            Func<T, bool> condition
        )
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
    }
}
