// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables
{
    using System;

    /// <summary>
    /// An interface describing a single input value (scalar or enum) that can be resolved to a .NET object.
    /// </summary>
    public interface IResolvableValue : IResolvableItem
    {
        /// <summary>
        /// Gets the value to be used to resolve to some .NET type.
        /// </summary>
        /// <value>The resolvable value.</value>
        ReadOnlySpan<char> ResolvableValue { get; }
    }
}