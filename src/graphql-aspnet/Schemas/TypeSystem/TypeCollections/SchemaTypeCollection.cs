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
        private readonly SchemaRuntimeTypeAnalyzer _typeMatchProcessor;
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
            _typeMatchProcessor = new SchemaRuntimeTypeAnalyzer(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IGraphType FindGraphType(Type concreteType, TypeKind kind)
        {
            this.DenySearchForListAndKVP(concreteType);
            return _concreteTypes.FindGraphType(concreteType, kind);
        }

        /// <inheritdoc />
        public IGraphType FindGraphType(string graphTypeName)
        {
            if (_graphTypesByName.TryGetValue(graphTypeName, out var graphType))
                return graphType;

            return null;
        }

        /// <inheritdoc />
        public IGraphType FindGraphType(IGraphField field)
        {
            return this.FindGraphType(field?.TypeExpression?.TypeName);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Type FindConcreteType(IGraphType graphType)
        {
            return _concreteTypes.FindType(graphType);
        }

        /// <inheritdoc />
        public IEnumerable<Type> FindConcreteTypes(params IGraphType[] graphTypes)
        {
            var list = new List<Type>();
            foreach (var graphType in graphTypes)
            {
                foreach (var egt in this.ExpandAbstractType(graphType))
                {
                    var type = this.FindConcreteType(egt);
                    if (type != null)
                        list.Add(type);
                }
            }

            return list;
        }

        /// <inheritdoc />
        public SchemaConcreteTypeAnalysisResult AnalyzeRuntimeConcreteType(IGraphType targetGraphType, Type typeToCheck)
        {
            Validation.ThrowIfNull(targetGraphType, nameof(targetGraphType));
            Validation.ThrowIfNull(typeToCheck, nameof(typeToCheck));

            if (!this.Contains(targetGraphType))
                return new SchemaConcreteTypeAnalysisResult(targetGraphType, typeToCheck, new Type[0]);

            this.DenySearchForListAndKVP(typeToCheck);

            var result = _typeMatchProcessor.FindAllowedTypes(targetGraphType, typeToCheck);
            return new SchemaConcreteTypeAnalysisResult(targetGraphType, typeToCheck, result);
        }

        /// <inheritdoc />
        public IDirectiveGraphType FindDirective(string name)
        {
            return this.FindGraphType(name) as IDirectiveGraphType;
        }

        /// <inheritdoc />
        public IEnumerable<IObjectGraphType> FindGraphTypesByInterface(IInterfaceGraphType interfaceType)
        {
            if (interfaceType == null)
                return Enumerable.Empty<IObjectGraphType>();
            else
                return this.FindGraphTypesByInterface(interfaceType.Name);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Contains(string graphTypeName)
        {
            return _graphTypesByName.ContainsKey(graphTypeName);
        }

        /// <inheritdoc />
        public bool Contains(Type concreteType, TypeKind? kind = null)
        {
            return _concreteTypes.Contains(concreteType, kind);
        }

        /// <inheritdoc />
        public bool Contains(IGraphType graphType)
        {
            if (_graphTypesByName.TryGetValue(graphType.Name, out var foundType))
                return foundType == graphType;

            return false;
        }

        /// <inheritdoc />
        public int Count => _graphTypesByName.Count;

        /// <inheritdoc />
        public int QueuedExtensionFieldCount => _typeQueue.FieldCount;

        /// <inheritdoc />
        public IEnumerable<Type> TypeReferences => _concreteTypes.Types;

        /// <inheritdoc />
        public IEnumerator<IGraphType> GetEnumerator()
        {
            return _graphTypesByName.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}