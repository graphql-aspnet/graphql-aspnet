// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    /// <summary>
    /// An interface describing a call back method to be invoked
    /// when a known descendent is added to a parent part.
    /// </summary>
    internal interface IDescendentDocumentPartSubscriber
    {
        /// <summary>
        /// Called when a new document part has been added to this instance
        /// as a decendent.
        /// </summary>
        /// <param name="decendentPart">The decendent part that was added.</param>
        /// <param name="relativeDepth">The depth of the part relative to this part. (1 == a direct child).</param>
        void OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth);
    }
}