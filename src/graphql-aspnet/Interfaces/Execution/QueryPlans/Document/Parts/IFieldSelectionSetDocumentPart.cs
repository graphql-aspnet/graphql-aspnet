// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A collection of fields to query <see cref="IFieldDocumentPart"/>
    /// from a source object.
    /// </summary>
    public interface IFieldSelectionSetDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets a set of fields to resolve for this selection set, in order of execution,
        /// combining all document parts that contribute to the set of fields to be resolved.
        /// This list walks any inline fragments and fragment spreads to produce a final set of fields
        /// that should be resolved.
        /// </summary>
        /// <value>The executable fields of this selection set.</value>
        IExecutableFieldSelectionSet ExecutableFields { get; }
    }
}