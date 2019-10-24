// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    /// <summary>
    /// An interface describing an entity that is capable of
    /// converting some scalar value to a serialized span of characters that can be written to an output stream. This
    /// serializer must perform appropriate escaping of characters.
    /// </summary>
    public interface IScalarValueSerializer
    {
        /// <summary>
        /// Serializes the scalar from its object representing to a serializable value. For most scalars this is
        /// a conversion to a valid string represnetation.
        /// </summary>
        /// <param name="item">The scalar to serialize.</param>
        /// <returns>System.Object.</returns>
        object Serialize(object item);
    }
}