using System;

namespace ChaosLib
{
    [AttributeUsage(AttributeTargets.Property)]
    class ArgumentAttribute : Attribute
    {
        public string Name { get; }

        public ArgumentAttribute(string name)
        {
            Name = name;
        }
    }
}