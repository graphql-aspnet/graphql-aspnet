// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// An wrapper for a <see cref="OperationTypeNode"/> to track additional details needed during the validation
    /// and construction phase.
    /// </summary>
    [DebuggerDisplay("Operation: {Name} (Type = {OperationType})")]
    public class QueryOperation : IFieldContainerDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryOperation" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="operationType">Type of the operation being represented.</param>
        /// <param name="operationGraphType">The graph type representing the operation type.</param>
        public QueryOperation(OperationTypeNode node, GraphCollection operationType, IObjectGraphType operationGraphType)
        {
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.OperationType = operationType;
            this.GraphType = Validation.ThrowIfNullOrReturn(operationGraphType, nameof(operationGraphType));
            this.Name = this.Node.OperationName.IsEmpty ? string.Empty : node.OperationName.ToString();
        }

        /// <summary>
        /// Creates a collection of variables to be used and applied to the operation as its defined in the query document.
        /// Prior to calling this method this opertion will accept no variables.
        /// </summary>
        /// <returns>QueryVariableCollection.</returns>
        public IQueryVariableCollection CreateVariableCollection()
        {
            if (this.Variables == null)
            {
                this.Variables = new QueryVariableCollection();
            }

            return this.Variables;
        }

        /// <summary>
        /// Creates the field selection set to return from the operation type.
        /// </summary>
        /// <returns>FieldSelectionSet.</returns>
        public FieldSelectionSet CreateFieldSelectionSet()
        {
            if (this.FieldSelectionSet == null)
            {
                this.FieldSelectionSet = new FieldSelectionSet(this.GraphType, new SourcePath());
            }

            return this.FieldSelectionSet;
        }

        /// <summary>
        /// Gets the type of the operation that was parsed.
        /// </summary>
        /// <value>The type of the operation.</value>
        public GraphCollection OperationType { get; }

        /// <summary>
        /// Gets the set of nodes requested from this operation.
        /// </summary>
        /// <value>The complete collection of nodes.</value>
        public FieldSelectionSet FieldSelectionSet { get; private set; }

        /// <summary>
        /// Gets the node parsed from the syntax tree to warrant this operation reference.
        /// </summary>
        /// <value>The node.</value>
        public OperationTypeNode Node { get; }

        /// <summary>
        /// Gets the operation, in the target schema, that is referenced by this instance.
        /// </summary>
        /// <value>The operation.</value>
        public IObjectGraphType GraphType { get; }

        /// <summary>
        /// Gets a collection of variables as defined for this operation in the query document.
        /// </summary>
        /// <value>The variables.</value>
        public IQueryVariableCollection Variables { get; private set; }

        /// <summary>
        /// Gets the name of the operation as its defined in the query document.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                if (this.Variables != null)
                    yield return this.Variables;

                if (this.FieldSelectionSet != null)
                    yield return this.FieldSelectionSet;
            }
        }
    }
}