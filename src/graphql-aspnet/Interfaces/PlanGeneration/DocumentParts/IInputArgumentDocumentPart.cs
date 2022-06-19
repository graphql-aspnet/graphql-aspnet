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
    /// <summary>
    /// Represents an input argument to a field or directive declared
    /// in a query document.
    /// </summary>
    public interface IInputArgumentDocumentPart : IDirectiveContainerDocumentPart, IAssignableValueDocumentPart
    {
    }
}