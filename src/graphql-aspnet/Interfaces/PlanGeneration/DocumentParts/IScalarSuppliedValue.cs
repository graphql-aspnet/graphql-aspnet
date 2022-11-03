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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A supplied value to an query document that represents a scalar.
    /// </summary>
    public interface IScalarSuppliedValue : ISuppliedValueDocumentPart, IResolvableValue
    {
        /// <summary>
        /// Gets the type of the value as it was read on the query document.
        /// </summary>
        /// <value>The type of the value.</value>
        ScalarValueType ValueType { get; }

        /// <summary>
        /// Gets the value literal of the data passed to the input value.
        /// </summary>
        /// <value>The value.</value>
        string Value { get; }
    }
}