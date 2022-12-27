// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;

    /// <summary>
    /// A value supplied in a query document representing a complex input object with its
    /// own defined fields.
    /// </summary>
    public interface IComplexSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableFieldSet
    {
        /// <summary>
        /// Determines whether this complex value contains an input value named <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="fieldName">Name of the field to look for.</param>
        /// <returns><c>true</c> if the field exists on this collection; otherwise, <c>false</c>.</returns>
        bool ContainsField(string fieldName);

        /// <summary>
        /// Attempts to retrieve an named field on this complex value.
        /// </summary>
        /// <param name="argumentName">Name of the field to search for.</param>
        /// <param name="foundField">When found, the field is assigned to this parameter.</param>
        /// <returns><c>true</c> if the argument was found, <c>false</c> otherwise.</returns>
        bool TryGetField(string argumentName, out IInputObjectFieldDocumentPart foundField);

        /// <summary>
        /// Gets the set of fields defined on the query document for this complex value.
        /// </summary>
        /// <value>The supplied fields.</value>
        IInputObjectFieldCollectionDocumentPart Fields { get; }
    }
}