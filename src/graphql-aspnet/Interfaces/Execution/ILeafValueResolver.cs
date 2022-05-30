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
    /// converting some low-level source data (a set of characters representing a value in a query)
    /// to a concrete leaf value (enum or scalar) usable for completing a query.
    /// The implementation and expected output will vary from resolver to resolver.
    /// </summary>
    public interface ILeafValueResolver
    {
        /// <summary>
        /// Converts the raw source data from a query doucment
        /// into an object that represents the entity this resolver is employed on.
        /// </summary>
        /// <param name="data">The actual data read from the query document.</param>
        /// <returns>The final native value of the converted data.</returns>
        object Resolve(ReadOnlySpan<char> data);
    }
}