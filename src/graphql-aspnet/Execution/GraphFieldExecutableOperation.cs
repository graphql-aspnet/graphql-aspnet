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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// A container representing the conversion of an operation on a query document into a runnable collection of
    /// contexts that can be executed to generate an output.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{OperationName} (Type = {OperationType})")]
    public class GraphFieldExecutableOperation : IGraphFieldExecutableOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldExecutableOperation" /> class.
        /// </summary>
        /// <param name="operation">The reference operation to use when constructing this container.</param>
        public GraphFieldExecutableOperation(QueryOperation operation)
        {
            Validation.ThrowIfNull(operation, nameof(operation));

            this.OperationName = operation.Name?.Trim() ?? string.Empty;
            this.OperationType = operation.OperationType;
            this.FieldContexts = new FieldInvocationContextCollection();
            this.Messages = new GraphMessageCollection();
            this.DeclaredVariables = new QueryVariableCollection();

            if (operation.Variables != null)
            {
                foreach (var variable in operation.Variables.Values)
                    this.DeclaredVariables.AddVariable(variable);
            }
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
        public GraphCollection OperationType { get; }

        /// <summary>
        /// Gets the name of the operation as it was defined in the query document. May be null or empty if
        /// this is an anonymous operation.
        /// </summary>
        /// <value>The name of the operation.</value>
        public string OperationName { get; }

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
        public IQueryVariableCollection DeclaredVariables { get; }

        /// <summary>
        /// Gets a collection of the field contexts present in this operation, regardless of level, that have some
        /// security requirements attached to them.
        /// </summary>
        /// <value>The secure fields.</value>
        public IEnumerable<IGraphFieldInvocationContext> SecureFieldContexts
        {
            get
            {
                foreach (var context in this.FieldContexts)
                {
                    var found = this.YieldSecureContexts(context);
                    foreach (var secureContext in found)
                        yield return secureContext;
                }
            }
        }

        private IEnumerable<IGraphFieldInvocationContext> YieldSecureContexts(IGraphFieldInvocationContext context)
        {
            if (context.Field.SecurityGroups.Any())
                yield return context;

            foreach (var child in context.ChildContexts)
            {
                var found = this.YieldSecureContexts(child);
                foreach (var childContext in found)
                    yield return childContext;
            }
        }
    }
}