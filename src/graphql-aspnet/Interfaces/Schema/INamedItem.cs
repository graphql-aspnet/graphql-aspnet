﻿// *************************************************************
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
    /// An item that has a name.
    /// </summary>
    public interface INamedItem
    {
        /// <summary>
        /// Gets the formal name of this item as it exists in the schema. This value is
        /// deteremined by the item from which it was templated and cannot be changed.
        /// </summary>
        /// <value>The publically referenced name of this entity in the graph.</value>
        string Name { get; }

        /// <summary>
        /// Gets or sets the human-readable description distributed with this item
        /// when requested. The description should accurately describe the contents of this entity
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        string Description { get; set; }
    }
}