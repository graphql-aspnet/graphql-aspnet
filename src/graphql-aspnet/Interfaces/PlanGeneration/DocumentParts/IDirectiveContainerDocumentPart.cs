namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    public interface IDirectiveContainerDocumentPart : IDocumentPart
    {
        IDirectiveCollectionDocumentPart Directives { get; }
    }
}
