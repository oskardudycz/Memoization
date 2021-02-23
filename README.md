![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/)

# Memoization

Memoization is a useful technique that allows easily optimize method calls. The sample shows how the memoization works and how to implement it in C#.

It takes a function and wraps it in the method to check if the provided input function was already called. If yes, then it will return value from the cache. If not - it'll return the cached value without running the function.

This works best for the method that are called numerous times. Memoized function should provide deterministic results. For the same input, it should return the same result and do not create side-effects (is a _["Higher-order function"](https://en.wikipedia.org/wiki/Higher-order_function)_).

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Memoization
{
    public static class Memoizer
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
            var memo = new Dictionary<TInput, TResult>();

            // wrap provided function with cache handling
            return input =>
            {
                // check if result for set input was already cached
                if (memo.TryGetValue(input, out var fromMemo))
                    // if yes, return value
                    return fromMemo;

                // if no, call function
                var result = func(input);
                
                // cache the result
                memo.Add(input, result);

                // return result
                return result;
            };
        }
    }
	
    class Program
    {
        static void Main(string[] args)
        {
            Func<Type, Type, bool> hasAttribute =
                (type, attributeType) => type.GetCustomAttributes(attributeType, true).Any();

            Func<Type, bool> hasSomeCustomAttribute = 
                type => hasAttribute(type, typeof(SomeCustomAttribute));
            
            Func<Type, bool> hasSomeCustomAttributeMemo = hasSomeCustomAttribute.Memoize();
            
            // Function will be called, as result wasn't memoized
            hasSomeCustomAttributeMemo(typeof(FirstTypeWithAttribute));
            // Function will be called, as this is not memoized function
            hasSomeCustomAttribute(typeof(FirstTypeWithAttribute));
            // Function will NOT be called, as result was memoized
            hasSomeCustomAttributeMemo(typeof(FirstTypeWithAttribute));
            
            // Function will be called, as we memoized result of the different input value (other attribute)
            hasSomeCustomAttributeMemo(typeof(SecondTypeWithAttribute));
            // Function will be called, as this is not memoized function
            hasSomeCustomAttribute(typeof(SecondTypeWithAttribute));
            // Function will NOT be called, as result was memoized
            hasSomeCustomAttributeMemo(typeof(SecondTypeWithAttribute));
        }
    }

    [SomeCustomAttribute]
    public class FirstTypeWithAttribute
    {
        
    }
    
    [SomeCustomAttribute]
    public class SecondTypeWithAttribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class)  
    ]  
    public class SomeCustomAttribute : Attribute
    {
        
    }
}

```
See the [Program.cs](./Memoization/Program.cs) to debug the code.

## TO DO
- [x] - basic sample and introduction
- [ ] - thread-safe sample using `ConcurrentDictionary`
- [ ] - Use generators or currying techniques
- [ ] - Sample in JavaScript/TypeScript
