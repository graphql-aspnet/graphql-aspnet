// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLRuntimeSchemaItemDefinitionExtensions
    {
        /// <summary>
        /// Registers a new type extension to a given OBJECT or INTERFACE type for the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is synonymous with using the <see cref="TypeExtensionAttribute"/> on
        /// a controller action.
        /// </para>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOwnerType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="builder">The schema builder to append the field to.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TOwnerType"/>.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension<TOwnerType>(this ISchemaBuilder builder, string fieldName)
        {
            Validation.ThrowIfNull(builder, nameof(builder));

            return MapTypeExtension(
                builder.Options,
                typeof(TOwnerType),
                fieldName,
                null, // unionName
                null as Delegate);
        }

        /// <summary>
        /// Registers a new type extension to a given OBJECT or INTERFACE type for the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is synonymous with using the <see cref="TypeExtensionAttribute"/> on
        /// a controller action.
        /// </para>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOwnerType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="builder">The schema builder to append the field to.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TOwnerType"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension<TOwnerType>(this ISchemaBuilder builder, string fieldName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(builder, nameof(builder));

            return MapTypeExtension(
                builder.Options,
                typeof(TOwnerType),
                fieldName,
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Registers a new type extension to a given OBJECT or INTERFACE type for the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is synonymous with using the <see cref="TypeExtensionAttribute"/> on
        /// a controller action.
        /// </para>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOwnerType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="builder">The schema builder to append the field to.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TOwnerType"/>.</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension<TOwnerType>(this ISchemaBuilder builder, string fieldName, string unionName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(builder, nameof(builder));

            return MapTypeExtension(
                builder.Options,
                typeof(TOwnerType),
                fieldName,
                unionName,
                resolverMethod);
        }

        /// <summary>
        /// Registers a new type extension to a given OBJECT or INTERFACE type for the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is synonymous with using the <see cref="TypeExtensionAttribute"/> on
        /// a controller action.
        /// </para>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOwnerType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TOwnerType"/>.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension<TOwnerType>(this SchemaOptions schemaOptions, string fieldName)
        {
            return MapTypeExtension(
                schemaOptions,
                typeof(TOwnerType),
                fieldName,
                null, // unionName
                null as Delegate);
        }

        /// <summary>
        /// Registers a new type extension to a given OBJECT or INTERFACE type for the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is synonymous with using the <see cref="TypeExtensionAttribute"/> on
        /// a controller action.
        /// </para>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOwnerType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TOwnerType"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension<TOwnerType>(this SchemaOptions schemaOptions, string fieldName, Delegate resolverMethod)
        {
            return MapTypeExtension(
                schemaOptions,
                typeof(TOwnerType),
                fieldName,
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Registers a new field to a given type on the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <paramref name="fieldOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldOwnerType">The concrete interface, class or struct to extend with a new field.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="fieldOwnerType"/>.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension(this SchemaOptions schemaOptions, Type fieldOwnerType, string fieldName)
        {
            return MapTypeExtension(
                schemaOptions,
                fieldOwnerType,
                fieldName,
                null, // unionName
                null as Delegate);
        }

        /// <summary>
        /// Registers a new field to a given type on the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <paramref name="fieldOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldOwnerType">The concrete interface, class or struct to extend with a new field.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="fieldOwnerType"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension(this SchemaOptions schemaOptions, Type fieldOwnerType, string fieldName, Delegate resolverMethod)
        {
            return MapTypeExtension(
                schemaOptions,
                fieldOwnerType,
                fieldName,
                null, // unionName
                resolverMethod);
        }

        /// <summary>
        /// Registers a new type extension to a given OBJECT or INTERFACE type for the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is synonymous with using the <see cref="TypeExtensionAttribute"/> on
        /// a controller action.
        /// </para>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOwnerType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TOwnerType"/>.</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension<TOwnerType>(this SchemaOptions schemaOptions, string fieldName, string unionName, Delegate resolverMethod)
        {
            return MapTypeExtension(
                schemaOptions,
                typeof(TOwnerType),
                fieldName,
                unionName,
                resolverMethod);
        }

        /// <summary>
        /// Registers a new field to a given type on the target schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The supplied resolver must declare a parameter that is of the same type as <paramref name="fieldOwnerType"/>.
        /// </para>
        /// </remarks>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldOwnerType">The concrete interface, class or struct to extend with a new field.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="fieldOwnerType"/>.</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeResolvedFieldDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition MapTypeExtension(this SchemaOptions schemaOptions, Type fieldOwnerType, string fieldName, string unionName, Delegate resolverMethod)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            fieldOwnerType = Validation.ThrowIfNullOrReturn(fieldOwnerType, nameof(fieldOwnerType));
            fieldName = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));

            IGraphQLRuntimeTypeExtensionDefinition field = new RuntimeTypeExtensionDefinition(
                schemaOptions,
                fieldOwnerType,
                fieldName,
                FieldResolutionMode.PerSourceItem);

            schemaOptions.AddRuntimeSchemaItem(field);

            if (resolverMethod != null)
                field = field.AddResolver(unionName, resolverMethod);

            return field;
        }

        /// <summary>
        /// Instructs the new type extension field that it should process data in batched mode rather than
        /// in a "per source item" mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The supplied resolver must declare a parameter that is an <see cref="IEnumerable{T}"/> of the same <see cref="Type"/> as
        /// class, interface or struct that was originally extended as indicated by <see cref="IGraphQLRuntimeTypeExtensionDefinition.TargetType"/>.
        /// </para>
        /// </remarks>
        /// <param name="typeExtension">The type extension to make into a batch field.</param>
        /// <returns>IGraphQLTypeExtensionTemplate.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition WithBatchProcessing(this IGraphQLRuntimeTypeExtensionDefinition typeExtension)
        {
            Validation.ThrowIfNull(typeExtension, nameof(typeExtension));
            typeExtension.ExecutionMode = FieldResolutionMode.Batch;
            return typeExtension;
        }

        /// <summary>
        /// Adds a set of possible return types for this field. This is synonymous to using the
        /// <see cref="PossibleTypesAttribute" /> on a controller's action method.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times. Any new types will be appended to the field.
        /// </remarks>
        /// <param name="typeExtension">The field being built.</param>
        /// <param name="firstPossibleType">The first possible type that might be returned by this
        /// field.</param>
        /// <param name="additionalPossibleTypes">Any number of additional possible types that
        /// might be returned by this field.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition AddPossibleTypes(this IGraphQLRuntimeTypeExtensionDefinition typeExtension, Type firstPossibleType, params Type[] additionalPossibleTypes)
        {
            Validation.ThrowIfNull(typeExtension, nameof(typeExtension));
            return AddPossibleTypesInternal(typeExtension, firstPossibleType, additionalPossibleTypes);
        }

        /// <summary>
        /// Clears all extra defined possible types this field may declare. This will not affect the core type defined by the resolver, if
        /// a resolver has been defined for this field.
        /// </summary>
        /// <param name="typeExtension">The field builder.</param>
        /// <returns>IGraphQLRuntimeResolvedFieldDefinition.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition ClearPossibleTypes(this IGraphQLRuntimeTypeExtensionDefinition typeExtension)
        {
            Validation.ThrowIfNull(typeExtension, nameof(typeExtension));
            return ClearPossibleTypesInternal(typeExtension);
        }

        /// <summary>
        /// Assigns a custom value to the internal name of this type exension. This value will be used in error
        /// messages and log entries instead of an anonymous method name. This can significantly increase readability
        /// while trying to debug an issue.
        /// </summary>
        /// <remarks>
        /// This value does NOT affect the field name as it would appear in a schema. It only effects the internal
        /// name used in log messages and exception text.
        /// </remarks>
        /// <param name="typeExtension">The type exension field being built.</param>
        /// <param name="internalName">The value to use as the internal name for this field definition when its
        /// added to the schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition WithInternalName(this IGraphQLRuntimeTypeExtensionDefinition typeExtension, string internalName)
        {
            typeExtension.InternalName = internalName;
            return typeExtension;
        }

        /// <summary>
        /// Adds policy-based authorization requirements to the field.
        /// </summary>
        /// <remarks>
        /// This is similar to adding the <see cref="AuthorizeAttribute"/> to a controller method
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="policyName">The name of the policy to assign via this requirement.</param>
        /// <param name="roles">A comma-seperated list of roles to assign via this requirement.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition RequireAuthorization(
            this IGraphQLRuntimeTypeExtensionDefinition fieldBuilder,
            string policyName = null,
            string roles = null)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));
            return RequireAuthorizationInternal(fieldBuilder, policyName, roles);
        }

        /// <summary>
        /// Indicates that the field should allow anonymous access.
        /// </summary>
        /// <remarks>
        /// This is similar to adding the <see cref="AllowAnonymousAttribute"/> to a controller method
        /// </remarks>
        /// <param name="typeExtension">The field being built.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition AllowAnonymous(this IGraphQLRuntimeTypeExtensionDefinition typeExtension)
        {
            Validation.ThrowIfNull(typeExtension, nameof(typeExtension));
            return AllowAnonymousInternal(typeExtension);
        }

        /// <summary>
        /// Sets the resolver to be used when this field is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previously set resolver will be replaced.
        /// </remarks>
        /// <param name="typeExtension">The field being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition AddResolver(this IGraphQLRuntimeTypeExtensionDefinition typeExtension, Delegate resolverMethod)
        {
            return AddResolverInternal(
              typeExtension,
              null as Type, // returnType
              null,  // unionName
              resolverMethod);
        }

        /// <summary>
        /// Sets the resolver to be used when this field is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previously set resolver will be replaced.
        /// </remarks>
        /// <typeparam name="TReturnType">The expected, primary return type of the field. Must be provided
        /// if the supplied delegate returns an <see cref="IGraphActionResult"/>.</typeparam>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition AddResolver<TReturnType>(this IGraphQLRuntimeTypeExtensionDefinition fieldBuilder, Delegate resolverMethod)
        {
            return AddResolverInternal(
                fieldBuilder,
                typeof(TReturnType),
                null,  // unionName
                resolverMethod);
        }

        /// <summary>
        /// Sets the resolver to be used when this field is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previously set resolver will be replaced.
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="unionName">Provide a name and this field will be declared to return a union. Use <see cref="AddPossibleTypes(IGraphQLRuntimeTypeExtensionDefinition, Type, Type[])"/> to declare union members.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeTypeExtensionDefinition AddResolver(this IGraphQLRuntimeTypeExtensionDefinition fieldBuilder, string unionName, Delegate resolverMethod)
        {
            return AddResolverInternal(
                fieldBuilder,
                null as Type, // returnType
                unionName,
                resolverMethod);
        }
    }
}