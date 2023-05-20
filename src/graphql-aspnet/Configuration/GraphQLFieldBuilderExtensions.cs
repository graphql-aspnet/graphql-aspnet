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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.MinimalApi;
    using GraphQL.AspNet.Interfaces.Configuration;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for configuring specifical aspects of a field generated via
    /// the minimal API
    /// </summary>
    public static class GraphQLFieldBuilderExtensions
    {
        /// <summary>
        /// Adds policy-based authorization requirements to the field.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is similar to
        /// adding an <c>[Authorize]</c> attribute to a controller methods. This method can be
        /// called more than once.
        /// </para>
        /// <para>
        /// Each subsequent call will append a new policy in a similar fashion
        /// as if you added multiple <c>[Authorize]</c> attributes to a controller methods.
        /// </para>
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="policyName">Name of the policy to assign via this authorization.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLFieldBuilder RequireAuthorization(
            this IGraphQLFieldBuilder fieldBuilder,
            string policyName)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));

            fieldBuilder.Attributes.Add(new AuthorizeAttribute(policyName));
            return fieldBuilder;
        }

        /// <summary>
        /// Indicates that the field should allow anonymous access.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is similar to adding <c>[AllowAnonymous]</c> to a controller method
        /// </para>
        /// <para>
        /// Any inherited authorization permissions from field groups are automatically
        /// dropped from this field instance.
        /// </para>
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLFieldBuilder AllowAnonymous(this IGraphQLFieldBuilder fieldBuilder)
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));
            fieldBuilder.Attributes.Add(new AllowAnonymousAttribute());
            return fieldBuilder;
        }

        /// <summary>
        /// Sets the resolver to be used when this field is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previous resolver will be replaced.
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <param name="resolver">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLFieldBuilder AddResolver(this IGraphQLFieldBuilder fieldBuilder, Delegate resolver)
        {
            fieldBuilder[GraphQLFieldBuilderConstants.RESOLVER] = resolver;
            return fieldBuilder;
        }
    }
}