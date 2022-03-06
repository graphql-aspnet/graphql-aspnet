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
    /// Marks a graph type as being extendable such that additional fields can be added to it after its been created.
    /// </summary>
    public interface IExtendableGraphType : IGraphType
    {
        /// <summary>
        /// Extends this graph type by adding a new field. An exception may be thrown if
        /// a field with the same name already exists.
        /// </summary>
        /// <param name="newField">The new field to add.</param>
        /// <returns>The newly added field.</returns>
        IGraphField Extend(IGraphField newField);
    }
}