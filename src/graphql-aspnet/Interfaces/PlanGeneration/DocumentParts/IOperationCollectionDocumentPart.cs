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
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    public interface IOperationCollectionDocumentPart : IReadOnlyDictionary<string, IOperationDocumentPart>
    {
        IGraphQueryDocument Owner { get; }

        IOperationDocumentPart this[int index] { get; }
    }
}