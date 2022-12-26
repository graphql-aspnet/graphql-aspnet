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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;

    /// <summary>
    /// A supplied value in a query document representing <c>null</c> or nothing.
    /// </summary>
    public interface INullSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableNullValue
    {
    }
}