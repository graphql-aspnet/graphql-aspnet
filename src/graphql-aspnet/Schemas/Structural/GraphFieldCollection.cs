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
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A collection of <see cref="IGraphField"/> owned by a <see cref="IGraphType"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class GraphFieldCollection : IGraphFieldCollection
    {
        private readonly Dictionary<string, IGraphField> _fields;
        private readonly List<IGraphField> _requiredFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldCollection" /> class.
        /// </summary>
        public GraphFieldCollection()
        {
            _fields = new Dictionary<string, IGraphField>(StringComparer.Ordinal);
            _requiredFields = new List<IGraphField>();
        }

        /// <inheritdoc />
        public IGraphField AddField(IGraphField field)
        {
            Validation.ThrowIfNull(field, nameof(field));

            if (_fields.ContainsKey(field.Name))
            {
                throw new GraphTypeDeclarationException(
                    $"Duplicate field name detected. The collection already declares a field named '{field.Name}' (Path: {field.ItemPath}). " +
                    "This may occur if a type extension is added with the same name as an existing field, " +
                    "when an attempt is made to extend an OBJECT type through a direct field extension and an indirect " +
                    "interface field extension with the same name or when a schema attempts to include multiple overloads " +
                    "of the same method on a class, interface or struct.");
            }

            _fields.Add(field.Name, field);
            if (field.TypeExpression.IsNonNullable)
                _requiredFields.Add(field);

            return field;
        }

        /// <inheritdoc />
        public IGraphField FindField(string fieldName)
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
        public bool Contains(IGraphField field)
        {
            Validation.ThrowIfNull(field, nameof(field));
            if (!this.ContainsKey(field.Name))
                return false;

            var foundField = this[field.Name];
            return object.ReferenceEquals(field, foundField);
        }

        /// <inheritdoc />
        public IGraphField this[string fieldName]
        {
            get
            {
                return _fields[fieldName];
            }
        }

        /// <inheritdoc />
        public int Count => _fields.Count;

        /// <inheritdoc />
        public IReadOnlyList<IGraphField> RequiredFields => _requiredFields;

        /// <inheritdoc />
        public IEnumerator<IGraphField> GetEnumerator()
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