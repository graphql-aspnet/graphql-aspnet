﻿// *************************************************************
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
    /// A collection of rules that can be executed against a given context.
    /// </summary>
    /// <typeparam name="TContext">The type of the context the rules work against.</typeparam>
    public interface IRulePackage<TContext>
    {
        /// <summary>
        /// Fetches the rules that should be executed, in order, for the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;IRuleStep&lt;TContext&gt;&gt;.</returns>
        IEnumerable<IRuleStep<TContext>> FetchRules(TContext context);
    }
}