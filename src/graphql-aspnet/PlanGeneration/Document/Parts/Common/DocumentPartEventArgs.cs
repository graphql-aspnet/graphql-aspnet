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
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The <see cref="DocumentPartEventArgs"/> instance containing the event data.</param>
    public delegate void DocumentCollectionAlteredHandler(object sender, DocumentPartEventArgs eventArgs);

    /// <summary>
    /// A set of event args to communicate changes to a document parts collection.
    /// </summary>
    public class DocumentPartEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartEventArgs" /> class.
        /// </summary>
        /// <param name="targetPart">The target part.</param>
        /// <param name="relativeDepth">The relative depth of the <paramref name="targetPart"/>
        /// compared to the owner of the collection raising the event.</param>
        public DocumentPartEventArgs(IDocumentPart targetPart, int relativeDepth)
        {
            this.TargetDocumentPart = targetPart;
            this.RelativeDepth = relativeDepth;
        }

        /// <summary>
        /// Gets the document part that was effected and caused the event to raise.
        /// </summary>
        /// <value>The target document part.</value>
        public IDocumentPart TargetDocumentPart { get; }

        /// <summary>
        /// Gets the relative depth of the <see cref="TargetDocumentPart"/> compared
        /// to the owner of the part collection. For example, a depth of 1 indicates a direct child.
        /// A depth of two indicates a grand-child etc.
        /// </summary>
        /// <value>The relative depth.</value>
        public int RelativeDepth { get; }
    }
}