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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// The complete result of turning a <see cref="IGraphFieldTemplateBase" /> into a <typeparamref name="TFieldType"/>.
    /// </summary>
    /// <typeparam name="TFieldType">The type of the field that was created.</typeparam>
    public class GraphFieldCreationResult<TFieldType> : DependentTypeCollection
        where TFieldType : IGraphFieldBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldCreationResult{TFieldType}" /> class.
        /// </summary>
        public GraphFieldCreationResult()
        {
        }

        /// <summary>
        /// Gets or sets the generated field.
        /// </summary>
        /// <value>The type of the graph.</value>
        public TFieldType Field { get; set; }
    }
}