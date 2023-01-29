// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    /// <summary>
    /// A schema item that may define a default value.
    /// </summary>
    public interface IDefaultValueSchemaItem : ISchemaItem
    {
        /// <summary>
        /// Gets a value indicating whether this instance was explictly marked as required, meaning it
        /// must be supplied on a query.
        /// </summary>
        /// <value><c>true</c> if this instance is required; otherwise, <c>false</c>.</value>
        bool IsRequired { get; }

        /// <summary>
        /// Gets a default value to use for any instances of this schema item when one is not explicitly provided.
        /// The default can be <c>null</c>. Inspect <see cref="HasDefaultValue"/>
        /// to determine if a default value is present.
        /// </summary>
        /// <value>The boxed, default value, if any.</value>
        object DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has a defined <see cref="DefaultValue"/>.
        /// </summary>
        /// <value><c>true</c> if this instance has a default value; otherwise, <c>false</c>.</value>
        bool HasDefaultValue { get; }
    }
}