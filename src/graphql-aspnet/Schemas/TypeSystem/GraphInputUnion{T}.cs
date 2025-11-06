// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// Represents an inline input union (i.e. a `@oneOf` input type). The supplied type
    /// parameter will be parsed and exposed as an input union rather than a traditional input object.
    /// </summary>
    /// <typeparam name="T">The type of the input object to treat as a union.</typeparam>
    /// <remarks>
    /// Note: the type <typeparamref name="T" /> must adhere to the rules defined for one of input unions or else an
    /// exception will be thrown at startup. See documentation for details.
    /// </remarks>
    public sealed class GraphInputUnion<T> : GraphInputUnion
    {
        /// <summary>
        /// Gets or sets the value of the input object as it was supplied in the query.
        /// </summary>
        [GraphSkip]
        public T Value { get; set; }
    }
}