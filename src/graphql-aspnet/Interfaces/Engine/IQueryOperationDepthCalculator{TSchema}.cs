// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An interface describing aclass that can calulate a complexity metric score for a given
    /// query operation.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema.</typeparam>
    public interface IQueryOperationDepthCalculator<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Inspects the operation and determines a final complexity score.
        /// </summary>
        /// <param name="operation">The operation to inspect.</param>
        /// <returns>System.Single.</returns>
        int Calculate(IOperationDocumentPart operation);
    }
}