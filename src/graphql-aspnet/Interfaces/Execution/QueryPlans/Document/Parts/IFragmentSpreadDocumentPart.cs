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
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A document part representing the spreading of a named fragment within
    /// the selection set where this spread is defined.
    /// </summary>
    public interface IFragmentSpreadDocumentPart : IDirectiveContainerDocumentPart, IIncludeableDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Assigns the named fragment document part this spread is referencing.
        /// </summary>
        /// <param name="targetFragment">The target fragment.</param>
        void AssignNamedFragment(INamedFragmentDocumentPart targetFragment);

        /// <summary>
        /// Gets the name of the fragment this instance is spreading.
        /// </summary>
        /// <value>The name of the fragment.</value>
        string FragmentName { get; }

        /// <summary>
        /// Gets a reference to the named fragment in the document this instance is targeting,
        /// if any.
        /// </summary>
        /// <value>The named fragment in the document.</value>
        INamedFragmentDocumentPart Fragment { get; }
    }
}