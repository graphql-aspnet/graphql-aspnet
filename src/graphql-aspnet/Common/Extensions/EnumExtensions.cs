// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// <para>A collection of helper methods for dealing with enumerations.</para>
    /// <para>Many have been adapted from Jeff Mercado's solution: https://stackoverflow.com/a/4171168 .</para>
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// <para>Gets the flags that are set on the enum value. Composite flags (those "or-ed" together) are held
        /// intact according to their definition in the enumeration definition.</para>
        /// </summary>
        /// <typeparam name="TEnum">The final enumeration type to cast the results to.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable&lt;Enum&gt;.</returns>
        public static IEnumerable<TEnum> GetFlags<TEnum>(this Enum value)
            where TEnum : Enum
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray()).Cast<TEnum>();
        }

        /// <summary>
        /// <para>Gets the individual flags set on the enum value. Any combined or composite flags
        /// are reduced to their individual settings.</para>
        /// </summary>
        /// <typeparam name="TEnum">The final enumeration type to cast the results to.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable&lt;Enum&gt;.</returns>
        public static IEnumerable<TEnum> GetIndividualFlags<TEnum>(this Enum value)
            where TEnum : Enum
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray()).Cast<TEnum>();
        }

        /// <summary>
        /// <para>Gets the flags set on the value which are contained in the provided values array.</para>
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="checkForValues">The values to check for.</param>
        /// <returns>IEnumerable&lt;Enum&gt;.</returns>
        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] checkForValues)
        {
            ulong bits = Convert.ToUInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = checkForValues.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(checkForValues[i]);
                if (i == 0 && mask == 0L)
                    break;

                if ((bits & mask) == mask)
                {
                    results.Add(checkForValues[i]);
                    bits -= mask;
                }
            }

            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && checkForValues.Length > 0 && Convert.ToUInt64(checkForValues[0]) == 0L)
                return checkForValues.Take(1);
            return Enumerable.Empty<Enum>();
        }

        /// <summary>
        /// <para>Gets the flag values available on the given enum type.</para>
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns>IEnumerable&lt;Enum&gt;.</returns>
        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);

                // skip the zero value
                if (bits == 0L)
                    continue;

                while (flag < bits)
                    flag <<= 1;

                if (flag == bits)
                    yield return value;
            }
        }
    }
}