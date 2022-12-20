// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.RulesEngine
{
    using System.Collections.Generic;

    /// <summary>
    /// <para>
    /// An interface describing a generator that can create a collection of child contexts using its own
    /// internal rules for determing what constitutes a child context.
    /// </para>
    /// <para>
    /// When implemented by a context, a rules package will automatically generate child contexts
    /// and execute rules against them in a depth first manner.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">The type of the child context that needs to be generated.</typeparam>
    public interface IChildContextGenerator<TContext>
    {
        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that
        /// need to be processed against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        IEnumerable<TContext> CreateChildContexts();
    }
}