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
    public interface IInputGraphFieldCollection : IReadOnlyInputGraphFieldCollection
    {
        /// <summary>
        /// Adds the <see cref="IInputGraphField" /> to the collection.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <returns>IGraphTypeField.</returns>
        IInputGraphField AddField(IInputGraphField field);
    }
}