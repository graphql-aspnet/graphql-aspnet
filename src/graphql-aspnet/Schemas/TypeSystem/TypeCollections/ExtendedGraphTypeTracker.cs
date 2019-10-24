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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection responsible for ensuring continutiy between interface graph types and the object types that
    /// implement them in the graph. If an interface is extended by user code any graph types made from concrete types
    /// that implement that interface are also extended.
    /// </summary>
    internal class ExtendedGraphTypeTracker
    {
        // a collection of extendable types that implement an interface not yet known to this instance
        // if an interface is later added these types will be automatically assigned to it for extendability
        private readonly ConcurrentDictionary<string, HashSet<IObjectGraphType>> _queuedTypesByInterfaceName;

        private readonly ConcurrentDictionary<string, IInterfaceGraphType> _interfacesByName;
        private readonly ConcurrentDictionary<IInterfaceGraphType, HashSet<IObjectGraphType>> _graphTypesByInterface;
        private readonly ConcurrentDictionary<IInterfaceGraphType, HashSet<IGraphField>> _fieldsByInterface;
        private readonly HashSet<IGraphType> _allKnownTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedGraphTypeTracker" /> class.
        /// </summary>
        public ExtendedGraphTypeTracker()
        {
            _interfacesByName = new ConcurrentDictionary<string, IInterfaceGraphType>();
            _graphTypesByInterface = new ConcurrentDictionary<IInterfaceGraphType, HashSet<IObjectGraphType>>();
            _fieldsByInterface = new ConcurrentDictionary<IInterfaceGraphType, HashSet<IGraphField>>();
            _queuedTypesByInterfaceName = new ConcurrentDictionary<string, HashSet<IObjectGraphType>>();
            _allKnownTypes = new HashSet<IGraphType>(GraphTypeEqualityComparer.Instance);
        }

        /// <summary>
        /// Finds the graph types known to this instance to have implemented the given interface.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <returns>IEnumerable&lt;IGraphType&gt;.</returns>
        public IEnumerable<IObjectGraphType> FindGraphTypesByInterface(string interfaceName)
        {
            if (!string.IsNullOrWhiteSpace(interfaceName) &&
                _interfacesByName.TryGetValue(interfaceName, out var interfaceType))
            {
                return _graphTypesByInterface[interfaceType];
            }

            return Enumerable.Empty<IObjectGraphType>();
        }

        /// <summary>
        /// Adds the field extension to the associated graph type if possible. If the type is an interface the field is also
        /// added to any graph types that implement the interface. The field is also added to the tracker such that if any
        /// future graph types are added to this instance, that also implement the interface, the field is automatically added to them as well.
        /// If the graph type cannot accept new fields the addition is ignored and no exception is thrown.
        /// </summary>
        /// <param name="graphType">The graph type that will accept the new field.</param>
        /// <param name="newField">The new field to add.</param>
        public void AddFieldExtension(IGraphType graphType, IGraphField newField)
        {
            if (graphType is IInterfaceGraphType interfaceType)
                this.AddInterfaceField(interfaceType, newField);
            else if (graphType is IExtendableGraphType extendableType)
                extendableType.Extend(newField);
        }

        /// <summary>
        /// Adds the new field to the interface and to any object types associated with the interface.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="newField">The new field.</param>
        private void AddInterfaceField(IInterfaceGraphType interfaceType, IGraphField newField)
        {
            this.EnsureInterfaceIsTracked(interfaceType);

            interfaceType.Extend(newField);
            foreach (var objectType in _graphTypesByInterface[interfaceType])
            {
                objectType.Extend(newField);
            }
        }

        /// <summary>
        /// Ensures the interface is known to this instance and is primed to accept fields and apply
        /// them to other graph types appropriately.
        /// </summary>
        /// <param name="interfaceType">The interface to include in this tracker.</param>
        private void EnsureInterfaceIsTracked(IInterfaceGraphType interfaceType)
        {
            _allKnownTypes.Add(interfaceType);
            _fieldsByInterface.TryAdd(interfaceType, new HashSet<IGraphField>());
            _interfacesByName.TryAdd(interfaceType.Name, interfaceType);
            _graphTypesByInterface.TryAdd(interfaceType, new HashSet<IObjectGraphType>());

            // if any extendable types are queued for this new interface
            // ensure they are converted to being tracked
            if (_queuedTypesByInterfaceName.TryRemove(interfaceType.Name, out var objectGraphTypes))
            {
                foreach (var type in objectGraphTypes)
                {
                    _graphTypesByInterface[interfaceType].Add(type);
                }
            }
        }

        /// <summary>
        /// Ensures the object graph type is tracked against all its implemented interfaces and queued
        /// to be added to the interfaces it declares that are not known to this instance.
        /// </summary>
        /// <param name="objectGraphType">The object graph type to watch.</param>
        private void EnsureObjectTypeIsTracked(IObjectGraphType objectGraphType)
        {
            _allKnownTypes.Add(objectGraphType);
            foreach (var interfaceName in objectGraphType.InterfaceNames)
            {
                if (_interfacesByName.TryGetValue(interfaceName, out var interfaceType))
                {
                    _graphTypesByInterface[interfaceType].Add(objectGraphType);
                    this.ApplyAllFields(interfaceType, objectGraphType);
                }
                else
                {
                    if (!_queuedTypesByInterfaceName.TryGetValue(interfaceName, out var _))
                        _queuedTypesByInterfaceName.TryAdd(interfaceName, new HashSet<IObjectGraphType>());

                    _queuedTypesByInterfaceName[interfaceName].Add(objectGraphType);
                }
            }
        }

        /// <summary>
        /// Applies all fields known extension fields for the interface to the given object graph type.
        /// </summary>
        /// <param name="interfaceType">The interface type to search for associated fields.</param>
        /// <param name="objectGraphType">The object graph type to recieve new fields.</param>
        private void ApplyAllFields(IInterfaceGraphType interfaceType, IObjectGraphType objectGraphType)
        {
            foreach (var field in _fieldsByInterface[interfaceType])
                objectGraphType.Extend(field);
        }

        /// <summary>
        /// Adds the supplied graph type to the collection and begins tracking it for changes and applies any
        /// changes that may effect it. Non-extendable graph types are safely filtered out and ignored.
        /// </summary>
        /// <param name="graphType">The graph type to begin watching.</param>
        public void MonitorGraphType(IGraphType graphType)
        {
            if (_allKnownTypes.Contains(graphType))
                return;

            if (graphType is IInterfaceGraphType interfaceType)
                this.EnsureInterfaceIsTracked(interfaceType);
            else if (graphType is IObjectGraphType objType)
                this.EnsureObjectTypeIsTracked(objType);
        }
    }
}