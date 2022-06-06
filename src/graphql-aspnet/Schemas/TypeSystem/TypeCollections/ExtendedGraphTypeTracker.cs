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
    /// <para>A entity responsible for ensuring continuity between interface graph types
    /// and the object types that implement them in the graph.
    /// If an interface is extended by user code any graph types made from concrete types
    /// that implement that interface are also extended.
    /// </para>
    /// <para>
    /// (Oct 2021) Similarly, if an interface implements a newly added interface then the
    /// fields on the interface's graph type are extended to include those of the
    /// newly added interface.
    /// </para>
    /// </summary>
    internal sealed class ExtendedGraphTypeTracker
    {
        private readonly ConcurrentDictionary<string, IInterfaceGraphType> _interfacesByName;
        private readonly ConcurrentDictionary<string, HashSet<IGraphType>> _graphTypesByInterfaceName;
        private readonly HashSet<IGraphType> _allKnownTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedGraphTypeTracker" /> class.
        /// </summary>
        public ExtendedGraphTypeTracker()
        {
            _interfacesByName = new ConcurrentDictionary<string, IInterfaceGraphType>();
            _graphTypesByInterfaceName = new ConcurrentDictionary<string, HashSet<IGraphType>>();
            _allKnownTypes = new HashSet<IGraphType>(GraphTypeEqualityComparer.Instance);
        }

        /// <summary>
        /// Finds the graph types known to this instance to have implemented the given interface.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <returns>IEnumerable&lt;IGraphType&gt;.</returns>
        public IEnumerable<IGraphType> FindGraphTypesByInterface(string interfaceName)
        {
            if (!string.IsNullOrWhiteSpace(interfaceName) && _graphTypesByInterfaceName.ContainsKey(interfaceName))
                return _graphTypesByInterfaceName[interfaceName];

            return Enumerable.Empty<IGraphType>();
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
            this.MonitorGraphTypeInternal(graphType);

            if (graphType is IExtendableGraphType extendableType)
                this.ApplyField(extendableType, newField);

            this.Syncronize(graphType);
        }

        private void ApplyField(IExtendableGraphType graphType, IGraphField field)
        {
            var fieldClone = field.Clone(graphType);
            graphType.Extend(fieldClone);
        }

        /// <summary>
        /// Adds the supplied graph type to the collection and begins tracking it for changes and applies any
        /// changes that may affect it. Non-extendable graph types are safely filtered out and ignored.
        /// </summary>
        /// <param name="graphType">The graph type to begin watching.</param>
        public void MonitorGraphType(IGraphType graphType)
        {
            this.MonitorGraphTypeInternal(graphType);
            this.Syncronize(graphType);
        }

        private void MonitorGraphTypeInternal(IGraphType graphType)
        {
            if (!_allKnownTypes.Contains(graphType))
            {
                _allKnownTypes.Add(graphType);

                if (graphType is IInterfaceGraphType ifaceType)
                    _interfacesByName.TryAdd(ifaceType.Name, ifaceType);

                if (graphType is IInterfaceContainer ifaceContainer)
                {
                    foreach (var ifaceName in ifaceContainer.InterfaceNames)
                    {
                        _graphTypesByInterfaceName.TryAdd(ifaceName, new HashSet<IGraphType>());
                        _graphTypesByInterfaceName[ifaceName].Add(graphType);
                    }
                }
            }
        }

        private void Syncronize(IGraphType newlyAddedType)
        {
            // if the type implements interfaces
            // sync those interfaces already known into the type
            if (newlyAddedType is IInterfaceContainer iic)
            {
                foreach (var interfaceName in iic.InterfaceNames)
                {
                    if (_interfacesByName.ContainsKey(interfaceName))
                        this.ApplyInterface(newlyAddedType, _interfacesByName[interfaceName]);
                }
            }

            // if the type is an interface
            // sync it into any known types
            if (newlyAddedType is IInterfaceGraphType igt)
            {
                if (_graphTypesByInterfaceName.ContainsKey(igt.Name))
                {
                    foreach (var type in _graphTypesByInterfaceName[igt.Name])
                        this.ApplyInterface(type, igt);
                }
            }
        }

        private void ApplyInterface(IGraphType graphType, IInterfaceGraphType interfaceGraphType)
        {
            if (graphType is IExtendableGraphType egt)
            {
                foreach (var field in interfaceGraphType.Fields)
                {
                    if (!egt.Fields.ContainsKey(field.Name))
                        this.ApplyField(egt, field);
                }
            }
        }
    }
}