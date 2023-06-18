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
    /// A template of a directive generated at runtime rather than via a class that
    /// inherits from a <see cref="GraphDirective"/>.
    /// </summary>
    internal class RuntimeSchemaItemTemplate : GraphDirectiveTemplate
    {
        private IGraphQLRuntimeSchemaItemTemplate _runtimeTemplate;

        public RuntimeSchemaItemTemplate(IGraphQLRuntimeSchemaItemTemplate template)
            : base(typeof(RuntimeSchemaItemTypeMarker))
        {
            _runtimeTemplate = Validation.ThrowIfNullOrReturn(template, nameof(template));
        }
    }
}