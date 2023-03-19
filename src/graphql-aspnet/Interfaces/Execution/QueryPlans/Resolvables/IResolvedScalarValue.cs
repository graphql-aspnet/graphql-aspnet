// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables
{
    using System;

    /// <summary>
    /// An interface describing a single scalar value that has been "pre-resolved" by some
    /// external means, usually a server extension, and injected into the resolvable value stream.
    /// </summary>
    public interface IResolvedScalarValue : IResolvableValueItem
    {
        /// <summary>
        /// Gets the already resolved value to be used as the scalar value. This object will not
        /// be altered in any way and is expected to conform to the type of registered
        /// object type in the target schema.
        /// </summary>
        /// <value>The pre-resolved value.</value>
        object ResolvedValue { get; }
    }
}