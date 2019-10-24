// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    /// <summary>
    /// An interface denoting that an item can be deprecated.
    /// </summary>
    public interface IDeprecatable
    {
        /// <summary>
        /// Gets a value indicating whether this item  is depreciated. The <see cref="DeprecationReason"/> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        bool IsDeprecated { get; }

        /// <summary>
        /// Gets the provided reason for this item being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        string DeprecationReason { get; }
    }
}