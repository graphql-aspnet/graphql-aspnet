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
    /// <summary>
    /// A resolvable value that has been assigned an explicit key value.
    /// </summary>
    public interface IResolvableKeyedItem : IResolvableValueItem
    {
        /// <summary>
        /// Gets the key value assigned to this instance.
        /// </summary>
        /// <value>The key.</value>
        string Key { get; }
    }
}