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
    using System.Collections.Generic;

    /// <summary>
    /// A representation of a collection of variables on a given operation defined in a user's query document.
    /// </summary>
    public interface IVariableCollectionDocumentPart : IReadOnlyDictionary<string, IVariableDocumentPart>
    {
        /// <summary>
        /// Gets the operation that defines these variables.
        /// </summary>
        /// <value>The owner operation.</value>
        public IOperationDocumentPart Operation { get; }
    }
}