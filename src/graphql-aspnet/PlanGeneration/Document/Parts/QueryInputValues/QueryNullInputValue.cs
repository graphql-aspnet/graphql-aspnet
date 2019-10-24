// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues
{
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// An input value representing that nothing/null was used as a supplied parameter for an input argument.
    /// </summary>
    public class QueryNullInputValue : QueryInputValue, IResolvableValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryNullInputValue" /> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        public QueryNullInputValue(SyntaxNode node)
            : base(node)
        {
        }

        /// <summary>
        /// Gets the value to be used to resolve to some .NET type.
        /// </summary>
        /// <value>The resolvable value.</value>
        ReadOnlySpan<char> IResolvableValue.ResolvableValue => ReadOnlySpan<char>.Empty;
    }
}