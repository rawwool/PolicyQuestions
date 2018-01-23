﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Utility
{
    public static class Extension
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static List<Tuple<string, string>> GetEnumNameDescriptionAttribueValuePairs<T>() where T : struct
        {
            return Enum.GetNames(typeof(T))
                .Select(s =>
                {
                    Type type = typeof(T);
                    var memInfo = type.GetMember(s);
                    var attributes = memInfo[0].GetCustomAttributes(typeof(Attribute), false);
                    return new Tuple<string, string>(s, (attributes.Length > 0) ? ((DescriptionAttribute)attributes[0]).Description : "");

                })
                .ToList();
        }
    }
}
