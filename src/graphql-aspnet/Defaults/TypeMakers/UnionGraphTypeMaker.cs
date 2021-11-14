// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults.TypeMakers
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object responsible for generating a union graph type from a proxy.
    /// </summary>
    public class UnionGraphTypeMaker
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

        /// <summary>
        /// Creates a union graph type from the given proxy.
        /// </summary>
        /// <param name="proxy">The proxy to convert to a union.</param>
        /// <param name="ownerKind">The typekind of the <see cref="IGraphType"/> that sponsors this union.</param>
        /// <returns>IUnionGraphType.</returns>
        public IUnionGraphType CreateGraphType(IGraphUnionProxy proxy, TypeKind ownerKind)
        {
            if (proxy == null)
                return null;

            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            var name = formatter.FormatGraphTypeName(proxy.Name);
            var union = new UnionGraphType(name, proxy)
            {
                Description = proxy.Description,
                Publish = proxy.Publish,
            };

            foreach (var type in proxy.Types)
            {
                union.AddPossibleGraphType(
                    formatter.FormatGraphTypeName(GraphTypeNames.ParseName(type, ownerKind)),
                    type);
            }

            return union;
        }
    }
}