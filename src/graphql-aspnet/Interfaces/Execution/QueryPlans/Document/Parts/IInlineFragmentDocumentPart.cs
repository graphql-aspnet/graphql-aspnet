// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts
{
    /// <summary>
    /// A document part representing an inlined fragment inside a field selection set.
    /// </summary>
    public interface IInlineFragmentDocumentPart : IFragmentDocumentPart, IIncludeableDocumentPart
    {
    }
}