// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
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
    public interface ISuppliedValueDocumentPart : IDocumentPart, IResolvableItem
    {
        /// <summary>
        /// Adds the provided document part as a child of this instance.
        /// </summary>
        /// <param name="child">The child to add to this document part.</param>
        internal void AddChild(IDocumentPart child);

#pragma warning disable SA1623 // Property summary documentation should match accessors
        /// <summary>
        /// Gets the defined argument in the query document to which this value is ultimately associated.
        /// </summary>
        /// <value>The owner of this supplie value.</value>
        IAssignableValueDocumentPart Owner { get; internal set; }

        /// <summary>
        /// Gets the input value this instance is contained within, if any. For example a list item or a field
        /// of a complex input object.
        /// </summary>
        /// <value>The value that contains this value.</value>
        ISuppliedValueDocumentPart ParentValue { get; internal set; }
#pragma warning restore SA1623 // Property summary documentation should match accessors

        /// <summary>
        /// Gets the value node from the query document that represents this input value.
        /// </summary>
        /// <value>The value node.</value>
        SyntaxNode ValueNode { get; }
    }
}