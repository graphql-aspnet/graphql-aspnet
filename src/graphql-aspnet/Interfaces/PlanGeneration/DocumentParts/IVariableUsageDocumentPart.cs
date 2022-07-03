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
    /// A supplied value in a query document representing a variable defined on the
    /// query operation that owns this document part.
    /// </summary>
    public interface IVariableUsageDocumentPart : ISuppliedValueDocumentPart, IResolvablePointer
    {
        /// <summary>
        /// Gets the name of the variable this instance references.
        /// </summary>
        /// <value>The name of the variable.</value>
        ReadOnlyMemory<char> VariableName { get; }
    }
}