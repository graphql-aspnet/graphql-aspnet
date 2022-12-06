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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Represents an input argument to a field or directive declared
    /// in a query document.
    /// </summary>
    public interface IInputObjectFieldDocumentPart : IInputValueDocumentPart
    {
        /// <summary>
        /// Gets the input field from the schema represented by this document part.
        /// </summary>
        /// <value>The field.</value>
        IInputGraphField Field { get; }
    }
}