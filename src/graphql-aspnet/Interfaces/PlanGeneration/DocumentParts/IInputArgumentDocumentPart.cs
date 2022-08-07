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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Represents an input argument to a field or directive declared
    /// in a query document.
    /// </summary>
    public interface IInputArgumentDocumentPart : IInputValueDocumentPart
    {
        /// <summary>
        /// Gets the argument from the schema represented by this document part.
        /// </summary>
        /// <value>The type expression.</value>
        IGraphArgument Argument { get; }
    }
}