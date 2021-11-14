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
        /// Processes the given sequence of characters against this scalar resolver
        /// attempting to convert the characters into a valid data item representing the scalar.
        /// </summary>
        /// <param name="data">The actual data read from the query document.</param>
        /// <returns>The final native value of the converted data.</returns>
        object Resolve(ReadOnlySpan<char> data);
    }
}