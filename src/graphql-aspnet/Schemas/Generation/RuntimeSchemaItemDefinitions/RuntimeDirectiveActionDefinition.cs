// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeDirectiveDefinition"/>
    /// used to generate new graphql directives via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{ItemPath.Path}")]
    public class RuntimeDirectiveActionDefinition : RuntimeControllerActionDefinitionBase, IGraphQLRuntimeDirectiveDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDirectiveActionDefinition" /> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options where this directive will be created.</param>
        /// <param name="directiveName">Name of the directive to use in the schema.</param>
        public RuntimeDirectiveActionDefinition(
            SchemaOptions schemaOptions,
            string directiveName)
            : base(schemaOptions, ItemPathRoots.Directives, directiveName)
        {
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            // if the user never declared any restrictions
            // supply location data for ALL locations
            if (this.AppendedAttributes.OfType<DirectiveLocationsAttribute>().Any())
                return null;

            return new DirectiveLocationsAttribute(DirectiveLocation.AllExecutionLocations | DirectiveLocation.AllTypeSystemLocations)
            {
            };
        }

        /// <inheritdoc />
        public Delegate Resolver { get; set; }

        /// <inheritdoc />
        public Type ReturnType { get; set; }

        /// <inheritdoc />
        public string InternalName { get; set; }
    }
}