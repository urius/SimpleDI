using System;

namespace SimpleDI
{
    public class TypeOf<T>
    {
        public static readonly string Name = typeof(T).Name;
        public static readonly Type Type = typeof(T);
    }
}