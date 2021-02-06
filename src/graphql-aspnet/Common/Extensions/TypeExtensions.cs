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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for working with .NET <see cref="Type"/>
    /// values.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// <para>A list of known short hand aliases for commong types.</para>
        /// <para>Adapted from: https://stackoverflow.com/a/1362899. </para>
        /// </summary>
        private static readonly Dictionary<Type, string> TYPE_ALIAS_NAMES = new Dictionary<Type, string>()
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(void), "void" },
            { typeof(byte?), "byte?" },
            { typeof(sbyte?), "sbyte?" },
            { typeof(short?), "short?" },
            { typeof(ushort?), "ushort?" },
            { typeof(int?), "int?" },
            { typeof(uint?), "uint?" },
            { typeof(long?), "long?" },
            { typeof(ulong?), "ulong?" },
            { typeof(float?), "float?" },
            { typeof(double?), "double?" },
            { typeof(decimal?), "decimal?" },
            { typeof(bool?), "bool?" },
            { typeof(char?), "char?" },
            { typeof(DateTime?), "DateTime?" },
        };

        private static readonly ConcurrentDictionary<Tuple<Type, bool, int>, bool> VALIDATION_SCANS;

        /// <summary>
        /// Initializes static members of the <see cref="TypeExtensions"/> class.
        /// </summary>
        static TypeExtensions()
        {
            VALIDATION_SCANS = new ConcurrentDictionary<Tuple<Type, bool, int>, bool>();
        }

        /// <summary>
        /// Returns a single attribute of a given type or null. If the type declares more than one instance
        /// of the attribute type, null is returned.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
        /// <param name="type">The type to inspect.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute..</param>
        /// <returns>TAttribute.</returns>
        public static TAttribute SingleAttributeOrDefault<TAttribute>(this ICustomAttributeProvider type, bool inherit = false)
            where TAttribute : Attribute
        {
            if (type == null)
                return null;

            var attribs = type.GetCustomAttributes(typeof(TAttribute), inherit).Where(x => x.GetType() == typeof(TAttribute)).Take(2);
            if (attribs.Count() == 1)
                return attribs.Single() as TAttribute;

            return null;
        }

        /// <summary>
        /// Returns a single attribute that is castable to the given type or null. If the type declares more than one instance
        /// that matches the type condition, null is returned.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
        /// <param name="type">The type to inspect.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute..</param>
        /// <returns>TAttribute.</returns>
        public static TAttribute SingleAttributeOfTypeOrDefault<TAttribute>(this ICustomAttributeProvider type, bool inherit = false)
            where TAttribute : Attribute
        {
            if (type == null)
                return null;

            var attribs = type.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>().Take(2);
            if (attribs.Count() == 1)
                return attribs.Single();

            return null;
        }

        /// <summary>
        /// Determines if the given type had the attribute defined at least once.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute..</param>
        /// <returns>TAttribute.</returns>
        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider type, bool inherit = false)
            where TAttribute : Attribute
        {
            return type.HasAttribute(typeof(TAttribute), inherit);
        }

        /// <summary>
        /// Determines if the given type had the attribute defined at least once.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute..</param>
        /// <returns>TAttribute.</returns>
        public static bool HasAttribute(this ICustomAttributeProvider type, Type attributeType, bool inherit = false)
        {
            if (type == null || attributeType == null || !Validation.IsCastable<Attribute>(attributeType))
                return false;

            return type.IsDefined(attributeType, inherit);
        }

        /// <summary>
        /// Determines whether the given type represents a <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the type of a <see cref="Nullable{T}"/>; otherwise, <c>false</c>.</returns>
        public static bool IsNullableOfT(this Type type)
        {
            if (type == null)
                return false;

            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Determines whether the given type is, or can be coerced into, <see cref="IEnumerable{T}"/> then checks the
        /// generic type T to ensure if it is <see cref="Nullable{K}"/>. T and K are arbitrary in this scenario.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type represents a nullable IEnumerable; otherwise, <c>false</c>.</returns>
        public static bool IsNullableEnumerableOfT(this Type type)
        {
            if (type == null)
                return false;

            // if the type implements IEnumerable<> grab that interface then check its generic argument
            return type.GetEnumerableUnderlyingType()?.IsNullableOfT() ?? false;
        }

        /// <summary>
        /// If the provided type implements 'IEnumerable[T]', this method returns the 'T' type. Returns
        /// null otherwise.
        /// </summary>
        /// <param name="type">The type to extract from.</param>
        /// <param name="resolveAllEnumerables">if set to <c>true</c> any and all nested Enumerables will be resolved down to the base type. (e.g. IEnumerable[IEnumerable[T]] => 'T').</param>
        /// <returns>The 'T' of IEnumerable[T].</returns>
        public static Type GetEnumerableUnderlyingType(this Type type, bool resolveAllEnumerables = false)
        {
            if (type == null)
                return null;

            // short circut string as a common convention
            if (type == typeof(string))
                return type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return resolveAllEnumerables
                    ? type.GetGenericArguments()[0].GetEnumerableUnderlyingType(true)
                    : type.GetGenericArguments()[0];
            }

            // if the type implements IEnumerable<> grab that interface then return its argument
            var enumerableInterface = type.GetInterface(typeof(IEnumerable<>).Name);
            if (enumerableInterface != null)
            {
                return resolveAllEnumerables
                    ? enumerableInterface.GetGenericArguments()[0].GetEnumerableUnderlyingType(true)
                    : enumerableInterface.GetGenericArguments()[0];
            }

            return type;
        }

        /// <summary>
        /// Gets the value type of dictionary; i.e. the "K" in IDictionary{T, K}.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public static Type GetValueTypeOfDictionary(this Type type)
        {
            var enumeratedType = type.GetEnumerableUnderlyingType();

            if (enumeratedType == null || !enumeratedType.IsGenericType || typeof(KeyValuePair<,>) != enumeratedType.GetGenericTypeDefinition())
                return null;

            var paramSet = enumeratedType.GetGenericArguments();
            return paramSet[1];
        }

        /// <summary>
        /// Gets the key type of the dictionary; i.e. the "T" in IDictionary{T, K}.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public static Type GetKeyTypeOfDictionary(this Type type)
        {
            var enumeratedType = type.GetEnumerableUnderlyingType();

            if (enumeratedType == null || !enumeratedType.IsGenericType || typeof(KeyValuePair<,>) != enumeratedType.GetGenericTypeDefinition())
            {
                return null;
            }

            var paramSet = enumeratedType.GetGenericArguments();
            if (paramSet.Length != 2)
                return null;

            return paramSet[0];
        }

        /// <summary>
        /// Attempts to create a friendly name to represent the type accounting for nested generic arguments.
        /// (i.e. 'IEnumerable&lt;int&gt;' instead of 'IEnumerable`1').
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fullName">if set to <c>true</c> The full name (including namespace) of the type
        /// will be returned; otherwise, just the type name will be returned. Type aliases will not be
        /// used when the fully qualified typename is returned.</param>
        /// <returns>System.String.</returns>
        public static string FriendlyName(this Type type, bool fullName = false)
        {
            return FriendlyName(type, "<", ">", ", ", fullName);
        }

        /// <summary>
        /// Attempts to create a friendly name to represent the type accounting for nested generic arguments.
        /// using the provided delimiter for both left and right sides of the generic argument set. (i.e. 'IEnumerable_int_' instead of 'IEnumerable&lt;int&gt;').
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="fullName">if set to <c>true</c> The full name (including namespace) of the type
        /// will be returned; otherwise, just the type name will be returned. Type aliases will not be
        /// used when the fully qualified typename is returned.</param>
        /// <returns>System.String.</returns>
        public static string FriendlyName(this Type type, string delimiter, bool fullName = false)
        {
            return FriendlyName(type, delimiter, delimiter, delimiter, fullName);
        }

        /// <summary>
        /// Attempts to create a friendly name to represent the type accounting for nested generic arguments.
        /// using the provided left and rigth delimiters.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="leftDelimiter">The left generic delimiter to use in the output.</param>
        /// <param name="rightDelimiter">The right generic delimiter to use in the output.</param>
        /// <param name="typeJoiner">The phrase to use to join arguments on multi generic types..</param>
        /// <param name="fullName">if set to <c>true</c> The full name (including namespace) of the type
        /// will be returned; otherwise, just the type name will be returned. Type aliases will not be
        /// used when the fully qualified typename is returned.</param>
        /// <returns>System.String.</returns>
        public static string FriendlyName(this Type type, string leftDelimiter, string rightDelimiter, string typeJoiner, bool fullName = false)
        {
            if (type == null)
                return string.Empty;

            string typeName = null;
            if (!fullName && TYPE_ALIAS_NAMES.ContainsKey(type))
            {
                typeName = TYPE_ALIAS_NAMES[type];
            }
            else
            {
                if (!type.IsGenericType)
                {
                    typeName = type.Name;
                }
                else
                {
                    var genericParamNames = type
                        .GetGenericArguments()
                        .Select(x => x.FriendlyName(leftDelimiter, rightDelimiter, typeJoiner, fullName)).ToArray();
                    typeName = type.Name.Replace(
                        $"`{genericParamNames.Length}",
                        string.Format(
                            "{0}{1}{2}",
                            leftDelimiter,
                            string.Join(typeJoiner, genericParamNames),
                            rightDelimiter));
                }
            }

            return fullName ? $"{type.Namespace}.{typeName}" : typeName;
        }

        /// <summary>
        /// Determines whether the given type has at least one validation attribute on one of its properties.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="inspectChildProperties">if set to <c>true</c> validation attributes of contained property types will also be checked.</param>
        /// <param name="maxDepth">The maximum depth into the object tree to be inspected.</param>
        /// <returns><c>true</c> if a validation attribute was found, <c>false</c> otherwise.</returns>
        public static bool RequiresValidation(this Type type, bool inspectChildProperties = true, int maxDepth = 5)
        {
            if (type == null || maxDepth <= 0)
                return false;

            Tuple<Type, bool, int> key = Tuple.Create(type, inspectChildProperties, maxDepth);

            if (VALIDATION_SCANS.TryGetValue(key, out var result))
                return result;

            result = InspectTypeForValidationAttribute(type, inspectChildProperties, maxDepth);
            VALIDATION_SCANS.TryAdd(key, result);
            return result;
        }

        /// <summary>
        /// Internal recursive method for walking a type and checking for validation attributes.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="inspectChildProperties">if set to <c>true</c> validation attributes of contained property types will also be checked.</param>
        /// <param name="maxDepth">The maximum depth into the object tree to be inspected.</param>
        /// <returns><c>true</c> if a validation attribute was found, <c>false</c> otherwise.</returns>
        private static bool InspectTypeForValidationAttribute(Type type, bool inspectChildProperties, int maxDepth)
        {
            if (!type.IsClass)
                return false;

            if (type == typeof(string))
                return false;

            if (maxDepth <= 0)
                return false;

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var hasValidationAttribs = prop.GetCustomAttributes().Any(x => x is ValidationAttribute);
                if (hasValidationAttribs)
                    return true;

                if (inspectChildProperties
                    && InspectTypeForValidationAttribute(prop.PropertyType, true, maxDepth - 1))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Extracts the generic parameters of the given type as a list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type[].</returns>
        public static IEnumerable<Type> ExtractGenericParameters(this Type type)
        {
            if (type == null || !type.IsGenericType)
                return Enumerable.Empty<Type>();

            return type.GetGenericArguments();
        }

        /// <summary>
        /// Determines whether the given <paramref name="type"/> is
        /// both an enum and that its underlying type is an unsigned numeric
        /// type. (e.g. uint, ulong etc.)
        /// </summary>
        /// <param name="type">The type toinspect.</param>
        /// <returns>bool.</returns>
        public static bool IsEnumOfUnsignedNumericType(this Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException("type must be an enum", nameof(type));

            var underlyingType = Enum.GetUnderlyingType(type);

            return
                underlyingType == typeof(byte) || // 8bit
                underlyingType == typeof(ushort) || // 16bit
                underlyingType == typeof(uint) || // 32bit
                underlyingType == typeof(ulong); // 64 bit
        }
    }
}