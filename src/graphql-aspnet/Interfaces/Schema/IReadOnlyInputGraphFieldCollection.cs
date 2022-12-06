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
    using System.Collections.Generic;

    /// <summary>
    /// A collection of input fields on a given graph type.
    /// </summary>
    public interface IReadOnlyInputGraphFieldCollection : IEnumerable<IInputGraphField>
    {
        /// <summary>
        /// Gets the <see cref="IInputObjectGraphType"/> that owns this field collection.
        /// </summary>
        /// <value>The owner.</value>
        IInputObjectGraphType Owner { get; }

        /// <summary>
        /// Attempts to find a field of the given name. Returns null if the field is not found.
        /// </summary>
        /// <param name="fieldName">The name of the field to find.</param>
        /// <returns>A graph field matching the name or null.</returns>
        IInputGraphField FindField(string fieldName);

        /// <summary>
        /// Determines whether this collection contains a <see cref="IInputGraphField" />. Field
        /// names are case sensitive and should match the public name of the field as its
        /// represented in the graph schema...NOT the internal name if the
        /// field is bound to a method.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns><c>true</c> if this collection contains the type name; otherwise, <c>false</c>.</returns>
        bool ContainsKey(string fieldName);

        /// <summary>
        /// Determines whether collection contains the field provided.
        /// </summary>
        /// <param name="field">The field to search for.</param>
        /// <returns><c>true</c> if this instance contains the specified field; otherwise, <c>false</c>.</returns>
        bool Contains(IInputGraphField field);

        /// <summary>
        /// Gets the <see cref="IInputGraphField" /> with the specified name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphType.</returns>
        IInputGraphField this[string fieldName] { get; }

        /// <summary>
        /// Gets the total number of <see cref="IInputGraphField"/> in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets a subset of fields which are required to be provided on a query document
        /// when declaring the input object.
        /// </summary>
        /// <value>The required fields.</value>
        public IReadOnlyList<IInputGraphField> RequiredFields { get; }

        /// <summary>
        /// Gets a subset of fields which MUST have a defined value on an input object.
        /// </summary>
        /// <value>The non nullable fields.</value>
        public IReadOnlyList<IInputGraphField> NonNullableFields { get; }
    }
}