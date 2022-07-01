// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentPartsNew
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    public interface INamedFragmentDocumentPart : IFragmentDocumentPart, IReferenceDocumentPart
    {

        /// <summary>
        /// Gets the unique name of this reference in the collection.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}