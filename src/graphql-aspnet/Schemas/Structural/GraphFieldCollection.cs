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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A collection of <see cref="IGraphField"/> owned by a <see cref="IGraphType"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class GraphFieldCollection : IReadOnlyGraphFieldCollection
    {
        private readonly IGraphType _owner;
        private readonly Dictionary<string, IGraphField> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldCollection" /> class.
        /// </summary>
        /// <param name="owner">The graphtype that owns this field collection.</param>
        public GraphFieldCollection(IGraphType owner)
        {
            _owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _fields = new Dictionary<string, IGraphField>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Adds the <see cref="IGraphField" /> to the collection.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <returns>IGraphTypeField.</returns>
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

            _fields.Add(field.Name, field);
            return field;
        }

        /// <summary>
        /// Creates and adds a new <see cref="IGraphField" /> to the growing collection.
        /// </summary>
        /// <typeparam name="TSource">The expected type of the source data supplied to the resolver.</typeparam>
        /// <typeparam name="TReturn">The expected type of data to be returned from this field.</typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="typeExpression">The item representing how this field returns a graph type.</param>
        /// <param name="route">The formal route that identifies this field in the object graph.</param>
        /// <param name="resolver">The resolver used to fulfil requests to this field.</param>
        /// <param name="description">The description to assign to the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public IGraphField AddField<TSource, TReturn>(
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

        /// <summary>
        /// Attempts to find a field of the given name. Returns null if the field is not found.
        /// </summary>
        /// <param name="fieldName">The name of the field to find.</param>
        /// <returns>A graph field matching the name or null.</returns>
        public IGraphField FindField(string fieldName)
        {
            return !string.IsNullOrWhiteSpace(fieldName) && _fields.ContainsKey(fieldName)
                ? _fields[fieldName]
                : null;
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="IGraphField" />. Field
        /// names are case sensitive and should match the public name supplied for introspection
        /// requests...NOT the internal concrete action name if the field is bound to a method.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns><c>true</c> if this collection contains the type name; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string fieldName)
        {
            return _fields.ContainsKey(fieldName);
        }

        /// <summary>
        /// Gets the <see cref="IGraphField" /> with the specified name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphType.</returns>
        public IGraphField this[string fieldName]
        {
            get
            {
                return _fields[fieldName];
            }
        }

        /// <summary>
        /// Gets the total number of <see cref="IGraphField"/> in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _fields.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGraphField> GetEnumerator()
        {
            return _fields.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}