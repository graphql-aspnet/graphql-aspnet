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
    /// A value supplied in a query document representing a complex input object with its
    /// own defined fields.
    /// </summary>
    public interface IComplexSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IQueryArgumentContainerDocumentPart, IResolvableFieldSet
    {
    }
}