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
    /// <summary>
    /// A serialized item that represents a pointer to another, already resolved item.
    /// </summary>
    public interface IResolvablePointer : IResolvableItem
    {
        /// <summary>
        /// Gets the name of the item pointed to by this instance.
        /// </summary>
        /// <value>The points to.</value>
        string PointsTo { get; }

        /// <summary>
        /// Gets the fallback resolvable item should the item this pointer points to not exist.
        /// </summary>
        /// <value>The default item.</value>
        IResolvableItem DefaultItem { get; }
    }
}