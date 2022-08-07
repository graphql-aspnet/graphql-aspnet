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
    /// A delegate describing a method that can act to determine if a document part
    /// should be added to a collection.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The <see cref="DocumentPartBeforeAddEventArgs"/> instance containing the event data.</param>
    internal delegate void DocumentCollectionBeforeAddHandler(object sender, DocumentPartBeforeAddEventArgs eventArgs);

    /// <summary>
    /// A set of event args used to determine if a part should be added to a collection.
    /// </summary>
    internal class DocumentPartBeforeAddEventArgs : DocumentPartEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBeforeAddEventArgs"/> class.
        /// </summary>
        /// <param name="targetPart">The target part.</param>
        /// <param name="relativeDepth">The relative depth of the <paramref name="targetPart"/>
        /// compared to the owner of the collection raising the event.</param>
        public DocumentPartBeforeAddEventArgs(IDocumentPart targetPart, int relativeDepth)
            : base(targetPart, relativeDepth)
        {
            this.AllowAdd = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the target document part can be added
        /// to the children of this instance.
        /// </summary>
        /// <value><c>true</c> if the part should be added; otherwise, <c>false</c>.</value>
        public bool AllowAdd { get; set; }
    }
}