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


        bool TryGetArgument(string fieldName, out IInputArgumentDocumentPart foundArgument);
    }
}