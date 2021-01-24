// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems;

    /// <summary>
    /// Recorded when the startup service generates a new schema instance.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema that was generated.</typeparam>
    public class SchemaInstanceCreatedLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaInstanceCreatedLogEntry{TSchema}"/> class.
        /// </summary>
        /// <param name="schemaInstance">The schema instance.</param>
        public SchemaInstanceCreatedLogEntry(TSchema schemaInstance)
            : base(LogEventIds.SchemaInstanceCreated)
        {
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SchemaInstanceName = schemaInstance.Name;
            this.SchemaGraphTypeCount = schemaInstance.KnownTypes.Count;
            this.SchemaSupportedOperationTypes = schemaInstance
                .OperationTypes
                .OrderBy(x => x.Key)
                .Select(x => x.Key.ToString().ToLower())
                .ToList();

            // sort introspected types to the top
            // then by kind
            // then by name
            var orderedList = schemaInstance.KnownTypes
                .OrderBy(x => x.Name.StartsWith("__") ? -1 : 1)
                .ThenBy(x => x.Kind.ToString())
                .ThenBy(x => x.Name);

            var graphTypeItems = new List<IGraphLogPropertyCollection>();
            foreach (var graphType in orderedList)
            {
                var concreteType = schemaInstance.KnownTypes.FindConcreteType(graphType);
                graphTypeItems.Add(new SchemaGraphTypeLogItem(graphType, concreteType));
            }

            this.GraphTypes = graphTypeItems;
        }

        /// <summary>
        /// Gets a count of the number of <see cref="IGraphType"/> registered to the schema instance.
        /// </summary>
        /// <value>The graph type count.</value>
        public int SchemaGraphTypeCount
        {
            get => this.GetProperty<int>(LogPropertyNames.SCHEMA_GRAPH_TYPE_COUNT);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_GRAPH_TYPE_COUNT, value);
        }

        /// <summary>
        /// Gets the named assigned to the generated schema instance. This name may or may not be
        /// unique depending on implementation.
        /// the query plan.
        /// </summary>
        /// <value>The name assigned to this specific schema instance.</value>
        public string SchemaInstanceName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_INSTANCE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_INSTANCE_NAME, value);
        }

        /// <summary>
        /// Gets the schema supported operation types. A comma-seperated list of the query types available
        /// (e.g. mutation, query, subscription etc).
        /// </summary>
        /// <value>The schema supported operation types.</value>
        public IList<string> SchemaSupportedOperationTypes
        {
            get => this.GetProperty<IList<string>>(LogPropertyNames.SCHEMA_SUPPORTED_OPERATION_TYPES);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_SUPPORTED_OPERATION_TYPES, value);
        }

        /// <summary>
        /// Gets the <see cref="Type" /> name of the schema instance.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the collection of graph types defined on the schema instance.
        /// </summary>
        /// <value>The graph types.</value>
        public IList<IGraphLogPropertyCollection> GraphTypes
        {
            get => this.GetProperty<IList<IGraphLogPropertyCollection>>(LogPropertyNames.SCHEMA_GRAPH_TYPE_COLLECTION);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_GRAPH_TYPE_COLLECTION, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Schema Created | Name: '{this.SchemaInstanceName}, Type: '{_schemaTypeShortName}', GraphTypes: {this.SchemaGraphTypeCount}";
        }
    }
}