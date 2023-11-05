﻿using System;
using System.ComponentModel;
using System.Linq;

namespace ChaosLib
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum element, bool toStringFallback = true)
        {
            var memberInfo = element.GetType().GetMember(element.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo.First().GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    return ((DescriptionAttribute)attributes.First()).Description;
                }
            }

            if (toStringFallback)
                return element.ToString();

            throw new InvalidOperationException($"{element} is missing a {nameof(DescriptionAttribute)}");
        }
    }
}