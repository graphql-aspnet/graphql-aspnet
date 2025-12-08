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
    /// Represents a schema item that has a formal schema coordinate.
    /// </summary>
    public interface ISchemaCoordinateItem
    {
        /// <summary>
        /// Gets the formal schema coordinate value applied to this schema item. This value uniquely identifies the
        /// schema item in a consistent manner according to the official specification.
        /// </summary>
        string SchemaCoordinate { get; }
    }
}
