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
    /// A document part representing the special '__typename' field applicable to any
    /// object or interface graph type.
    /// </summary>
    public interface IFieldTypeNameDocumentPart : IFieldDocumentPart
    {
    }
}