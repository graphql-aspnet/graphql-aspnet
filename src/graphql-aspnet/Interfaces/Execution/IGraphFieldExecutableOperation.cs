// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A parsed operation from a query document that contains the resolvers and argument references that
    /// can fulfill a request for data from the system.
    /// </summary>
    public interface IGraphFieldExecutableOperation
    {
        /// <summary>
        /// Gets the top level group of field contexts that need to be resolved to fulfill the operation
        /// requirements.
        /// </summary>
        /// <value>The field contexts.</value>
        IFieldInvocationContextCollection FieldContexts { get; }

        /// <summary>
        /// Gets the type of the operation (mutation, query etc.)
        /// </summary>
        /// <value>The type of the operation.</value>
        GraphCollection OperationType { get; }

        /// <summary>
        /// Gets the name of the operation as it was defined in the query document. May be null or empty if
        /// this is an anonymous operation.
        /// </summary>
        /// <value>The name of the operation.</value>
        string OperationName { get; }

        /// <summary>
        /// Gets the messages generated during the creation of this operation.
        /// </summary>
        /// <value>The messages.</value>
        IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the declared variables that must be resolved for this operation to be
        /// executed.
        /// </summary>
        /// <value>The declared variables.</value>
        IQueryVariableCollection DeclaredVariables { get; }

        /// <summary>
        /// Gets a collection of the field contexts present in this operation, regardless of level, that have some
        /// security requirements attached to them.
        /// </summary>
        /// <value>The secure fields.</value>
        IEnumerable<IGraphFieldInvocationContext> SecureFieldContexts { get; }
    }
}