using System;
using System.Collections.Generic;
using System.Linq;

namespace Memoization
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<Type, Type, bool> hasAttribute =
                (type, attributeType) => type.GetCustomAttributes(attributeType, true).Any();

            Func<Type, bool> hasSomeCustomAttribute = 
                type => hasAttribute(type, typeof(SomeCustomAttribute));
            
            Func<Type, bool> hasSomeCustomAttributeMemo = hasSomeCustomAttribute.Memoize();
                
            hasSomeCustomAttributeMemo(typeof(FirstTypeWithAttribute));
            hasSomeCustomAttribute(typeof(FirstTypeWithAttribute));
            hasSomeCustomAttributeMemo(typeof(FirstTypeWithAttribute));
            
            hasSomeCustomAttributeMemo(typeof(SecondTypeWithAttribute));
            hasSomeCustomAttribute(typeof(SecondTypeWithAttribute));
            hasSomeCustomAttributeMemo(typeof(SecondTypeWithAttribute));
        }
    }

    public static class Memoizer
    {
        public static Func<TInput, TResult> Memoize<TInput, TResult>(this Func<TInput, TResult> func)
        {
            var memo = new Dictionary<TInput, TResult>();

            return input =>
            {
                if (memo.TryGetValue(input, out var fromMemo))
                    return fromMemo;

                var result = func(input);

                memo.Add(input, result);

                return result;
            };
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
