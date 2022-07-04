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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphArgumentTemplate"/> into a usable <see cref="IGraphArgument"/> on a graph field.
    /// </summary>
    public class GraphArgumentMaker : IGraphArgumentMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphArgumentMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public GraphArgumentMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public GraphArgumentCreationResult CreateArgument(ISchemaItem owner, IGraphArgumentTemplate template)
        {
            Validation.ThrowIfNull(owner, nameof(owner));
            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;

            var directives = template.CreateAppliedDirectives();

            var argument = new GraphFieldArgument(
                owner,
                formatter.FormatFieldName(template.Name),
                template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
                template.Route,
                template.ArgumentModifiers,
                template.DeclaredArgumentName,
                template.InternalFullName,
                template.ObjectType,
                template.HasDefaultValue,
                template.DefaultValue,
                template.Description,
                directives);

            var result = new GraphArgumentCreationResult();
            result.Argument = argument;

            result.AddDependentRange(template.RetrieveRequiredTypes());

            return result;
        }
    }
}