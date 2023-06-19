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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLRuntimeSchemaItemDefinitionExtensions
    {
        /// <summary>
        /// Adds policy-based authorization requirements to the directive.
        /// </summary>
        /// <remarks>
        /// This is similar to adding the <see cref="AuthorizeAttribute"/> to a controller method
        /// </remarks>
        /// <param name="directiveTemplate">The directive being built.</param>
        /// <param name="policyName">The name of the policy to assign via this requirement.</param>
        /// <param name="roles">A comma-seperated list of roles to assign via this requirement.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition RequireAuthorization(
            this IGraphQLRuntimeDirectiveActionDefinition directiveTemplate,
            string policyName = null,
            string roles = null)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));

            var attrib = new AuthorizeAttribute();
            attrib.Policy = policyName?.Trim();
            attrib.Roles = roles?.Trim();
            directiveTemplate.AddAttribute(attrib);
            return directiveTemplate;
        }

        /// <summary>
        /// Indicates that the directive should allow anonymous access.
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
        /// <param name="directiveTemplate">The directive being built.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition AllowAnonymous(this IGraphQLRuntimeDirectiveActionDefinition directiveTemplate)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));
            directiveTemplate.AddAttribute(new AllowAnonymousAttribute());
            return directiveTemplate;
        }

        /// <summary>
        /// Marks this directive as being repeatable such that it can be applie to a single
        /// schema item more than once.
        /// </summary>
        /// <param name="directiveTemplate">The directive template.</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition IsRepeatable(this IGraphQLRuntimeDirectiveActionDefinition directiveTemplate)
        {
            var repeatable = new RepeatableAttribute();
            directiveTemplate.AddAttribute(repeatable);
            return directiveTemplate;
        }

        /// <summary>
        /// Restricts the locations that this directive can be applied.
        /// </summary>
        /// <remarks>
        /// If called more than once this method acts as an additive restrictor. Each additional
        /// call will add more location restrictions. Duplicate restrictions are ignored.
        /// </remarks>
        /// <param name="directiveTemplate">The directive template to alter.</param>
        /// <param name="locations">The bitwise set of locations where this
        /// directive can be applied.</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition RestrictLocations(
            this IGraphQLRuntimeDirectiveActionDefinition directiveTemplate,
            DirectiveLocation locations)
        {
            var restrictions = new DirectiveLocationsAttribute(locations);
            directiveTemplate.AddAttribute(restrictions);

            return directiveTemplate;
        }

        /// <summary>
        /// Sets the resolver to be used when this directive is requested at runtime.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this method is called more than once, the previously set resolver will be replaced.
        /// </para>
        /// <para>
        /// Directive resolver methods must return a <see cref="IGraphActionResult"/>.
        /// </para>
        /// </remarks>
        /// <param name="directiveTemplate">The directive being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the field on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition AddResolver(this IGraphQLRuntimeDirectiveActionDefinition directiveTemplate, Delegate resolverMethod)
        {
            directiveTemplate.Resolver = resolverMethod;
            directiveTemplate.ReturnType = null;

            return directiveTemplate;
        }

        /// <summary>
        /// Sets the resolver to be used when this directive is requested at runtime.
        /// </summary>
        /// <remarks>
        ///  If this method is called more than once the previously set resolver will be replaced.
        /// </remarks>
        /// <typeparam name="TReturnType">The expected, primary return type of the directive. Must be provided
        /// if the supplied delegate returns an <see cref="IGraphActionResult"/>.</typeparam>
        /// <param name="directiveTemplate">The directive being built.</param>
        /// <param name="resolverMethod">The delegate to assign as the resolver. This method will be
        /// parsed to determine input arguments for the directive on the target schema.</param>
        /// <returns>IGraphQLFieldBuilder.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition AddResolver<TReturnType>(this IGraphQLRuntimeDirectiveActionDefinition directiveTemplate, Delegate resolverMethod)
        {
            directiveTemplate.Resolver = resolverMethod;
            directiveTemplate.ReturnType = typeof(TReturnType);
            return directiveTemplate;
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaOptions">The schema options where the directive will be created.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition MapDirective(this SchemaOptions schemaOptions, string directiveName)
        {
            return MapDirectiveInternal(schemaOptions, directiveName);
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaOptions">The schema options where the directive will be created.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <param name="resolverMethod">The resolver that will be executed when the directive is invoked.</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition MapDirective(this SchemaOptions schemaOptions, string directiveName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var directive = MapDirectiveInternal(schemaOptions, directiveName);
            return directive.AddResolver(resolverMethod);
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema being constructed.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition MapDirective(this ISchemaBuilder schemaBuilder, string directiveName)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));

            var directive = MapDirectiveInternal(
                schemaBuilder.Options,
                directiveName);

            return directive;
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema being constructed.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <param name="resolverMethod">The resolver that will be executed when the directive is invoked.</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveActionDefinition MapDirective(this ISchemaBuilder schemaBuilder, string directiveName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            Validation.ThrowIfNull(resolverMethod, nameof(resolverMethod));

            var directive = MapDirectiveInternal(
                schemaBuilder.Options,
                directiveName);

            return directive.AddResolver(resolverMethod);
        }
    }
}