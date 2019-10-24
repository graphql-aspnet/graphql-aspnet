// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;

    /// <summary>
    /// An interface describing an entity that is capable of
    /// converting some source data (a set of characters representing a value in a query) to a concrete object usable completing
    /// a query.  The implementation and expected output will vary from resolver to resolver.
    /// </summary>
    public interface IScalarValueResolver
    {
        /// <summary>
        /// Processes the given request against this instance
        /// performing the source data conversion operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="data">The actual data read from the query document.</param>
        /// <returns>The final native value of the converted data.</returns>
        object Resolve(ReadOnlySpan<char> data);
    }
}