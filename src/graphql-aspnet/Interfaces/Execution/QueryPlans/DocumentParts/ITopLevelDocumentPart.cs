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
    using System.Collections.Generic;

    /// <summary>
    /// An interface describing common elements amongst all top level document
    /// parts within a query document.
    /// </summary>
    public interface ITopLevelDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets a collection of all the directives defined on any document part, at any level,
        /// within this part.
        /// </summary>
        /// <value>All directives defined as children, at any level, on this part.</value>
        IReadOnlyList<IDirectiveDocumentPart> AllDirectives { get; }
    }
}