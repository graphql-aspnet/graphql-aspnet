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
    public interface IGraphFieldCollection : IReadOnlyGraphFieldCollection
    {
        /// <summary>
        /// Adds the <see cref="IGraphField" /> to the collection.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <returns>IGraphTypeField.</returns>
        IGraphField AddField(IGraphField field);
    }
}