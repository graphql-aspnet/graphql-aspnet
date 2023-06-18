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
    using GraphQL.AspNet.Configuration.Templates;
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
        public static IGraphQLFieldTemplate RequireAuthorization(
            this IGraphQLFieldTemplate fieldBuilder,
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
        /// <para>
        /// This is similar to adding the <see cref="AllowAnonymousAttribute"/> to a controller method
        /// </para>
        /// <para>
        /// Any inherited authorization permissions from field groups are automatically
        /// dropped from this field instance.
        /// </para>
        /// </remarks>
        /// <param name="fieldBuilder">The field being built.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLFieldTemplate AllowAnonymous(this IGraphQLFieldTemplate fieldBuilder)
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
        /// <param name="field">The field being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldTemplate AddResolver(this IGraphQLFieldTemplate field, Delegate resolverMethod)
        {
            // convert the virtual field to a resolved field
            var resolvedBuilder = GraphQLResolvedFieldTemplate.FromFieldTemplate(field);
            resolvedBuilder.Options.AddSchemaItemTemplate(resolvedBuilder);

            resolvedBuilder.Resolver = resolverMethod;
            resolvedBuilder.ReturnType = null;

            return resolvedBuilder;
        }

        /// <summary>
        /// Sets the resolver to be used when this field is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previously set resolver will be replaced.
        /// </remarks>
        /// <typeparam name="TReturnType">The expected, primary return type of the field. Must be provided
        /// if the supplied delegate returns an <see cref="IGraphActionResult"/>.</typeparam>
        /// <param name="field">The field being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldTemplate AddResolver<TReturnType>(this IGraphQLFieldTemplate field, Delegate resolverMethod)
        {
            // convert the virtual field to a resolved field
            var resolvedBuilder = GraphQLResolvedFieldTemplate.FromFieldTemplate(field);
            resolvedBuilder.Options.AddSchemaItemTemplate(resolvedBuilder);

            resolvedBuilder.Resolver = resolverMethod;
            resolvedBuilder.ReturnType = typeof(TReturnType);
            return resolvedBuilder;
        }

        /// <summary>
        /// Maps a terminal child field into the schema and assigns the resolver method to it.
        /// </summary>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when this field is requested.</param>
        /// <returns>IGraphQLResolvedFieldBuilder.</returns>
        public static IGraphQLRuntimeResolvedFieldTemplate MapField(this IGraphQLFieldTemplate field, string subTemplate, Delegate resolverMethod)
        {
            var subField = new GraphQLResolvedFieldTemplate(field, subTemplate);
            subField.AddResolver(resolverMethod);

            subField.Options.AddSchemaItemTemplate(subField);
            return subField;
        }

        /// <summary>
        /// Maps a child field into the schema underneath the supplied field. This field can be
        /// further extended.
        /// </summary>
        /// <param name="field">The field under which this new field will be nested.</param>
        /// <param name="subTemplate">The template pattern to be appended to the supplied <paramref name="field"/>.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLFieldTemplate MapField(this IGraphQLFieldTemplate field, string subTemplate)
        {
            var subField = new GraphQLVirtualFieldTemplate(field, subTemplate);
            return subField;
        }
    }
}