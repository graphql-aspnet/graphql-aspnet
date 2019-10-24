// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// A set of common helper methods for data validation.
    /// </summary>
    [DebuggerStepThrough]
    public static class Validation
    {
        /// <summary>
        /// Throws an exception if the type argument is not castable to the <see><cref>TExpectedType</cref></see>.
        /// </summary>
        /// <typeparam name="TExpectedType">The type that the parameter is expected to be cast to.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNotCastable<TExpectedType>(Type type, string parameterName)
            where TExpectedType : class
        {
            ThrowIfNull(type, parameterName);
            if (!IsCastable<TExpectedType>(type))
                throw new ArgumentException($"Type {type.Name} is not castable to type {typeof(TExpectedType).FriendlyName()}");
        }

        /// <summary>
        /// Determines if the supplied type is castable to the Expected Type.
        /// </summary>
        /// <typeparam name="TExpectedType">The type that the parameter is expected to be cast to.</typeparam>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the specified type is castable; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCastable<TExpectedType>(Type type)
            where TExpectedType : class
        {
            return IsCastable(type, typeof(TExpectedType));
        }

        /// <summary>
        /// Determines if the supplied type is castable to the Expected Type.
        /// </summary>
        /// <param name="type">The type to check if it can be cast.</param>
        /// <param name="expectedType">The expected type to be casted to.</param>
        /// <returns><c>true</c> if the specified type is castable; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCastable(Type type, Type expectedType)
        {
            if (expectedType == null)
                return false;

            if (type == null)
                return false;

            if (type == expectedType)
                return true;

            return expectedType.IsAssignableFrom(type);
        }

        /// <summary>
        /// Throws an exception if the string is empty or null.
        /// </summary>
        /// <param name="text">The actual text to check.</param>
        /// <param name="parameterName">Name of the parameter being checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullWhiteSpace(string text, string parameterName)
        {
            ThrowIfNullEmpty(text, parameterName);
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"Parameter cannot be null, empty or whitespace.", parameterName);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException" /> if the string is empty or null.
        /// Returns the text as is or trimmed if no error occurs.
        /// </summary>
        /// <param name="text">The actual text to check.</param>
        /// <param name="parameterName">Name of the parameter being checked.</param>
        /// <param name="trimText">if set to <c>true</c> the provided text is trimmed before returning, otherwise returned as is.</param>
        /// <returns>System.String.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ThrowIfNullWhiteSpaceOrReturn(string text, string parameterName, bool trimText = true)
        {
            ThrowIfNullWhiteSpace(text, parameterName);
            return trimText ? text.Trim() : text;
        }

        /// <summary>
        /// Throws an if the string is empty or null.
        /// </summary>
        /// <param name="text">The actual text to check.</param>
        /// <param name="parameterName">Name of the parameter being checked.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullEmpty(string text, string parameterName)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException($"Parameter cannot be null or empty.", parameterName);
        }

        /// <summary>
        /// Throws if null empty or return.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="trimText">if set to <c>true</c> the text is trimmed before its returned.</param>
        /// <returns>System.String.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ThrowIfNullEmptyOrReturn(string text, string parameterName, bool trimText = true)
        {
            ThrowIfNullEmpty(text, parameterName);
            return trimText ? text.Trim() : text;
        }

        /// <summary>
        /// Throws an if the object reference is null.
        /// </summary>
        /// <param name="obj">The object refrence to check.</param>
        /// <param name="parameterName">Name of the parameter being checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull(object obj, string parameterName)
        {
            if (obj == null)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Throws an exception if the object reference is null.
        /// The object is returned unchanged if it exists.
        /// </summary>
        /// <typeparam name="T">Any reference type to be null checked.</typeparam>
        /// <param name="obj">The object refrence to check.</param>
        /// <param name="parameterName">Name of the parameter being checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(T obj, string parameterName)
        {
            if (obj == null)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Throws an exception if the object reference is null.
        /// The object is returned unchanged if it exists.
        /// </summary>
        /// <typeparam name="T">Any reference type to be null checked.</typeparam>
        /// <param name="obj">The object refrence to check.</param>
        /// <param name="parameterName">Name of the parameter being checked.</param>
        /// <returns>The object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfNullOrReturn<T>(T obj, string parameterName)
        {
            ThrowIfNull(obj, parameterName);
            return obj;
        }

        /// <summary>
        /// Throws an exception if the provided value is less than one.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">Name of the parameter.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfLessThanOne(int value, string paramName)
        {
            if (value < 1)
                throw new ArgumentException("Value cannot be less than 1", paramName);
        }

        /// <summary>
        /// Throws an exception if the provided value is less than one
        /// otherwise returns it untouched.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>System.Int32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ThrowIfLessThanOneOrReturn(int value, string paramName)
        {
            ThrowIfLessThanOne(value, paramName);
            return value;
        }

        /// <summary>
        /// Determines whether the given type is a <see cref="Nullable{T}"/> struct ref.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is nullable of t] [the specified type]; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullableOfT(Type type)
        {
            ThrowIfNull(type, nameof(type));
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Determines whether the specified type is an enumeration.
        /// </summary>
        /// <param name="enumType">The type to examine.</param>
        /// <returns><c>true</c> if the specified type is an enumeration; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnumeration(Type enumType)
        {
            if (enumType == null)
                return false;

            return enumType.IsEnum;
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException" /> if the provided type is not an enumeration.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>Type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ThrowIfNullOrNotEnumOrReturn(Type enumType, string parameterName)
        {
            Validation.ThrowIfNull(enumType, parameterName);

            if (!IsEnumeration(enumType))
                throw new ArgumentException($"The type '{enumType.Name}' is not an enumeration.", parameterName);

            return enumType;
        }
    }
}