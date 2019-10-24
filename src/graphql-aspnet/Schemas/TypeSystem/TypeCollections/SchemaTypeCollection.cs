// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.TypeCollections
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A complete collection of <see cref="IGraphType"/>s known to the schema.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class SchemaTypeCollection : ISchemaTypeCollection
    {
        private readonly ConcurrentDictionary<string, IGraphType> _graphTypesByName;
        private readonly ExtendedGraphTypeTracker _extendableGraphTypeTracker;
        private readonly GraphTypeExtensionQueue _typeQueue;
        private readonly ConcreteTypeCollection _concreteTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaTypeCollection"/> class.
        /// </summary>
        public SchemaTypeCollection()
        {
            _graphTypesByName = new ConcurrentDictionary<string, IGraphType>();
            _concreteTypes = new ConcreteTypeCollection();
            _extendableGraphTypeTracker = new ExtendedGraphTypeTracker();
            _typeQueue = new GraphTypeExtensionQueue();
        }

        /// <summary>
        /// Registers the extension field to the <see cref="IObjectGraphType" /> corrisponding to the supplied
        /// concrete type. If a matching graph type cannot be found for the concrete type supplied, the field
        /// is queued for when it is registered.
        /// </summary>
        /// <param name="masterType">The master type that, once added to the schema, will trigger that addition of this field.</param>
        /// <param name="field">The field that will be added to the graph type associated with the master type.</param>
        public void EnsureGraphFieldExtension(Type masterType, IGraphField field)
        {
            Validation.ThrowIfNull(masterType, nameof(masterType));
            Validation.ThrowIfNullOrReturn(field, nameof(field));

            var graphType = _concreteTypes.FindGraphType(masterType);
            if (graphType != null)
            {
                if (!(graphType is IExtendableGraphType))
                {
                    throw new GraphTypeDeclarationException(
                        $"Fatal error. The graph type '{graphType.Name}' of type '{graphType.Kind.ToString()}' does not implement '{typeof(IExtendableGraphType).FriendlyName()}' " +
                        $"and cannot be extended with the new field '{field.Name}'.");
                }

                _extendableGraphTypeTracker.AddFieldExtension(graphType, field);
                return;
            }

            _typeQueue.EnQueueField(masterType, field);
        }

        /// <summary>
        /// Ensures the provided <see cref="IGraphType" /> exists in this collection (adding it if it is missing)
        /// and that the given type reference is assigned to it. An exception will be thrown if the type reference is already assigned
        /// to a different <see cref="IGraphType" />. No dependents or additional types will be added.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        /// <param name="associatedType">The concrete type to associate to the graph type.</param>
        /// <returns><c>true</c> if type had to be added, <c>false</c> if it already existed in the collection.</returns>
        public bool EnsureGraphType(IGraphType graphType, Type associatedType = null)
        {
            Validation.ThrowIfNull(graphType, nameof(graphType));

            associatedType = GraphValidation.EliminateWrappersFromCoreType(associatedType);
            GraphValidation.EnsureValidGraphTypeOrThrow(associatedType);

            // attempt to create a relationship between the graph type and the associated type
            // the concrete type collection will throw an exception if that relationship fails or isnt
            // updated to the new data correctly.
            var justAdded = _graphTypesByName.TryAdd(graphType.Name, graphType);
            _concreteTypes.EnsureRelationship(graphType, associatedType);
            _extendableGraphTypeTracker.MonitorGraphType(graphType);

            // dequeue and add any extension fields if present
            if (associatedType != null)
            {
                var unregisteredFields = _typeQueue.DequeueFields(associatedType);
                if (graphType is IExtendableGraphType objType)
                {
                    foreach (var field in unregisteredFields)
                    {
                        _extendableGraphTypeTracker.AddFieldExtension(objType, field);
                    }
                }
            }

            return justAdded;
        }

        /// <summary>
        /// Attempts to expand the given graph into the object graph
        /// types it represents. If the given graph type is not one that can be expanded an empty list is returned. Object
        /// graph types will be returned untouched as part of an enumeration.
        /// </summary>
        /// <param name="graphType">The graph type to expand.</param>
        /// <returns>An enumeration of graph types.</returns>
        public IEnumerable<IObjectGraphType> ExpandAbstractType(IGraphType graphType)
        {
            switch (graphType)
            {
                case IInterfaceGraphType igt:
                    return this.FindGraphTypesByInterface(igt);

                case IUnionGraphType ugt:
                    return ugt.PossibleGraphTypeNames
                        .Select(this.FindGraphType)
                        .Where(x => x != null)
                        .OfType<IObjectGraphType>();
                case IObjectGraphType ogt:
                    return ogt.AsEnumerable();

                default:
                    return Enumerable.Empty<IObjectGraphType>();
            }
        }

        /// <summary>
        /// Attempts to find an <see cref="IGraphType" /> currently associated with the given concrete type. Returns null
        /// if no <see cref="IGraphType" /> is found.
        /// </summary>
        /// <param name="concreteType">The concrete type to search for.</param>
        /// <param name="kind">The graph type to search for an association of.</param>
        /// <returns>IGraphType.</returns>
        public IGraphType FindGraphType(Type concreteType, TypeKind kind)
        {
            this.DenySearchForListAndKVP(concreteType);
            return _concreteTypes.FindGraphType(concreteType, kind);
        }

        /// <summary>
        /// Attempts to find an <see cref="IGraphType" /> with the given name. Returns null
        /// if no <see cref="IGraphType" /> is found.
        /// </summary>
        /// <param name="graphTypeName">The name of the type in the object graph.</param>
        /// <returns>IGraphType.</returns>
        public IGraphType FindGraphType(string graphTypeName)
        {
            if (_graphTypesByName.TryGetValue(graphTypeName, out var graphType))
                return graphType;

            return null;
        }

        /// <summary>
        /// Finds the graph type, if any, associated with the given field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>IGraphType.</returns>
        public IGraphType FindGraphType(IGraphField field)
        {
            return this.FindGraphType(field?.TypeExpression?.TypeName);
        }

        /// <summary>
        /// Finds the graph type, if any, associated with the object instance.
        /// </summary>
        /// <param name="data">The data object to search with.</param>
        /// <returns>IGraphType.</returns>
        public IGraphType FindGraphType(object data)
        {
            if (data == null)
                return null;

            if (data is Type type)
                return this.FindGraphType(type, TypeKind.OBJECT);

            if (data is VirtualResolvedObject virtualFieldObject)
            {
                return this.FindGraphType(virtualFieldObject.GraphTypeName);
            }

            return this.FindGraphType(data.GetType(), TypeKind.OBJECT);
        }

        /// <summary>
        /// Finds the concrete type related to the supplied graph type. returns null if no types are found.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        /// <returns>Type.</returns>
        public Type FindConcreteType(IGraphType graphType)
        {
            return _concreteTypes.FindType(graphType);
        }

        /// <summary>
        /// Attempts to find a single directive within this schema by its name. Returns null
        /// if the directive is not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IDirectiveGraphType.</returns>
        public IDirectiveGraphType FindDirective(string name)
        {
            return this.FindGraphType(name) as IDirectiveGraphType;
        }

        /// <summary>
        /// Retrieves the collection of graph types that implement the provided named interface,
        /// if any.
        /// </summary>
        /// <param name="interfaceType">The interface type type to look for.</param>
        /// <returns>IEnumerable&lt;IGraphType&gt;.</returns>
        public IEnumerable<IObjectGraphType> FindGraphTypesByInterface(IInterfaceGraphType interfaceType)
        {
            if (interfaceType == null)
                return Enumerable.Empty<IObjectGraphType>();
            else
                return this.FindGraphTypesByInterface(interfaceType.Name);
        }

        /// <summary>
        /// Retrieves the collection of graph types that implement the provided named interface
        /// if any.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <returns>IEnumerable&lt;IGraphType&gt;.</returns>
        public IEnumerable<IObjectGraphType> FindGraphTypesByInterface(string interfaceName)
        {
            return _extendableGraphTypeTracker.FindGraphTypesByInterface(interfaceName);
        }

        /// <summary>
        /// Throws an exception if the item being searched on is a list or KeyValuePair item managed by
        /// the library.
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        private void DenySearchForListAndKVP(Type concreteType)
        {
            if (concreteType == null)
                return;

            if (GraphQLProviders.ScalarProvider.IsScalar(concreteType))
                return;

            if (concreteType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(concreteType))
            {
                throw new GraphTypeDeclarationException(
                    $"Graph Type Mismatch, {concreteType.FriendlyName()}. Collections and KeyValuePair enumerable types " +
                    "cannot be directly searched. Instead, search for the types supplied to the generic type declaration. ");
            }
        }

        /// <summary>
        /// Determines whether the specified graph type name exists in this collection.
        /// </summary>
        /// <param name="graphTypeName">Name of the graph type.</param>
        /// <returns><c>true</c> if the specified graph type name contains key; otherwise, <c>false</c>.</returns>
        public bool Contains(string graphTypeName)
        {
            return _graphTypesByName.ContainsKey(graphTypeName);
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="Type" /> refrence for the
        /// provided concrete type as an object type reference (different from an input type).
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        /// <param name="kind">The kind of graph type to create from the supplied concrete type. If not supplied the concrete type will
        /// attempt to auto assign a type of scalar, enum or object as necessary.</param>
        /// <returns><c>true</c> if the type collection contains a reference to the concrete type for the given kind; otherwise, <c>false</c>.</returns>
        public bool Contains(Type concreteType, TypeKind? kind = null)
        {
            return _concreteTypes.Contains(concreteType, kind);
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="IGraphType" /> as an object type reference (different from an input type).
        /// </summary>
        /// <param name="graphType">the graph type to search for.</param>
        /// <returns><c>true</c> if the graph type is found; otherwise, <c>false</c>.</returns>
        public bool Contains(IGraphType graphType)
        {
            if (_graphTypesByName.TryGetValue(graphType.Name, out var foundType))
                return foundType == graphType;

            return false;
        }

        /// <summary>
        /// Gets the total number of <see cref="IGraphType"/> in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _graphTypesByName.Count;

        /// <summary>
        /// Gets the count of queued <see cref="IGraphField"/>.
        /// </summary>
        /// <value>The un registered field count.</value>
        public int QueuedExtensionFieldCount => _typeQueue.FieldCount;

        /// <summary>
        /// Gets the total count of concrete types in this collection.
        /// </summary>
        /// <value>The type references.</value>
        public IEnumerable<Type> TypeReferences => _concreteTypes.Types;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGraphType> GetEnumerator()
        {
            return _graphTypesByName.Values.GetEnumerator();
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