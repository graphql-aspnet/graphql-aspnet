namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    public interface IDirectiveCollectionDocumentPart : IEnumerable<IDirectiveDocumentPart>
    {
        int Count { get; }

        IDirectiveDocumentPart this[int index] { get; }

        IDocumentPart Owner { get; }
    }
}
