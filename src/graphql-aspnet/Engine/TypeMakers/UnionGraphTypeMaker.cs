﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object responsible for generating a union graph type from a proxy.
    /// </summary>
    public sealed class UnionGraphTypeMaker : IUnionGraphTypeMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionGraphTypeMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public UnionGraphTypeMaker(ISchema schema)
        {
            _schema = schema;
        }

        /// <inheritdoc />
        public GraphTypeCreationResult CreateUnionFromProxy(IGraphUnionProxy proxy)
        {
            if (proxy == null)
                return null;

            var result = new GraphTypeCreationResult();

            var directiveTemplates = proxy.GetType().ExtractAppliedDirectiveTemplates(proxy);
            foreach (var dt in directiveTemplates)
                dt.ValidateOrThrow();

            var directives = directiveTemplates.CreateAppliedDirectives();

            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            var name = formatter.FormatGraphTypeName(proxy.Name);
            var union = new UnionGraphType(
                name,
                (IUnionGraphTypeMapper)proxy,
                new SchemaItemPath(SchemaItemCollections.Types, name),
                directives)
            {
                Description = proxy.Description,
                Publish = proxy.Publish,
            };

            result.GraphType = union;

            // add dependencies to each type included in the union
            foreach (var type in proxy.Types)
            {
                union.AddPossibleGraphType(
                    formatter.FormatGraphTypeName(GraphTypeNames.ParseName(type, TypeKind.OBJECT)),
                    type);
            }

            // add dependencies for the directives declared on the union
            foreach (var d in directives.Where(x => x.DirectiveType != null))
            {
                result.AddDependent(d.DirectiveType, TypeKind.DIRECTIVE);
            }

            return result;
        }
    }
}