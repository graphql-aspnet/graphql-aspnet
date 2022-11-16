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

    /// <summary>
    /// An interface that declares a given <see cref="IDocumentPart"/>
    /// declares a collection of arguments.
    /// </summary>
    public interface IInputArgumentCollectionContainer
    {
        /// <summary>
        /// Gets the collection of arguments defined on this instance.
        /// </summary>
        /// <value>The arguments.</value>
        IInputArgumentCollectionDocumentPart Arguments { get; }
    }
}