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

    /// <summary>
    /// A general interface reprsenting an item in the query document that can contain directives.
    /// </summary>
    public interface IDirectiveContainerDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Inserts the directive into this document part at the indicated location.
        /// </summary>
        /// <param name="directive">The directive to add to this instance.</param>
        /// <param name="rank">The relative rank of this directive to others this instance might contain.
        /// Directives are executed in ascending order by the engine.</param>
        internal void InsertDirective(IDirectiveDocumentPart directive, int rank);

        /// <summary>
        /// Gets the directives assigned to this instance.
        /// </summary>
        /// <value>The directives.</value>
        IEnumerable<IDirectiveDocumentPart> Directives { get; }
    }
}