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
    /// A meta-interface identifying document parts that are conditionally included and
    /// may contribute to the resultant data object.
    /// </summary>
    public interface IIncludeableDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets or sets a value indicating whether this element is rendered and
        /// included in the result set.
        /// data.
        /// </summary>
        /// <value><c>true</c> if the element should be included; otherwise, <c>false</c>.</value>
        bool IsIncluded { get; set; }
    }
}