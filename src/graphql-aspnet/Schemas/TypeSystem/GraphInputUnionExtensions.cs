// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// Helper methods for use with <see cref="GraphInputUnion" /> metadata objects.
    /// </summary>
    public static class GraphInputUnionExtensions
    {
        private static readonly ConcurrentDictionary<(Type EntityType, Type ValueType), Delegate> SELECTOR_CACHE = new ConcurrentDictionary<(Type EntityType, Type ValueType), Delegate>();

        /// <summary>
        /// Returns the value of a indicated property on the <paramref name="entity" />. If the value of on the entity
        /// is null, the <paramref name="fallbackValue" /> value will be returned.
        /// </summary>
        /// <remarks>
        /// This method returns values of the type of <paramref name="fallbackValue" />. The
        /// type indicated on <paramref name="fallbackValue" /> must be compatiable to the target property value.
        /// For example, if the property returns a int? and the <paramref name="fallbackValue" /> returns an int. This method
        /// will not attempt to do numeric conversions (e.g. <c>int?</c> to <c>double</c>) but will honor type specificity and
        /// inheritance chains.
        /// </remarks>
        /// <param name="entity">The <see cref="GraphInputUnion" /> to operate on.</param>
        /// <param name="selector">The selector indicating which property to inspect.</param>
        /// <param name="fallbackValue">The fallback value to use if the target property was NOT supplied on the query.</param>
        /// <typeparam name="TType">The concrete type of <see cref="GraphInputUnion" /> being referenced.</typeparam>
        /// <typeparam name="TValue">
        /// The type value expected to be returned from the chosen property.
        /// </typeparam>
        /// <typeparam name="TReturn">
        /// The return type of this method. Generally, this may be a non-nullable
        /// type that is compatible with the nullable property of the union. (e.g. <c>int</c> for <c>int?</c>
        /// </typeparam>
        public static TReturn ValueOrDefault<TType, TValue, TReturn>(
            this TType entity,
            Expression<Func<TType, TValue>> selector,
            TReturn fallbackValue = default)
            where TType : GraphInputUnion
        {
            Validation.ThrowIfNull(entity, nameof(entity));
            Validation.ThrowIfNull(selector, nameof(selector));

            EnsureCompatiabilityOrThrow(typeof(TReturn), typeof(TValue));

            var getter = GetOrCompileSelector(selector);
            var current = getter(entity);

            if (current is not null)
            {
                // Pattern matching handles: Nullable<T> ↔ T, ref-type upcasts, etc.
                if (current is TReturn result)
                    return result;

                throw new InvalidOperationException(
                    $"Cannot convert property value of type '{typeof(TValue)}' to return type '{typeof(TReturn)}'.");
            }

            return fallbackValue;
        }

        /// <summary>
        /// For a given <paramref name="outputType" />, ensures coercability and compatability
        /// with <paramref name="suppliedValueType" />. If not compatible an <see cref="InvalidOperationException" /> is thrown.<br /><br />
        /// 1. Types that are the same are always compatible <br />
        /// 2. Nullable value types are compatible with their non-nullable equivilants (e.g. <c>int?</c> and <c>int</c>, <c>MyEnum?</c> and <c>MyEnum</c>, <c>MyStruct?</c> and <c>MyStruct</c>, )<br />
        /// 3. Reference types are compatible if <paramref name="suppliedValueType" /> if can be cast to <paramref name="outputType" />.  <br />
        /// 4. Value types are NEVER compatible with Reference Types (e.g. <c>MyClass</c> != <c>MyStruct</c> <br />
        /// </summary>
        private static void EnsureCompatiabilityOrThrow(Type outputType, Type suppliedValueType)
        {
            Validation.ThrowIfNull(outputType, nameof(outputType));
            Validation.ThrowIfNull(suppliedValueType, nameof(suppliedValueType));

            // 1. Exact match
            if (outputType == suppliedValueType)
                return;

            var outputUnderlying = Nullable.GetUnderlyingType(outputType);
            var suppliedUnderlying = Nullable.GetUnderlyingType(suppliedValueType);

            bool outputIsValue = (outputUnderlying ?? outputType).IsValueType;
            bool suppliedIsValue = (suppliedUnderlying ?? suppliedValueType).IsValueType;

            // 2. Value vs Reference incompatibility
            if (outputIsValue != suppliedIsValue)
                throw new InvalidOperationException(CreateIncompatibilityMessage(outputType, suppliedValueType));

            if (outputIsValue)
            {
                // Both are value types (possibly nullable)
                var outputCore = outputUnderlying ?? outputType;
                var suppliedCore = suppliedUnderlying ?? suppliedValueType;

                // 3. Nullable and non-nullable of the same core type are compatible
                if (outputCore == suppliedCore)
                    return;

                throw new InvalidOperationException(CreateIncompatibilityMessage(outputType, suppliedValueType));
            }

            // 4. Reference types: compatible if supplied can be cast to output
            if (Validation.IsCastable(suppliedValueType, outputType))
                return;

            throw new InvalidOperationException(CreateIncompatibilityMessage(outputType, suppliedValueType));
        }

        private static string CreateIncompatibilityMessage(Type output, Type supplied)
        {
            return $"The supplied type '{supplied}' is not compatible with the required output type '{output}'.";
        }

        private static Func<TType, TValue> GetOrCompileSelector<TType, TValue>(Expression<Func<TType, TValue>> selector)
        {
            (Type EntityType, Type ValueType) key = (typeof(TType), typeof(TValue));

            Delegate compiled = SELECTOR_CACHE.GetOrAdd(
                key,
                _ => selector.Compile());

            return (Func<TType, TValue>)compiled;
        }
    }
}