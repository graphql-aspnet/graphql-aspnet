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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLFieldTemplate"/>
    /// used to generate new type extensions via a minimal api style of coding.
    /// </summary>
    internal class GraphQLTypeExtensionFieldTemplate : GraphQLResolvedFieldTemplate, IGraphQLTypeExtensionTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLTypeExtensionFieldTemplate"/> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options where this type extension is being declared.</param>
        /// <param name="typeToExtend">The target OBJECT or INTERFACE type to extend.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="typeToExtend"/>.</param>
        /// <param name="resolutionMode">The resolution mode for the resolver implemented by this
        /// type extension.</param>
        public GraphQLTypeExtensionFieldTemplate(
            SchemaOptions schemaOptions,
            Type typeToExtend,
            string fieldName,
            FieldResolutionMode resolutionMode)
            : base(schemaOptions, fieldName)
        {
            this.ExecutionMode = resolutionMode;
            this.TargetType = typeToExtend;
        }

        /// <inheritdoc />
        public FieldResolutionMode ExecutionMode { get; set; }

        /// <inheritdoc />
        public Type TargetType { get; }
    }
}