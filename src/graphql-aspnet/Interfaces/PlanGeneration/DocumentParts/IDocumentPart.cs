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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.PlanGeneration.Document;

    /// <summary>
    /// A general interface describing part of a query document and the document parts it may contain.
    /// </summary>
    public interface IDocumentPart
    {
        /// <summary>
        /// Gets the child parts declared on this instance, if any. Child parts may include
        /// child fields, input arguments, variable collections, assigned directives etc.
        /// </summary>
        /// <value>The children.</value>
        IEnumerable<IDocumentPart> Children { get; }

        /// <summary>
        /// Gets the type of this document part.
        /// </summary>
        /// <value>The type of the part.</value>
        DocumentPartType PartType { get; }
    }
}