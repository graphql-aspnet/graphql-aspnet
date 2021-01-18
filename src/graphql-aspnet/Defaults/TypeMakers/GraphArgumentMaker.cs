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
    /// A maker capable of turning a <see cref="IGraphFieldArgumentTemplate"/> into a usable <see cref="IGraphFieldArgument"/> on a graph field.
    /// </summary>
    public class GraphArgumentMaker
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

        /// <summary>
        /// Creates a single graph field from the provided template using hte rules of this maker and the contained schema.
        /// </summary>
        /// <param name="template">The template to generate a field from.</param>
        /// <returns>IGraphField.</returns>
        public GraphArgumentCreationResult CreateArgument(IGraphFieldArgumentTemplate template)
        {
            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;

            var argument = new GraphFieldArgument(
                formatter.FormatFieldName(template.Name),
                template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
                template.ArgumentModifiers,
                template.DeclaredArgumentName,
                template.InternalFullName,
                template.ObjectType,
                template.DefaultValue,
                template.Description);

            var result = new GraphArgumentCreationResult();
            result.Argument = argument;

            result.AddDependentRange(template.RetrieveRequiredTypes());

            return result;
        }
    }
}