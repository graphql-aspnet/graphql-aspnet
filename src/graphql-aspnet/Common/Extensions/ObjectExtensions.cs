// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension helper methods for all object types.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts the object into a completed task containing itself.
        /// </summary>
        /// <typeparam name="TItem">The type of the item being wrapped in a task.</typeparam>
        /// <param name="item">The item to return.</param>
        /// <returns>Task&lt;TItem&gt;.</returns>
        [DebuggerStepThrough]
        public static Task<TItem> AsCompletedTask<TItem>(this TItem item)
        {
            return Task.FromResult(item);
        }
    }
}