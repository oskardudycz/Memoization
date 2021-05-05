using System;
using System.Collections.Concurrent;

namespace Memoization.ThreadSafe
{
    public static class ThreadSafeMemoizer
    {
        /// <summary>
        /// Memoizes provided function. Function should provide deterministic results.
        /// For the same input it should return the same result.
        /// Memoized function for the specific input will be called once, further calls will use cache.
        /// </summary>
        /// <param name="func">function to be memoized</param>
        /// <typeparam name="TInput">Type of the function input value</typeparam>
        /// <typeparam name="TResult">Type of the function result</typeparam>
        /// <returns></returns>
        public static Func<TInput, TResult> Memoize<TInput, TResult>(this Func<TInput, TResult> func)
        {
            // create cache ("memo")
            var memo = new ConcurrentDictionary<TInput, TResult>();

            // wrap provided function with cache handling
            // get a value from cache if it exists
            // if not, call factory method
            // ConcurrentDictionary will handle that internally
            return input => memo.GetOrAdd(input, func);
        }
    }
}