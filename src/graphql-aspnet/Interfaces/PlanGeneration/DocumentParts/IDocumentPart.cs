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
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A general interface describing part of a query document and the "child" items it may contain. Makes for easy
    /// iteration through a constructed document in a similar fashion to walking a <see cref="ISyntaxTree"/>.
    /// </summary>
    public interface IDocumentPart
    {
        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        IEnumerable<IDocumentPart> Children { get; }
    }
}