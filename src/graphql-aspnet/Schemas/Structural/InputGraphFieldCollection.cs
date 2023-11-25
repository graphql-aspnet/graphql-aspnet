// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A collection of <see cref="IInputGraphField" /> owned by a <see cref="IInputObjectGraphType" />.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class InputGraphFieldCollection : IInputGraphFieldCollection
    {
        private readonly Dictionary<string, IInputGraphField> _fields;
        private readonly List<IInputGraphField> _requiredFields;
        private readonly List<IInputGraphField> _nonNullableFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGraphFieldCollection" /> class.
        /// </summary>
        public InputGraphFieldCollection()
        {
            _fields = new Dictionary<string, IInputGraphField>(StringComparer.Ordinal);
            _requiredFields = new List<IInputGraphField>();
            _nonNullableFields = new List<IInputGraphField>();
        }

        /// <summary>
        /// Adds the <see cref="IInputGraphField" /> to the collection.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <returns>IGraphTypeField.</returns>
        public IInputGraphField AddField(IInputGraphField field)
        {
            Validation.ThrowIfNull(field, nameof(field));

            if (_fields.ContainsKey(field.Name))
            {
                throw new GraphTypeDeclarationException(
                    $"Duplicate field name detected. The collection already " +
                    $"declares a field named '{field.Name}' (Route: {field.Route}).");
            }

            _fields.Add(field.Name, field);
            if (field.IsRequired)
                _requiredFields.Add(field);
            if (field.TypeExpression.IsNonNullable)
                _nonNullableFields.Add(field);

            return field;
        }

        /// <inheritdoc />
        public IInputGraphField FindField(string fieldName)
        {
            return !string.IsNullOrWhiteSpace(fieldName) && _fields.ContainsKey(fieldName)
                ? _fields[fieldName]
                : null;
        }

        /// <inheritdoc />
        public bool ContainsKey(string fieldName)
        {
            if (fieldName == null)
                return false;

            return _fields.ContainsKey(fieldName);
        }

        /// <inheritdoc />
        public bool Contains(IInputGraphField field)
        {
            Validation.ThrowIfNull(field, nameof(field));
            if (!this.ContainsKey(field.Name))
                return false;

            var foundField = this[field.Name];
            return object.ReferenceEquals(field, foundField);
        }

        /// <inheritdoc />
        public IInputGraphField this[string fieldName]
        {
            get
            {
                return _fields[fieldName];
            }
        }

        /// <inheritdoc />
        public int Count => _fields.Count;

        /// <inheritdoc />
        public IReadOnlyList<IInputGraphField> RequiredFields => _requiredFields;

        /// <inheritdoc />
        public IReadOnlyList<IInputGraphField> NonNullableFields => _nonNullableFields;

        /// <inheritdoc />
        public IEnumerator<IInputGraphField> GetEnumerator()
        {
            return _fields.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}