// *********************************************
// ****************
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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of event names to full path names for any given constructed schema.
    /// </summary>
    public static class SchemaSubscriptionEventMap
    {
        private static readonly ConcurrentHashSet<Type> PARSED_SCHEMA_TYPES;
        private static readonly Dictionary<SubscriptionEventName, SchemaItemPath> SUBSCRIPTION_EVENTNAME_CATALOG;
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Initializes static members of the <see cref="SchemaSubscriptionEventMap"/> class.
        /// </summary>
        static SchemaSubscriptionEventMap()
        {
            SUBSCRIPTION_EVENTNAME_CATALOG = new Dictionary<SubscriptionEventName, SchemaItemPath>(SubscriptionEventNameEqualityComparer.Instance);
            PARSED_SCHEMA_TYPES = new ConcurrentHashSet<Type>();
        }

        /// <summary>
        /// Clears all locally cacehed subscription event names.
        /// </summary>
        internal static void ClearCache()
        {
            SUBSCRIPTION_EVENTNAME_CATALOG.Clear();
        }

        /// <summary>
        /// Attempts to generate a dictionary of value relating field path to the possible event names.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>Dictionary&lt;System.String, SchemaItemPath&gt;.</returns>
        public static Dictionary<SubscriptionEventName, SchemaItemPath> CreateEventMap(ISchema schema)
        {
            var dic = new Dictionary<SubscriptionEventName, SchemaItemPath>(SubscriptionEventNameEqualityComparer.Instance);
            if (schema == null || !schema.Operations.ContainsKey(GraphOperationType.Subscription))
                return dic;

            foreach (var field in schema.KnownTypes.OfType<IObjectGraphType>()
                .SelectMany(x => x.Fields.OfType<ISubscriptionGraphField>()))
            {
                var route = field.Route.Clone();

                var eventName = SubscriptionEventName.FromGraphField(schema, field);
                if (dic.ContainsKey(eventName))
                {
                    var path = dic[eventName];
                    throw new DuplicateNameException(
                        $"Duplciate Subscription Event Name. Unable to register the field '{route.Path}' " +
                        $"with event name '{eventName}'. The schema '{schema.Name}' already contains " +
                        $"a field with the event name '{eventName}'. (Event Owner: {path.Path}).");
                }

                dic.Add(eventName, route);

            }

            return dic;
        }

        /// <summary>
        /// Attempts to find the fully qualifed <see cref="SchemaItemPath"/> that is pointed at by the supplied event name.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="eventName">The short or long name of the event.</param>
        /// <returns>System.String.</returns>
        public static SchemaItemPath RetrieveSubscriptionFieldPath(this ISchema schema, SubscriptionEventName eventName)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            Validation.ThrowIfNull(eventName, nameof(eventName));

            if (eventName.OwnerSchemaType != schema.FullyQualifiedSchemaTypeName())
                return null;

            // parse and cache the schema's known fields into a set of event names
            if (!PARSED_SCHEMA_TYPES.Contains(schema.GetType()))
            {
                lock (_syncLock)
                {
                    if (!PARSED_SCHEMA_TYPES.Contains(schema.GetType()))
                    {
                        foreach (var kvp in CreateEventMap(schema))
                            SUBSCRIPTION_EVENTNAME_CATALOG.Add(kvp.Key, kvp.Value);

                        PARSED_SCHEMA_TYPES.Add(schema.GetType());
                    }
                }
            }

            if (SUBSCRIPTION_EVENTNAME_CATALOG.TryGetValue(eventName, out var routePath))
                return routePath;

            return null;
        }
    }
}