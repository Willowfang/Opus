namespace Opus.Common.Extensions
{
    /// <summary>
    /// Extension methods for objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// If the object is not null, execute the delegate.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">Object to check.</param>
        /// <param name="action">Delegate to run if conditions are met.</param>
        /// <returns>The original object for chaining.</returns>
        public static T IfNotNullExecute<T>(this T obj, Action<T> action)
        {
            if (obj != null) action(obj);

            return obj;
        }

        /// <summary>
        /// If the object is null, execute the delegate.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">Object to check.</param>
        /// <param name="action">Delegate to run if conditions are met.</param>
        /// <returns>The original object for chaining.</returns>
        public static T IfNullExecute<T>(this T obj, Action<T> action)
        {
            if (obj == null) action(obj);

            return obj;
        }
    }
}
