// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A value supplied in a query document that repressents a list of other
    /// <see cref="ISuppliedValueDocumentPart"/>.
    /// </summary>
    public interface IListSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableList
    {
        /// <summary>
        /// Gets the values supplied on this list in the source document.
        /// </summary>
        /// <value>The list items defined in the document.</value>
        public IReadOnlyList<ISuppliedValueDocumentPart> ListItems { get; }

        /// <summary>
        /// Gets the expected type expression of any item in this list.
        /// </summary>
        /// <value>The list item type expression.</value>
        public GraphTypeExpression ListItemTypeExpression { get; }
    }
}