using System;
using System.ComponentModel;
using System.Linq;

namespace ChaosLib
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum element, bool toStringFallback = true)
        {
            if (TryGetDescription(element, out var description))
                return description;

            if (toStringFallback)
                return element.ToString();

            throw new InvalidOperationException($"{element} is missing a {nameof(DescriptionAttribute)}");
        }

        public static bool TryGetDescription(this Enum element, out string description)
        {
            var memberInfo = element.GetType().GetMember(element.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo.First().GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    description = ((DescriptionAttribute)attributes.First()).Description;
                    return true;
                }
            }

            description = null;
            return false;
        }
    }
}
