// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A container representing the conversion of an operation on a query document into a runnable collection of
    /// contexts that can be executed to generate an output.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{OperationName} (Type = {OperationType})")]
    public class GraphFieldExecutableOperation : IGraphFieldExecutableOperation
    {
        private IOperationDocumentPart _operation;
        private IVariableCollectionDocumentPart _variableCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldExecutableOperation" /> class.
        /// </summary>
        /// <param name="operation">The reference operation to use when constructing this container.</param>
        public GraphFieldExecutableOperation(IOperationDocumentPart operation)
        {
            _operation = Validation.ThrowIfNullOrReturn(operation, nameof(operation));

            // Create a blank internal collection of variables if none
            // were defined in the query docuemnt.
            _variableCollection = _operation.Variables;

            this.FieldContexts = new FieldInvocationContextCollection();
            this.Messages = new GraphMessageCollection();
        }

        /// <summary>
        /// Gets the top level group of field contexts that need to be resolved to fulfill the operation
        /// requirements.
        /// </summary>
        /// <value>The field contexts.</value>
        public IFieldInvocationContextCollection FieldContexts { get; }

        /// <summary>
        /// Gets the type of the operation (mutation, query etc.)
        /// </summary>
        /// <value>The type of the operation.</value>
        public GraphOperationType OperationType => _operation.OperationType;

        /// <summary>
        /// Gets the name of the operation as it was defined in the query document. May be null or empty if
        /// this is an anonymous operation.
        /// </summary>
        /// <value>The name of the operation.</value>
        public string OperationName => _operation.Name?.Trim() ?? string.Empty;

        /// <summary>
        /// Gets the messages generated during the creation of this operation.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the declared variables that must be resolved for this operation to be
        /// executed.
        /// </summary>
        /// <value>The declared variables.</value>
        public IVariableCollectionDocumentPart Variables => _variableCollection;
    }
}