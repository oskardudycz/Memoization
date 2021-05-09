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

Of course, above implementation is quite naive, e.g. it's not thread-safe. We could enhance and simplify that by using [ConcurrentDictionary](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items) class:

```csharp
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
```

This version is still not perfect. When we are memoizing many items, our cache may grow exponentially and generate a memory usage issue. Generally, we'd like to keep in memory only the actively accessed entries. Not accessed, we can evict. We'd like to be able to set up a top limit of entries in our cache. To do that, we can use a [MemoryCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0) class. A sample implementation can look like this:

```csharp
public static Func<TInput, TResult> Memoize<TInput, TResult>(this Func<TInput, TResult> func)
{
    // create cache ("memo")
    var memo = new MemoryCache(new MemoryCacheOptions
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
```

In functional programming, recursion is a widespread practice. It's non-trivial as to understand recursion, you need to understand recursion. It can be computation expensive. How to use the Memoization with recursion? Let's take the Fibonacci sequence as an example. The rule is: the next number is found by adding up the two numbers before it.

```csharp
int Fibonacci(int n1)
{
    if (n1 <= 2)
        return 1;
                
    return Fibonacci(n1 -1) + Fibonacci(n1 - 2);
}
```

We'll need to find a way to inject the memoized version of the Fibonacci function. Let's start with breaking out function into the main and the overload:

```csharp
int Fibonacci(int n1)
{
    return Fibonacci(n1, Fibonacci);
}

int Fibonacci(int n1, Func<int, int> fibonacci)
{        
    if (n1 <= 2)
        return 1;
        
    return fibonacci(n1 -1) + fibonacci(n1 - 2);
}
```

Now instead of the direct self-call, we can inject the function to use while doing recursion. Now we have the possibility to memoize it by doing:

```csharp
Func<int, int> fibonacci = null;
            
fibonacci = Memoizer.Memoize((int n1)  => Fibonacci(n1, fibonacci));
            
var result = fibonacci(3);
```

The other way is to use the local function:

```csharp
Func<int, int> fibonacci = null;

fibonacci = n1 =>
{
    numberOfCalls++;
    
    if (n1 <= 2)
        return 1;
    
    return fibonacci(n1 - 1) + fibonacci(n1 - 2);
};

fibonacci = fibonacci.Memoize();


var result = fibonacci(3);
```

The trick is that the local _fibonacci_ function is lazily evaluated. That means that effectively it will use the assigned, memoized function while doing the call. I know that analyzing recursion can create a headache. It may be more accessible by debugging the tests:
- [Regular function](./Memoization.Tests/RecurrsionWithFunctionTests.cs)
- [Local function](./Memoization.Tests/RecurrsionWithLocalFunctionTests.cs)

See also:
- Simple implementation: [Memoizer.cs](./Memoization/Memoizer.cs)
- Thread-Safe implementation with [ConcurrentDictionary](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items): [ThreadSafeMemoizer.cs](./Memoization/ThreadSafeMemoizer.cs)
- Implementation with cache eviction with a [MemoryCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0): [MemoryCacheMemoizer.cs](./Memoization/MemoryCacheMemoizer.cs),
- the [Program.cs](./Memoization/Program.cs) to debug the code.

Read more in my article ["Memoization, a useful pattern for quick optimization"](https://event-driven.io/en/memoization_a_useful_pattern_for_quick_optimisation).

## TO DO
- [x] - basic sample and introduction
- [x] - thread-safe sample using `ConcurrentDictionary`
- [ ] - enhance the type check for reference type params
- [ ] - Use generators or currying techniques
- [ ] - Sample in JavaScript/TypeScript
