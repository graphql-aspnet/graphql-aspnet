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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A document part that carries security requirements which must the active
    /// user must be authorized before (or as) the query is executed.
    /// </summary>
    public interface ISecureDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets the secure schema item this document part represents.
        /// </summary>
        /// <value>The secure item.</value>
        ISecureSchemaItem SecureItem { get; }
    }
}