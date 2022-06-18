// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A represention of a top level operation defined in a query document.
    /// </summary>
    public interface IQueryOperationDocumentPart : IFieldContainerDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Creates a collection of variables to be used and applied to the operation as its defined in the query document.
        /// Prior to calling this method this opertion will accept no variables.
        /// </summary>
        /// <returns>QueryVariableCollection.</returns>
        internal IQueryVariableCollectionDocumentPart CreateVariableCollection();

        /// <summary>
        /// Gets the type of the operation that was parsed.
        /// </summary>
        /// <value>The type of the operation.</value>
        GraphOperationType OperationType { get; }

        /// <summary>
        /// Gets the operation, in the target schema, that is referenced by this instance.
        /// </summary>
        /// <value>The operation.</value>
        IObjectGraphType GraphType { get; }

        /// <summary>
        /// Gets a collection of variables as defined for this operation in the query document.
        /// </summary>
        /// <value>The variables.</value>
        IQueryVariableCollectionDocumentPart Variables { get; }

        /// <summary>
        /// Gets the name assigned to this query operation. Used to distinguish, and required for,
        /// documents containing multiple operations that may be executed.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the node parsed from the syntax tree to warrant this operation reference.
        /// </summary>
        /// <value>The node.</value>
        OperationTypeNode Node { get; }
    }
}