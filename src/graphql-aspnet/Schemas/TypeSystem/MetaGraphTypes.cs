// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    /// <summary>
    /// An enumeration of the possible wrappers that can surround a core graph type to express a value
    /// in the object graph.
    /// </summary>
    public enum MetaGraphTypes
    {
        /// <summary>
        /// Denotes that the field must not return a null value when being fulfilled by a resolver. If the field
        /// returns a list, the items contained within the list must not be null.
        /// </summary>
        IsNotNull = 2,

        /// <summary>
        /// Denotes that the item is to return a list of items of the given type rather than one single item.
        /// </summary>
        IsList = 3,
    }
}