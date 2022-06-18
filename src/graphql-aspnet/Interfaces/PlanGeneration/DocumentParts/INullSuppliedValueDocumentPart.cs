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
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;

    /// <summary>
    /// A supplied value in a query document representing <c>null</c> or nothing.
    /// </summary>
    public interface INullSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableValue
    {
    }
}