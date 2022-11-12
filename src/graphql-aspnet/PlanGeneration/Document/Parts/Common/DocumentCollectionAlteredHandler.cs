// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.Common
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// A delegate describing a method that wishes to subscribe to change events on a
    /// document parts collection.
    /// </summary>
    /// <param name="targetpart">The targetpart.</param>
    public delegate void DocumentCollectionAlteredHandler(IDocumentPart targetpart);
}