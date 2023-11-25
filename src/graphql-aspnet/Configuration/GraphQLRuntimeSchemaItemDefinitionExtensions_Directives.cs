﻿// *************************************************************
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
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
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition RequireAuthorization(
            this IGraphQLRuntimeDirectiveDefinition directiveTemplate,
            string policyName = null,
            string roles = null)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));

            return RequireAuthorizationInternal(directiveTemplate, policyName, roles);
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
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition AllowAnonymous(this IGraphQLRuntimeDirectiveDefinition directiveTemplate)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));
            return AllowAnonymousInternal(directiveTemplate);
        }

        /// <summary>
        /// Marks this directive as being repeatable such that it can be applied to a single
        /// schema item more than once.
        /// </summary>
        /// <param name="directiveTemplate">The directive template.</param>
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition IsRepeatable(this IGraphQLRuntimeDirectiveDefinition directiveTemplate)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));
            if (directiveTemplate.Attributes.Count(x => x is RepeatableAttribute) == 0)
            {
                var repeatable = new RepeatableAttribute();
                directiveTemplate.AddAttribute(repeatable);
            }

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
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition RestrictLocations(
            this IGraphQLRuntimeDirectiveDefinition directiveTemplate,
            DirectiveLocation locations)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));
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
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition AddResolver(this IGraphQLRuntimeDirectiveDefinition directiveTemplate, Delegate resolverMethod)
        {
            return AddResolverInternal(directiveTemplate, null, null, resolverMethod);
        }

        /// <summary>
        /// Assigns a custom value to the internal name of this directive. This value will be used in error
        /// messages and log entries instead of an anonymous method name. This can significantly increase readability
        /// while trying to debug an issue.
        /// </summary>
        /// <remarks>
        /// This value does NOT affect the directive name as it would appear in a schema. It only effects the internal
        /// name used in log messages and exception text.
        /// </remarks>
        /// <param name="directiveTemplate">The directive being built.</param>
        /// <param name="internalName">The value to use as the internal name for this field definition when its
        /// added to the schema.</param>
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition WithInternalName(this IGraphQLRuntimeDirectiveDefinition directiveTemplate, string internalName)
        {
            Validation.ThrowIfNull(directiveTemplate, nameof(directiveTemplate));
            directiveTemplate.InternalName = internalName;
            return directiveTemplate;
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema being constructed.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition MapDirective(this ISchemaBuilder schemaBuilder, string directiveName)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            return MapDirective(schemaBuilder.Options, directiveName, null as Delegate);
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaBuilder">The builder representing the schema being constructed.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <param name="resolverMethod">The resolver that will be executed when the directive is invoked.</param>
        /// <returns>IGraphQLDirectiveTemplate.</returns>
        public static IGraphQLRuntimeDirectiveDefinition MapDirective(this ISchemaBuilder schemaBuilder, string directiveName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaBuilder, nameof(schemaBuilder));
            return MapDirective(schemaBuilder.Options, directiveName, resolverMethod);
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaOptions">The schema options where the directive will be created.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition MapDirective(this SchemaOptions schemaOptions, string directiveName)
        {
            return MapDirective(schemaOptions, directiveName, null as Delegate);
        }

        /// <summary>
        /// Maps a new directive into the target schema.
        /// </summary>
        /// <param name="schemaOptions">The schema options where the directive will be created.</param>
        /// <param name="directiveName">Name of the directive (e.g. '@myDirective').</param>
        /// <param name="resolverMethod">The resolver that will be executed when the directive is invoked.</param>
        /// <returns>IGraphQLRuntimeDirectiveDefinition.</returns>
        public static IGraphQLRuntimeDirectiveDefinition MapDirective(this SchemaOptions schemaOptions, string directiveName, Delegate resolverMethod)
        {
            Validation.ThrowIfNull(schemaOptions, nameof(schemaOptions));

            while (directiveName != null && directiveName.StartsWith(TokenTypeNames.STRING_AT_SYMBOL))
                directiveName = directiveName.Substring(1);

            var directive = new RuntimeDirectiveActionDefinition(schemaOptions, directiveName);
            schemaOptions.AddRuntimeSchemaItem(directive);

            if (resolverMethod != null)
                directive.AddResolver(resolverMethod);

            return directive;
        }
    }
}