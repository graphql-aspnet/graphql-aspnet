// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1402 // File may only contain a single type
namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A delegate describing a method that wishes to subscribe to change events on a
    /// document parts collection.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The <see cref="DocumentPartEventArgs"/> instance containing the event data.</param>
    internal delegate void DocumentCollectionAlteredHandler(object sender, DocumentPartEventArgs eventArgs);

    /// <summary>
    /// A delegate describing a method that can act to determine if a document part
    /// should be added to a collection.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The <see cref="DocumentPartBeforeAddEventArgs"/> instance containing the event data.</param>
    internal delegate void DocumentCollectionBeforeAddHandler(object sender, DocumentPartBeforeAddEventArgs eventArgs);

    /// <summary>
    /// A set of event args to communicate changes to a document parts collection.
    /// </summary>
    internal class DocumentPartEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartEventArgs"/> class.
        /// </summary>
        /// <param name="targetPart">The target part.</param>
        public DocumentPartEventArgs(IDocumentPart targetPart)
        {
            this.TargetDocumentPart = targetPart;
        }

        /// <summary>
        /// Gets the document part that was effected and caused the event to raise.
        /// </summary>
        /// <value>The target document part.</value>
        public IDocumentPart TargetDocumentPart { get; }
    }

    /// <summary>
    /// A set of event args used to determine if a part should be added to a collection.
    /// </summary>
    internal class DocumentPartBeforeAddEventArgs : DocumentPartEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBeforeAddEventArgs"/> class.
        /// </summary>
        /// <param name="targetPart">The target part.</param>
        public DocumentPartBeforeAddEventArgs(IDocumentPart targetPart)
            : base(targetPart)
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