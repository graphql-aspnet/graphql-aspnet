// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Directives
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of searchable events that define the available life cycle
    /// events that can be employed. Used during templating and validation of
    /// candidate directives.
    /// </summary>
    internal class DirectiveLifeCycleEvents : IEnumerable<DirectiveLifeCycleEventItem>
    {
        /// <summary>
        /// Gets the single instance containing all the known lifecycle events.
        /// </summary>
        /// <value>The instance.</value>
        public static DirectiveLifeCycleEvents Instance { get; }

        /// <summary>
        /// Initializes static members of the <see cref="DirectiveLifeCycleEvents"/> class.
        /// </summary>
        static DirectiveLifeCycleEvents()
        {
            Instance = new DirectiveLifeCycleEvents();
        }

        private IList<DirectiveLifeCycleEventItem> _allItems;
        private IDictionary<DirectiveLocation, IList<DirectiveLifeCycleEventItem>> _itemsByLocation;
        private IDictionary<DirectiveLifeCyclePhase, IList<DirectiveLifeCycleEventItem>> _itemsByPhase;
        private IDictionary<string, DirectiveLifeCycleEventItem> _itemsByName;
        private IDictionary<DirectiveLifeCycleEvent, DirectiveLifeCycleEventItem> _itemsByEvent;

        /// <summary>
        /// Prevents a default instance of the <see cref="DirectiveLifeCycleEvents"/> class from being created.
        /// </summary>
        private DirectiveLifeCycleEvents()
        {
            _allItems = new List<DirectiveLifeCycleEventItem>();
            _itemsByLocation = new Dictionary<DirectiveLocation, IList<DirectiveLifeCycleEventItem>>();
            _itemsByPhase = new Dictionary<DirectiveLifeCyclePhase, IList<DirectiveLifeCycleEventItem>>();
            _itemsByEvent = new Dictionary<DirectiveLifeCycleEvent, DirectiveLifeCycleEventItem>();
            _itemsByName = new Dictionary<string, DirectiveLifeCycleEventItem>();

            var executionPhase = DirectiveLocation.QUERY | DirectiveLocation.MUTATION |
                DirectiveLocation.SUBSCRIPTION | DirectiveLocation.FIELD |
                DirectiveLocation.FRAGMENT_DEFINITION | DirectiveLocation.FRAGMENT_SPREAD |
                DirectiveLocation.INLINE_FRAGMENT;

            var typeBuildingPhase = DirectiveLocation.SCHEMA | DirectiveLocation.SCALAR |
                DirectiveLocation.OBJECT | DirectiveLocation.FIELD_DEFINITION |
                DirectiveLocation.ARGUMENT_DEFINITION | DirectiveLocation.INTERFACE |
                DirectiveLocation.UNION | DirectiveLocation.ENUM |
                DirectiveLocation.ENUM_VALUE | DirectiveLocation.INPUT_OBJECT |
                DirectiveLocation.INPUT_FIELD_DEFINITION;

            this.AddEvent(
                typeBuildingPhase,
                DirectiveLifeCycleEvent.AlterTypeSystem,
                DirectiveLifeCyclePhase.SchemaBuilding,
                Constants.ReservedNames.DIRECTIVE_ALTER_TYPE_SYSTEM_METHOD_NAME,
                new List<Type>() { typeof(ISchemaItem) });

            var beforeFieldResolution = this.AddEvent(
                executionPhase,
                DirectiveLifeCycleEvent.BeforeResolution,
                DirectiveLifeCyclePhase.Execution,
                Constants.ReservedNames.DIRECTIVE_BEFORE_RESOLUTION_METHOD_NAME);

            var afterFieldResolution = this.AddEvent(
                executionPhase,
                DirectiveLifeCycleEvent.AfterResolution,
                DirectiveLifeCyclePhase.Execution,
                Constants.ReservedNames.DIRECTIVE_AFTER_RESOLUTION_METHOD_NAME);

            beforeFieldResolution.AddSybling(afterFieldResolution);
            afterFieldResolution.AddSybling(beforeFieldResolution);
        }

        private DirectiveLifeCycleEventItem AddEvent(
            DirectiveLocation location,
            DirectiveLifeCycleEvent evt,
            DirectiveLifeCyclePhase phase,
            string methodName,
            List<Type> requiredTypes = null)
        {
            var item = new DirectiveLifeCycleEventItem(
                location,
                evt,
                phase,
                methodName,
                requiredTypes);

            _allItems.Add(item);
            _itemsByName.Add(item.MethodName, item);
            _itemsByEvent.Add(item.Event, item);

            // catalog by location
            var flags = location.GetIndividualFlags<DirectiveLocation>()
                .Where(x => x != DirectiveLocation.NONE);
            foreach (DirectiveLocation flag in flags)
            {
                if (!_itemsByLocation.ContainsKey(flag))
                    _itemsByLocation.Add(flag, new List<DirectiveLifeCycleEventItem>());
                _itemsByLocation[flag].Add(item);
            }

            // catalog by Phase
            if (!_itemsByPhase.ContainsKey(phase))
                _itemsByPhase.Add(phase, new List<DirectiveLifeCycleEventItem>());
            _itemsByPhase[phase].Add(item);

            return item;
        }

        /// <summary>
        /// Gets the event items coorisponding to the specified phase.
        /// </summary>
        /// <param name="phase">The phase to retrieve.</param>
        /// <returns>IEnumerable&lt;DirectiveLifeCycleEventItem&gt;.</returns>
        public IEnumerable<DirectiveLifeCycleEventItem> this[DirectiveLifeCyclePhase phase]
        {
            get
            {
                if (_itemsByPhase.ContainsKey(phase))
                    return _itemsByPhase[phase];

                return Enumerable.Empty<DirectiveLifeCycleEventItem>();
            }
        }

        /// <summary>
        /// Gets the single event item  coorisponding to the declared event.
        /// If no match is found, null is returned.
        /// </summary>
        /// <param name="evt">The event defined by the item.</param>
        /// <returns>DirectiveLifeCycleEventItem.</returns>
        public DirectiveLifeCycleEventItem this[DirectiveLifeCycleEvent evt]
        {
            get
            {
                if (_itemsByEvent.ContainsKey(evt))
                    return _itemsByEvent[evt];

                return null;
            }
        }

        /// <summary>
        /// Gets the set of events that are applicable to the bitwise locations
        /// provided.
        /// </summary>
        /// <param name="location">The bitwise location flags to retrieve for.</param>
        /// <returns>DirectiveLifeCycleEventItem.</returns>
        public IEnumerable<DirectiveLifeCycleEventItem> this[DirectiveLocation location]
        {
            get
            {
                var flags = location.GetIndividualFlags<DirectiveLocation>()
                    .Where(x => x != DirectiveLocation.NONE);

                foreach (var flag in flags)
                {
                    if (_itemsByLocation.ContainsKey(flag))
                    {
                        foreach (var item in _itemsByLocation[flag])
                            yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the single event item  with the given method name.
        /// If no match is found, null is returned.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>DirectiveLifeCycleEventItem.</returns>
        public DirectiveLifeCycleEventItem this[string methodName]
        {
            get
            {
                if (_itemsByName.ContainsKey(methodName))
                    return _itemsByName[methodName];

                return null;
            }
        }

        /// <inheritdoc />
        public IEnumerator<DirectiveLifeCycleEventItem> GetEnumerator()
        {
            return _allItems.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}