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
    /// A document part encapsulating a named fragment declared in a supplied
    /// query document.
    /// </summary>
    public interface INamedFragmentDocumentPart : IFragmentDocumentPart, IReferenceDocumentPart, ITopLevelDocumentPart, IDirectiveContainerDocumentPart
    {
        /// <summary>
        /// Gets the unique name of this reference in the collection.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}