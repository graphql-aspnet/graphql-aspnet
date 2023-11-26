// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// An object responsible for generating a union graph type from a proxy.
    /// </summary>
    public sealed class UnionGraphTypeMaker : IGraphTypeMaker, IUnionGraphTypeMaker
    {
        private readonly ISchemaConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionGraphTypeMaker" /> class.
        /// </summary>
        /// <param name="config">The schema configuration used to determine what to incude
        /// and how to build the union.</param>
        public UnionGraphTypeMaker(ISchemaConfiguration config)
        {
            _config = Validation.ThrowIfNullOrReturn(config, nameof(config));
        }

        /// <inheritdoc />
        public GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate)
        {
            if (!(typeTemplate is IUnionGraphTypeTemplate template))
                return null;

            template.Parse();
            template.ValidateOrThrow(false);

            var proxy = GlobalTypes.CreateUnionProxyFromType(template.ProxyType);
            return this.CreateUnionFromProxy(proxy);
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

            var union = new UnionGraphType(
                proxy.Name,
                proxy.InternalName,
                (IUnionGraphTypeMapper)proxy,
                new ItemPath(ItemPathRoots.Types, proxy.Name),
                directives)
            {
                Description = proxy.Description,
                Publish = proxy.Publish,
            };

            // add dependencies to each type included in the union
            foreach (var type in proxy.Types)
                union.AddPossibleGraphType(GraphTypeNames.ParseName(type, TypeKind.OBJECT), type);

            union = _config
                .DeclarationOptions?
                .SchemaFormatStrategy?
                .ApplyFormatting(_config, union) ?? union;

            result.GraphType = union;

            // add dependencies for the directives declared on the union
            foreach (var d in directives.Where(x => x.DirectiveType != null))
            {
                result.AddDependent(d.DirectiveType, TypeKind.DIRECTIVE);
            }

            return result;
        }
    }
}