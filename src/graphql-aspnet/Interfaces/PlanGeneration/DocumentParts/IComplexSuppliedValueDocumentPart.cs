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

    /// <summary>
    /// A value supplied in a query document representing a complex input object with its
    /// own defined fields.
    /// </summary>
    public interface IComplexSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableFieldSet
    {
        /// <summary>
        /// Determines whether this complex value contains an input value named <paramref name="argumentName"/>.
        /// </summary>
        /// <param name="argumentName">Name of the argument to look for.</param>
        /// <returns><c>true</c> if the specified argument name contains argument; otherwise, <c>false</c>.</returns>
        bool ContainsArgument(string argumentName);

        /// <summary>
        /// Attempts to retrieve an named argument on this complex value.
        /// </summary>
        /// <param name="argumentName">Name of the argument to search for.</param>
        /// <param name="foundArgument">When found, the argument is assigned to this parameter.</param>
        /// <returns><c>true</c> if the argument was found, <c>false</c> otherwise.</returns>
        bool TryGetArgument(string argumentName, out IInputArgumentDocumentPart foundArgument);

        /// <summary>
        /// Gets the set of arguments/fields defined on the query document for this complex value.
        /// </summary>
        /// <value>The arguments.</value>
        IInputArgumentCollectionDocumentPart Arguments { get; }
    }
}