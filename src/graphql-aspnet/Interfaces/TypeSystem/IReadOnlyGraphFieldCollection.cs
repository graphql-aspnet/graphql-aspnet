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
    using System.Collections.Generic;

    /// <summary>
    /// A collection of fields on a given graph type.
    /// </summary>
    public interface IReadOnlyGraphFieldCollection : IEnumerable<IGraphField>
    {
        /// <summary>
        /// Attempts to find a field of the given name. Returns null if the field is not found.
        /// </summary>
        /// <param name="fieldName">The name of the field to find.</param>
        /// <returns>A graph field matching the name or null.</returns>
        IGraphField FindField(string fieldName);

        /// <summary>
        /// Determines whether this collection contains a <see cref="IGraphField" />. Field
        /// names are case sensitive and should match the public name supplied for introspection
        /// requests...NOT the internal concrete action name if the field is bound to a method.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns><c>true</c> if this collection contains the type name; otherwise, <c>false</c>.</returns>
        bool ContainsKey(string fieldName);

        /// <summary>
        /// Gets the <see cref="IGraphField" /> with the specified name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphType.</returns>
        IGraphField this[string fieldName] { get; }

        /// <summary>
        /// Gets the total number of <see cref="IGraphField"/> in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }
    }
}