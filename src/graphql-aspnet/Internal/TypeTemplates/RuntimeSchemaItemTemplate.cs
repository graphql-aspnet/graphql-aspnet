// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;

    /// <summary>
    /// A template of a schema item generated at runtime rather than via a class defined
    /// at compile time.
    /// </summary>
    internal class RuntimeSchemaItemTemplate : GraphDirectiveTemplate
    {
        private IGraphQLRuntimeSchemaItemDefinition _runtimeDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSchemaItemTemplate"/> class.
        /// </summary>
        /// <param name="definition">The definition, configured at runtime to templatize.</param>
        public RuntimeSchemaItemTemplate(IGraphQLRuntimeSchemaItemDefinition definition)
            : base(typeof(RuntimeSchemaItemTypeMarker))
        {
            _runtimeDefinition = Validation.ThrowIfNullOrReturn(definition, nameof(definition));
        }
    }
}