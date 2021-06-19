// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Validation and helper methods related to various graphql specification requirements.
    /// </summary>
    public static class GraphValidation
    {
        /// <summary>
        /// Retrieves the security policies defined on the given provider, if any. Returns an empty enumeration
        /// if the provider is null or no policies are found.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <returns>IEnumerable&lt;IAuthorizeData&gt;.</returns>
        public static IEnumerable<IAuthorizeData> RetrieveSecurityPolicies(ICustomAttributeProvider attributeProvider)
        {
            if (attributeProvider == null)
                return Enumerable.Empty<IAuthorizeData>();

            return attributeProvider.GetCustomAttributes(false).OfType<IAuthorizeData>();
        }

        /// <summary>
        /// Resolves the <see cref="TypeKind" /> of the provided concrete type. If provided, the override
        /// value will be used by default if the type does not represent a reserved <see cref="TypeKind"/>. If an override value is supplied
        /// but the type cannot be corerced into said kind an exception is thrown.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="overrideValue">The override value to use. Pass null to attempt default resolution.</param>
        /// <returns>TypeKind.</returns>
        public static TypeKind ResolveTypeKindOrThrow(Type type, TypeKind? overrideValue = null)
        {
            var outKind = ResolveTypeKind(type, overrideValue);
            if (overrideValue.HasValue && outKind != overrideValue.Value && !overrideValue.Value.CanBecome(outKind))
            {
                throw new GraphTypeDeclarationException(
                    $"The concrete type '{type.FriendlyName()}' was to be resolved as a graph type of kind '{overrideValue.Value}' but " +
                    $"can only be assigned as '{outKind.ToString()}'",
                    type);
            }

            return outKind;
        }

        /// <summary>
        /// Attempts to classify the provided <see cref="Type"/> to determine its <see cref="TypeKind" />
        /// (enum, scalar, object etc.). If provided, the override
        /// value will be used if allowed by the core <see cref="TypeKind"/> for the provided <see cref="Type"/>.
        /// For instance, an enum or a scalar will always be just those types but an object
        /// type kind can be coerrced to be an input value in some scenarios.
        /// </summary>
        /// <param name="type">The concrete type to check.</param>
        /// <param name="overrideValue">The override value to use. Pass null to attempt default resolution.</param>
        /// <returns>TypeKind.</returns>
        public static TypeKind ResolveTypeKind(Type type, TypeKind? overrideValue = null)
        {
            if (type != null)
            {
                if (GraphQLProviders.ScalarProvider.IsScalar(type))
                {
                    return TypeKind.SCALAR;
                }

                if (type.IsEnum)
                {
                    return TypeKind.ENUM;
                }

                if (Validation.IsCastable<IGraphUnionProxy>(type))
                {
                    return TypeKind.UNION;
                }

                if (type.IsInterface)
                {
                    return TypeKind.INTERFACE;
                }

                if (Validation.IsCastable<GraphDirective>(type))
                {
                    return TypeKind.DIRECTIVE;
                }
            }

            return overrideValue ?? TypeKind.OBJECT;
        }

        /// <summary>
        /// Attempts to drill into the supplied type and remove the next found wrapper (given the provided filters)
        /// to distill it to its core type. (i.e. convert IEnumerable[T] to 'T').
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="eliminateEnumerables">if set to <c>true</c> this method will attempt to eliminate any wrapper type that declares <see cref="IEnumerable{T}" />.</param>
        /// <param name="eliminateTask">if set to <c>true</c> this method will attempt to remove a <see cref="Task{T}" /> declaration from the type.</param>
        /// <param name="eliminateNullableT">if set to <c>true</c> the Nullable{T} wrapper that can exist on value types will be stripped (leaving just T).</param>
        /// <returns>Type.</returns>
        public static Type EliminateNextWrapperFromCoreType(
            Type type,
            bool eliminateEnumerables = true,
            bool eliminateTask = true,
            bool eliminateNullableT = true)
        {
            if (type == null)
                return null;

            // eliminate Task<T>
            if (eliminateTask && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                return type.GetGenericArguments()[0];

            // drill into the concrete type if a list was supplied List<T>, IEnumerable<T>, ICollection<T> etc.
            if (eliminateEnumerables && GraphValidation.IsValidListType(type))
            {
                return type.GetEnumerableUnderlyingType(false);
            }

            if (eliminateNullableT && Validation.IsNullableOfT(type))
            {
                return Nullable.GetUnderlyingType(type);
            }

            return type;
        }

        /// <summary>
        /// Attempts to drill into the supplied type and remove all found wrappers (given the provided filters)
        /// to distill it to its core type. (i.e. convert IEnumerable[T] to 'T').
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="eliminateEnumerables">if set to <c>true</c> this method will attempt to eliminate any wrapper type that declares <see cref="IEnumerable{T}" />.</param>
        /// <param name="eliminateTask">if set to <c>true</c> this method will attempt to remove a <see cref="Task{T}" /> declaration from the type.</param>
        /// <param name="eliminateNullableT">if set to <c>true</c> the Nullable{T} wrapper that can exist on value types will be stripped (leaving just T).</param>
        /// <returns>Type.</returns>
        public static Type EliminateWrappersFromCoreType(
            Type type,
            bool eliminateEnumerables = true,
            bool eliminateTask = true,
            bool eliminateNullableT = true)
        {
            if (type == null)
                return null;

            while (true)
            {
                var unwrappedType = EliminateNextWrapperFromCoreType(type, eliminateEnumerables, eliminateTask, eliminateNullableT);
                if (unwrappedType == type)
                    break;

                type = unwrappedType;
            }

            return type;
        }

        /// <summary>
        /// Helper method to ensure that a name segment destined for the object graph is valid. An exception is
        /// thrown if it is invalid.
        /// </summary>
        /// <param name="internalName">The internal name of the object to display in an error message.</param>
        /// <param name="nameToTest">The potential graph name to test.</param>
        public static void EnsureGraphNameOrThrow(string internalName, string nameToTest)
        {
            if (!IsValidGraphName(nameToTest))
            {
                throw new GraphTypeDeclarationException(
                    $"The item, '{internalName}', declares an invalid name, '{nameToTest}'. Graph names " +
                    "can only contain letters A-Z, numbers 0-9 and an underscore. They must also not start with a double underscore.");
            }
        }

        /// <summary>
        /// Helper method to ensure that parsing of name segment, within a template, is handled
        /// appropriately.
        /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Names .
        /// </summary>
        /// <param name="nameToTest">The potential graph name to test.</param>
        /// <returns><c>true</c> if the supplied name represents a valid graph name; otherwise, <c>false</c>.</returns>
        public static bool IsValidGraphName(string nameToTest)
        {
            return Constants.RegExPatterns.NameRegex.IsMatch(nameToTest);
        }

        /// <summary>
        /// Determines whether the provided type can function a list within graphql.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsValidListType(Type type)
        {
            // disallow the string type (even though it implements IEnumerable<char>)
            // as its a defined scalar value in graphql
            if (type == null || type == typeof(string))
                return false;

            // all lists must be declared as generics to allow for type declaration and parsing.
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                Type enumerableType = type.GetEnumerableUnderlyingType();
                return !enumerableType.IsGenericType || !typeof(KeyValuePair<,>)
                    .IsAssignableFrom(enumerableType.GetGenericTypeDefinition());
            }

            return false;
        }

        /// <summary>
        /// Determines whether the given type CAN be represented in the object graph. An exception is thrown if it cannot be.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public static void EnsureValidGraphTypeOrThrow(Type type)
        {
            GraphValidation.IsValidGraphType(type, true);
        }

        /// <summary>
        /// Determines whether the given type CAN be represented in the object graph.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="throwOnFailure">if set to <c>true</c> a context rich <see cref="GraphTypeDeclarationException"/> will be thrown instead of returning false.</param>
        /// <returns><c>true</c> if the type can be correctly rendered otherwise, <c>false</c>.</returns>
        public static bool IsValidGraphType(Type type, bool throwOnFailure = false)
        {
            if (type == null)
                return false;

            if (type == typeof(void))
                return false;

            if (type.IsEnum)
                return true;

            if (type == typeof(object))
            {
                if (throwOnFailure)
                {
                    throw new GraphTypeDeclarationException(
                        $"The type '{typeof(object).FriendlyName()}' cannot be used directly in an object graph. GraphQL requires a complete " +
                        "definition of a schema for validation and introspection.  Using a generic object prohibits this library from " +
                        "scanning for fields, interfaces and inputs.",
                        type);
                }

                return false;
            }

            // remove any IEnumerable or Task wrappers from the supplied type
            // so we can validate the core entity
            type = GraphValidation.EliminateWrappersFromCoreType(type);

            // explicitly disallow dictionaries
            // graphQL doesn't allow for arbitrary keyvalue pairs in any type renderings
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                if (throwOnFailure)
                {
                    throw new GraphTypeDeclarationException(
                        $"The type '{type.FriendlyName()}' appears to be a {typeof(IDictionary).FriendlyName()}. Objects " +
                        "which allow for arbitrary key/value pairs of data are not allowed in graphQL. This type cannot be used as a publically" +
                        "available graph type.",
                        type);
                }

                return false;
            }

            if (type.IsGenericType)
            {
                if (typeof(IDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                {
                    if (throwOnFailure)
                    {
                        throw new GraphTypeDeclarationException(
                            $"The type '{type.FriendlyName()}' appears to be a {typeof(IDictionary<,>).FriendlyName()}. Objects " +
                            "which allow for arbitrary key/value pairs of data are not allowed in graphQL. This type cannot be used as a publically" +
                            "available graph type.",
                            type);
                    }

                    return false;
                }

                if (typeof(IReadOnlyDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                {
                    if (throwOnFailure)
                    {
                        throw new GraphTypeDeclarationException(
                            $"The type '{type.FriendlyName()}' appears to be a {typeof(IReadOnlyDictionary<,>).FriendlyName()}. Objects " +
                            "which allow for arbitrary key/value pairs of data are not allowed in graphQL. This type cannot be used as a publically" +
                            "available graph type.",
                            type);
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Inspects the provided type and generates a type expression to represent it in the object grpah.
        /// </summary>
        /// <param name="typeToCheck">The complete type specification to check.</param>
        /// <param name="typeDefinition">An optional expression declaration to use as a set of overrides on the type provided.</param>
        /// <returns>GraphFieldOptions.</returns>
        public static GraphTypeExpression GenerateTypeExpression(Type typeToCheck, IGraphTypeExpressionDeclaration typeDefinition = null)
        {
            Validation.ThrowIfNull(typeToCheck, nameof(typeToCheck));

            if (typeDefinition?.TypeWrappers != null)
            {
                typeToCheck = GraphValidation.EliminateWrappersFromCoreType(
                    typeToCheck,
                    eliminateEnumerables: true,
                    eliminateTask: true,
                    eliminateNullableT: true);

                return new GraphTypeExpression(typeToCheck.FriendlyName(), typeDefinition.TypeWrappers);
            }

            // strip out Task{T} before doin any type inspections
            typeToCheck = GraphValidation.EliminateWrappersFromCoreType(
                typeToCheck,
                eliminateEnumerables: false,
                eliminateTask: true,
                eliminateNullableT: false);

            var hasDefinedDefaultValue = typeDefinition?.HasDefaultValue ?? false;

            List<MetaGraphTypes> wrappers = new List<MetaGraphTypes>();
            if (GraphValidation.IsValidListType(typeToCheck))
            {
                // auto generated type expressions will always allow for a nullable list (since class references can be null)
                // unwrap any nested lists as necessary:  e.g.  IEnumerable<IEnumerable<IEnumerable<T>>>
                while (true)
                {
                    var unwrappedType = EliminateNextWrapperFromCoreType(
                        typeToCheck,
                        eliminateEnumerables: true,
                        eliminateTask: false,
                        eliminateNullableT: false);

                    if (unwrappedType == typeToCheck)
                        break;

                    wrappers.Add(MetaGraphTypes.IsList);
                    typeToCheck = unwrappedType;
                }

                if (!hasDefinedDefaultValue && GraphValidation.IsNotNullable(typeToCheck))
                {
                    wrappers.Add(MetaGraphTypes.IsNotNull);
                }
            }
            else if (!hasDefinedDefaultValue && GraphValidation.IsNotNullable(typeToCheck))
            {
                wrappers.Add(MetaGraphTypes.IsNotNull);
            }

            typeToCheck = EliminateWrappersFromCoreType(
                typeToCheck,
                eliminateEnumerables: false,
                eliminateTask: false,
                eliminateNullableT: true);

            return new GraphTypeExpression(typeToCheck.FriendlyName(), wrappers);
        }

        /// <summary>
        /// Checks if the concrete type MUST be provided on the object graph or if it can be represented with 'null'.
        /// </summary>
        /// <param name="typeToCheck">The type to check.</param>
        /// <returns><c>true</c> if not nullable, <c>false</c> otherwise.</returns>
        public static bool IsNotNullable(Type typeToCheck)
        {
            return typeToCheck.IsValueType && !typeToCheck.IsNullableOfT();
        }
    }
}