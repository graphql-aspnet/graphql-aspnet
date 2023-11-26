// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of event names to full path names for any given constructed schema.
    /// </summary>
    public static class SubscriptionEventSchemaMap
    {
        private static readonly ConcurrentHashSet<Type> PARSED_SCHEMA_TYPES;
        private static readonly ConcurrentDictionary<SubscriptionEventName, ItemPath> SUBSCRIPTION_EVENTNAME_CATALOG;
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Initializes static members of the <see cref="SubscriptionEventSchemaMap"/> class.
        /// </summary>
        static SubscriptionEventSchemaMap()
        {
            SUBSCRIPTION_EVENTNAME_CATALOG = new (SubscriptionEventNameEqualityComparer.Instance);
            PARSED_SCHEMA_TYPES = new ConcurrentHashSet<Type>();
        }

        /// <summary>
        /// Clears all locally cached subscription event names from all schemas.
        /// </summary>
        public static void ClearCache()
        {
            lock (_syncLock)
            {
                SUBSCRIPTION_EVENTNAME_CATALOG.Clear();
                PARSED_SCHEMA_TYPES.Clear();
            }
        }

        /// <summary>
        /// Parses the subscription events registered to the schema and validates their correctness.
        /// If an anomaly is detected and exception is thrown.
        /// </summary>
        /// <param name="schema">The schema to validate.</param>
        public static void EnsureSubscriptionEventsOrThrow(ISchema schema)
        {
            Validation.ThrowIfNull(schema, nameof(schema));

            // parse and cache the schema's known fields into a set of event names
            if (!PARSED_SCHEMA_TYPES.Contains(schema.GetType()))
            {
                lock (_syncLock)
                {
                    if (!PARSED_SCHEMA_TYPES.Contains(schema.GetType()))
                    {
                        foreach (var kvp in CreateEventMap(schema))
                            SUBSCRIPTION_EVENTNAME_CATALOG.TryAdd(kvp.Key, kvp.Value);

                        PARSED_SCHEMA_TYPES.Add(schema.GetType());
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to generate a dictionary of value relating field path to the possible
        /// event names. An exception will be thrown if a duplicate name is detected.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>Dictionary&lt;System.String, SchemaItemPath&gt;.</returns>
        public static Dictionary<SubscriptionEventName, ItemPath> CreateEventMap(ISchema schema)
        {
            var dic = new Dictionary<SubscriptionEventName, ItemPath>();

            if (schema == null || !schema.Operations.ContainsKey(GraphOperationType.Subscription))
                return dic;

            foreach (var field in schema.KnownTypes.OfType<IObjectGraphType>()
                .SelectMany(x => x.Fields.OfType<ISubscriptionGraphField>()))
            {
                var route = field.ItemPath.Clone();

                var eventName = SubscriptionEventName.FromGraphField(schema, field);
                if (dic.ContainsKey(eventName))
                {
                    var path = dic[eventName];
                    throw new GraphTypeDeclarationException(
                        $"Duplciate Subscription Event Name. Unable to register the field '{route.Path}' " +
                        $"with event name '{eventName.EventName}'. The schema '{schema.Name}' already contains " +
                        $"a field with the event name '{eventName.EventName}'. (Event Owner: {path.Path}).");
                }

                dic.Add(eventName, route);
            }

            return dic;
        }

        /// <summary>
        /// Attempts to find the fully qualifed <see cref="ItemPath" /> that is pointed at by the supplied event name.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="eventName">The formally named event.</param>
        /// <returns>SchemaItemPath.</returns>
        public static ItemPath RetrieveSubscriptionFieldPath(this ISchema schema, SubscriptionEventName eventName)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            Validation.ThrowIfNull(eventName, nameof(eventName));

            EnsureSubscriptionEventsOrThrow(schema);
            if (SUBSCRIPTION_EVENTNAME_CATALOG.TryGetValue(eventName, out var routePath))
                return routePath;

            return null;
        }
    }
}