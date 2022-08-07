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
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;

    /// <summary>
    /// Represents a specific provided value (usually a variable reference or hard coded value) in a query document.
    /// </summary>
    public interface ISuppliedValueDocumentPart : IDocumentPart, IResolvableKeyedItem
    {
        /// <summary>
        /// Determines whether this value part is equal to the provided part according to the
        /// graphql rules of argument value equality. What constitutes
        /// equalness will vary amongst value types.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns><c>true</c> if this value has equality with the supplied value; otherwise, <c>false</c>.</returns>
        bool IsEqualTo(ISuppliedValueDocumentPart value);
    }
}