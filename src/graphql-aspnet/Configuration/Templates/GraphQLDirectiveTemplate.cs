// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Configuration.Templates;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLDirectiveTemplate"/>
    /// used to generate new graphql directives via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    internal class GraphQLDirectiveTemplate : BaseGraphQLRuntimeSchemaItemTemplate, IGraphQLDirectiveTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLDirectiveTemplate"/> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options where this directive will be created.</param>
        /// <param name="directiveName">Name of the directive to use in the schema.</param>
        public GraphQLDirectiveTemplate(SchemaOptions schemaOptions, string directiveName)
            : base(schemaOptions, directiveName)
        {
        }

        /// <inheritdoc />
        public Delegate Resolver { get; set; }

        /// <inheritdoc />
        public Type ReturnType { get; set; }
    }
}