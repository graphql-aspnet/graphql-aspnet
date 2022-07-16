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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Resolvers;

    /// <summary>
    /// A collection of <see cref="IGraphField"/> owned by a <see cref="IGraphType"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class GraphFieldCollection : IGraphFieldCollection
    {
        private readonly IGraphType _owner;
        private readonly Dictionary<string, IGraphField> _fields;
        private readonly List<IGraphField> _requiredFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldCollection" /> class.
        /// </summary>
        /// <param name="owner">The graphtype that owns this field collection.</param>
        public GraphFieldCollection(IGraphType owner)
        {
            _owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _fields = new Dictionary<string, IGraphField>(StringComparer.Ordinal);
            _requiredFields = new List<IGraphField>();
        }

        /// <inheritdoc />
        public IGraphField AddField(IGraphField field)
        {
            Validation.ThrowIfNull(field, nameof(field));

            if (_fields.ContainsKey(field.Name))
            {
                var existingField = _fields[field.Name];
                throw new GraphTypeDeclarationException(
                    $"Duplciate field name detected. The graph type '{_owner.Name}' already declares a field named '{existingField.Name}'. " +
                    "This may occur if a type extension is added with the same name as an existing field or " +
                    "when an attempt is made to extend an OBJECT type through a direct extension and an indirect " +
                    "INTERFACE extension with the same field name.");
            }

            field.AssignParent(_owner);
            _fields.Add(field.Name, field);
            if (field.TypeExpression.IsRequired)
                _requiredFields.Add(field);

            return field;
        }

        /// <summary>
        /// Creates and adds a new <see cref="IGraphField" /> to the growing collection.
        /// </summary>
        /// <typeparam name="TSource">The expected type of the source data supplied to the resolver.</typeparam>
        /// <typeparam name="TReturn">The expected type of data to be returned from this field.</typeparam>
        /// <param name="fieldName">The formatted name of the field as it will appear in the object graph.</param>
        /// <param name="typeExpression">The item representing how this field returns a graph type.</param>
        /// <param name="route">The formal route that uniquely identifies this field in the object graph.</param>
        /// <param name="resolver">The resolver used to fulfil requests to this field.</param>
        /// <param name="description">The description to assign to the field.</param>
        /// <returns>IGraphTypeField.</returns>
        internal IGraphField AddField<TSource, TReturn>(
            string fieldName,
            GraphTypeExpression typeExpression,
            GraphFieldPath route,
            Func<TSource, Task<TReturn>> resolver,
            string description = null)
            where TSource : class
        {
            var field = new MethodGraphField(
                fieldName,
                typeExpression,
                route,
                GraphValidation.EliminateNextWrapperFromCoreType(typeof(TReturn)),
                typeof(TReturn),
                FieldResolutionMode.PerSourceItem,
                new GraphDataValueResolver<TSource, TReturn>(resolver));
            field.Description = description;

            return this.AddField(field);
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
            return _fields.ContainsKey(fieldName);
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
        public IGraphType Owner => _owner;

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