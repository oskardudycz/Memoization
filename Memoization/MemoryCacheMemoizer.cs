using System;
using Microsoft.Extensions.Caching.Memory;

namespace Memoization.MemCache
{
    public static class MemoryCacheMemoizer
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
            var memo = new MemoryCache(
                new MemoryCacheOptions
                {
                    // Set cache size limit.
                    // Note: this is not size in bytes,
                    // but sum of all entries' sizes.
                    // Entry size is declared on adding to cache
                    // in the factory method 
                    SizeLimit = 100
                });
            
            // wrap provided function with cache handling
            // get a value from cache if it exists
            // if not, call factory method
            // MemCache will handle that internally
            return input => memo.GetOrCreate(input, entry =>
            {
                // you can set different options like e.g.
                // sliding expiration - time between now and last time
                // and the last time the entry was accessed 
                entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                
                // this value is used to calculate total SizeLimit
                entry.Size = 1;
                return func(input);
            });
        }
    }
}