using System;

namespace ChaosLib
{
    [AttributeUsage(AttributeTargets.Property)]
    class OptionAttribute : Attribute
    {
        public string Name { get; }

        public OptionAttribute(string name)
        {
            Name = name;
        }
    }
}