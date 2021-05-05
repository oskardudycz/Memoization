using System;
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
