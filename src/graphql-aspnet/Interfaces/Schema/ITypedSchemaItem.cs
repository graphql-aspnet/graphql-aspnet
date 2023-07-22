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
    using System;

    /// <summary>
    /// A  <see cref="ISchemaItem"/> constructed from a specific concrete <see cref="Type"/>.
    /// </summary>
    public interface ITypedSchemaItem : ISchemaItem
    {
        /// <summary>
        /// Gets the .NET type of the class or struct that represents this schema item at runtime.
        /// </summary>
        /// <value>The type of the object.</value>
        Type ObjectType { get; }
    }
}