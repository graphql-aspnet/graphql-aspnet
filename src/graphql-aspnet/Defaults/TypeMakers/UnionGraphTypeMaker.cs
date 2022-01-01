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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
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
        /// <returns>IUnionGraphType.</returns>
        public IUnionGraphType CreateGraphType(IGraphUnionProxy proxy)
        {
            if (proxy == null)
                return null;

            var directiveTemplates = proxy.GetType().ExtractAppliedDirectiveTemplates(proxy);
            foreach (var dt in directiveTemplates)
                dt.ValidateOrThrow();

            var directives = directiveTemplates.CreateAppliedDirectives();

            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            var name = formatter.FormatGraphTypeName(proxy.Name);
            var union = new UnionGraphType(
                name,
                (IUnionTypeMapper)proxy,
                new GraphFieldPath(GraphCollection.Types, name),
                directives)
            {
                Description = proxy.Description,
                Publish = proxy.Publish,
            };

            foreach (var type in proxy.Types)
            {
                union.AddPossibleGraphType(
                    formatter.FormatGraphTypeName(GraphTypeNames.ParseName(type, TypeKind.OBJECT)),
                    type);
            }

            return union;
        }
    }
}