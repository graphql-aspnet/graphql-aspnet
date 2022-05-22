// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Helpful filters for navgiating the <see cref="ISchemaItem"/> selection
    /// set when late binding directives.
    /// </summary>
    public static class DirectiveApplicatorSchemaItemExtensions
    {
        /// <summary>
        /// Determines whether the specified item is one related to introspection queries.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the specified item is internal; otherwise, <c>false</c>.</returns>
        public static bool IsIntrospectionItem(this ISchemaItem item)
        {
            return item != null && item is IIntrospectionSchemaItem;
        }

        /// <summary>
        /// Determines whether the specified item is one owned by the graphql system.
        /// (e.g. an item that starts with '__').
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the specified item is system level data; otherwise, <c>false</c>.</returns>
        public static bool IsSystemItem(this ISchemaItem item)
        {
            // name regex matches on valid "user supplied names" any schema items
            // made that don't match this are made internally by the system
            return item != null && !Constants.RegExPatterns.NameRegex.IsMatch(item.Name);
        }

        /// <summary>
        /// Determines whether the given graph item is a virtual item, or belongs to a vritual item,
        /// created by the library and not by the developer.
        /// </summary>
        /// <param name="item">The item to inspect.</param>
        /// <returns><c>true</c> if this item is virtual, not tied to a real object; otherwise, <c>false</c>.</returns>
        public static bool IsVirtualItem(this ISchemaItem item)
        {
            if (item == null)
                return true;

            if (item is IGraphType gt)
                return gt.IsVirtual;

            if (item is IGraphField gf)
                return gf.Parent.IsVirtualItem();

            if (item is IEnumValue ev)
                return ev.Parent.IsVirtualItem();

            if (item is IGraphArgument ga)
                return ga.Parent.IsVirtualItem();

            return false;
        }
    }
}