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
    /// An interface defining a graph type as containing a collection of fields.
    /// </summary>
    public interface IGraphFieldContainer : INamedItem
    {
        /// <summary>
        /// Gets a collection of fields made available by this interface.
        /// </summary>
        /// <value>The fields.</value>
        IReadOnlyGraphFieldCollection Fields { get; }

        /// <summary>
        /// Gets the <see cref="IGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        IGraphField this[string fieldName] { get; }
    }
}