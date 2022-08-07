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
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;

    /// <summary>
    /// A supplied value to a query document representing an ENUM value.
    /// </summary>
    public interface IEnumSuppliedValueDocumentPart : ISuppliedValueDocumentPart, IResolvableValue
    {
        /// <summary>
        /// Gets the value literal of the data passed to the input value.
        /// </summary>
        /// <value>The value.</value>
        ReadOnlyMemory<char> Value { get; }
    }
}