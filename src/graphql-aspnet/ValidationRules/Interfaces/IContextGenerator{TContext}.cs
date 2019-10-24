// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface describing a generator that can createa  collection of child contexts using its own
    /// internal rules for determing what constitutes a child context.
    /// </summary>
    /// <typeparam name="TContext">The type of the child context that needs to be generated.</typeparam>
    internal interface IContextGenerator<TContext>
    {
        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that need to be processed
        /// against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        IEnumerable<TContext> CreateChildContexts();
    }
}