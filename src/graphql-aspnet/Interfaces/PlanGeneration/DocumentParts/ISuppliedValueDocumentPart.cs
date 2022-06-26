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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// Represents a specific provided value (usually a variable reference or hard coded value) in a query document.
    /// </summary>
    public interface ISuppliedValueDocumentPart : IDocumentPart, IResolvableKeyedItem
    {
    }
}