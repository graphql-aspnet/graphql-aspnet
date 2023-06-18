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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Interfaces.Controllers;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLMinimalApiExtensions
    {
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
        public static IGraphQLRuntimeResolvedFieldTemplate RequireAuthorization(
            this IGraphQLRuntimeResolvedFieldTemplate fieldBuilder,
            string policyName = null,
            string roles = null)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));

            var attrib = new AuthorizeAttribute();
            attrib.Policy = policyName?.Trim();
            attrib.Roles = roles?.Trim();
            fieldBuilder.Attributes.Add(attrib);
            return fieldBuilder;
        }

        /// <summary>
        /// Indicates that the field should allow anonymous access.
        /// </summary>
        /// <remarks>
        /// This is similar to adding the <see cref="AllowAnonymousAttribute"/> to a controller method
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldTemplate AllowAnonymous(this IGraphQLRuntimeResolvedFieldTemplate fieldBuilder)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));
            fieldBuilder.Attributes.Add(new AllowAnonymousAttribute());
            return fieldBuilder;
        }

        /// <summary>
        /// Sets the resolver to be used when this field is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previously set resolver will be replaced.
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldTemplate AddResolver(this IGraphQLRuntimeResolvedFieldTemplate fieldBuilder, Delegate resolverMethod)
        {
            fieldBuilder.Resolver = resolverMethod;
            fieldBuilder.ReturnType = null;

            return fieldBuilder;
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
        public static IGraphQLRuntimeResolvedFieldTemplate AddResolver<TReturnType>(this IGraphQLRuntimeResolvedFieldTemplate fieldBuilder, Delegate resolverMethod)
        {
            fieldBuilder.Resolver = resolverMethod;
            fieldBuilder.ReturnType = typeof(TReturnType);
            return fieldBuilder;
        }

        /// <summary>
        /// Adds a set of possible return types for this field. This is synonymous to using the
        /// <see cref="PossibleTypesAttribute" /> on a controller's action method.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times. Any new types will be appended to the field.
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="firstPossibleType">The first possible type that might be returned by this
        /// field.</param>
        /// <param name="additionalPossibleTypes">Any number of additional possible types that
        /// might be returned by this field.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldTemplate AddPossibleTypes(this IGraphQLRuntimeResolvedFieldTemplate fieldBuilder, Type firstPossibleType, params Type[] additionalPossibleTypes)
        {
            var possibleTypes = new PossibleTypesAttribute(firstPossibleType, additionalPossibleTypes);
            fieldBuilder.Attributes.Add(possibleTypes);
            return fieldBuilder;
        }
    }
}