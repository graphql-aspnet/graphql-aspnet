// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A collection of event names to full path names for any given constructed schema.
    /// </summary>
    public static class SchemaSubscriptionEventMap
    {
        private static readonly ConcurrentHashSet<Type> PARSED_SCHEMA_TYPES;
        private static readonly ConcurrentDictionary<SubscriptionEventName, GraphFieldPath> NAME_CATALOG;

        /// <summary>
        /// Initializes static members of the <see cref="SchemaSubscriptionEventMap"/> class.
        /// </summary>
        static SchemaSubscriptionEventMap()
        {
            NAME_CATALOG = new ConcurrentDictionary<SubscriptionEventName, GraphFieldPath>(SubscriptionEventNameEqualityComparer.Instance);
            PARSED_SCHEMA_TYPES = new ConcurrentHashSet<Type>();
        }

        /// <summary>
        /// Clears all locally cacehed subscription event names.
        /// </summary>
        internal static void ClearCache()
        {
            NAME_CATALOG.Clear();
        }

        /// <summary>
        /// Attempts to generate a dictionary of value relating field path to the possible event names.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>Dictionary&lt;System.String, GraphFieldPath&gt;.</returns>
        public static Dictionary<SubscriptionEventName, GraphFieldPath> CreateEventMap(ISchema schema)
        {
            var dic = new Dictionary<SubscriptionEventName, GraphFieldPath>(SubscriptionEventNameEqualityComparer.Instance);
            if (schema == null || !schema.OperationTypes.ContainsKey(GraphCollection.Subscription))
                return dic;

            foreach (var field in schema.KnownTypes.OfType<IObjectGraphType>()
                .SelectMany(x => x.Fields.OfType<ISubscriptionGraphField>()))
            {
                var route = field.Route.Clone();
                foreach (var eventName in SubscriptionEventName.FromGraphField(schema, field))
                {
                    if (dic.ContainsKey(eventName))
                    {
                        var path = dic[eventName];
                        throw new DuplicateNameException(
                            $"Duplciate Subscription Event Name. Unable to register the field '{route.Path}' " +
                            $"with event name '{eventName}'. The schema '{schema.GetType().FriendlyName()}' already contains " +
                            $"a field with the event name '{eventName}'. (Event Owner: {path.Path}).");
                    }

                    dic.Add(eventName, route);
                }
            }

            return dic;
        }

        /// <summary>
        /// Parses the schema to preload the subscription event map instead of waiting for the first invocation.
        /// </summary>
        /// <param name="schema">The schema to load.</param>
        /// <returns><c>true</c> if the schema was just now loaded, <c>false</c> if it was loaded previously.</returns>
        internal static bool PreLoadSubscriptionEventNames(this ISchema schema)
        {
            if (schema == null || PARSED_SCHEMA_TYPES.Contains(schema.GetType()))
                return false;

            PARSED_SCHEMA_TYPES.Add(schema.GetType());
            foreach (var kvp in CreateEventMap(schema))
                NAME_CATALOG.TryAdd(kvp.Key, kvp.Value);

            return true;
        }

        /// <summary>
        /// Attempts to find the fully qualifed <see cref="GraphFieldPath"/> that is pointed at by the supplied event name.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="eventName">The short or long name of the event.</param>
        /// <returns>System.String.</returns>
        public static GraphFieldPath RetrieveSubscriptionFieldPath(this ISchema schema, SubscriptionEventName eventName)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            Validation.ThrowIfNull(eventName, nameof(eventName));

            if (eventName.OwnerSchemaType != schema.FullyQualifiedSchemaTypeName())
                return null;

            if (NAME_CATALOG.TryGetValue(eventName, out var routePath))
                return routePath;

            // attempt to load the schema into the cache in case it wasnt before
            if (PreLoadSubscriptionEventNames(schema))
            {
                if (NAME_CATALOG.TryGetValue(eventName, out routePath))
                    return routePath;
            }

            return null;
        }
    }
}