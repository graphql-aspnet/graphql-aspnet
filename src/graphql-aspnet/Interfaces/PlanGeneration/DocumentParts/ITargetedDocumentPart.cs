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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A general interface representing an item in the query document that can be restricted to a given graph type.
    /// </summary>
    public interface ITargetedDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets or sets the specific target type of the field, if any. Usually set as a result of
        /// spreading a fragment into a selection set.
        /// </summary>
        /// <value>A single graph type to restrict this field to. May be null.</value>
        IGraphType TargetGraphType { get; set; }
    }
}