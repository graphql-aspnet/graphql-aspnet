﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Templates
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeDirectiveActionDefinition"/>
    /// used to generate new graphql directives via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    internal class RuntimeDirectiveActionDefinition : BaseRuntimeControllerActionDefinition, IGraphQLRuntimeDirectiveActionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDirectiveActionDefinition"/> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options where this directive will be created.</param>
        /// <param name="directiveName">Name of the directive to use in the schema.</param>
        public RuntimeDirectiveActionDefinition(SchemaOptions schemaOptions, string directiveName)
            : base(schemaOptions, SchemaItemCollections.Directives, directiveName)
        {
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            return null;
        }

        /// <inheritdoc />
        public Delegate Resolver { get; set; }

        /// <inheritdoc />
        public Type ReturnType { get; set; }
    }
}