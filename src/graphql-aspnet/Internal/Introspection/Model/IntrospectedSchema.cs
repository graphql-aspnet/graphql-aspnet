// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A model object containing data for the __schema meta field.
    /// </summary>
    [DebuggerDisplay("Introspected Schema: {Name}")]
    public class IntrospectedSchema
    {
        private readonly ISchema _schema;
        private OrderedDictionary<string, IntrospectedType> _typeList;
        private List<IntrospectedDirective> _directiveList;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedSchema"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public IntrospectedSchema(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _typeList = new OrderedDictionary<string, IntrospectedType>();
        }

        /// <summary>
        /// Rebuilds all introspected types and directives for this instance.
        /// </summary>
        public void Rebuild()
        {
            this.RebuildIntrospectedTypeList();
            this.RebuildDirectiveList();
        }

        /// <summary>
        /// Resolves the directives as a special collection of items on the introspected schema.
        /// </summary>
        private void RebuildDirectiveList()
        {
            _directiveList?.Clear();
            _directiveList = new List<IntrospectedDirective>();
            foreach (var directiveType in _schema.KnownTypes.OfType<IDirectiveGraphType>().Where(x => x.Publish))
            {
                var directive = new IntrospectedDirective(directiveType);
                directive.Initialize(this);
                _directiveList.Add(directive);
            }
        }

        /// <summary>
        /// Rebuilds the introspected type list.
        /// </summary>
        private void RebuildIntrospectedTypeList()
        {
            _typeList.Clear();
            _typeList = new OrderedDictionary<string, IntrospectedType>();

            // build in a two pass approach
            // -----------------------------------------
            // first create and store a reference for all known graph types
            // because of the hierarchtical nature introspected types
            // may need to store a reference to a type (becaues a field returns it) but it has to exist
            // in the collection first
            foreach (var graphType in _schema.KnownTypes.OfTypeButNotType<IGraphType, IDirectiveGraphType>())
                this.CreateAndStoreIntrospectedType(graphType);

            // initialize each type creatd
            foreach (var introspectedType in _typeList.Values)
                introspectedType.Initialize(this);
        }

        /// <summary>
        /// Attempts to find a single introspected object representing the given graph type. If not found
        /// it will attempt to create it. This method does not support directives. Use the <see cref="Directives"/>
        /// properties to find directives.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        private void CreateAndStoreIntrospectedType(IGraphType graphType)
        {
            if (_typeList.TryGetValue(graphType.Name, out _))
                return;

            Func<IGraphType, IntrospectedType> creator;
            switch (graphType)
            {
                case IObjectGraphType _:
                case IInterfaceGraphType _:
                case IInputObjectGraphType _:
                case IScalarGraphType _:
                case IEnumGraphType _:
                case IUnionGraphType _:
                    creator = (gt) => new IntrospectedType(gt);
                    break;
                default:
                    throw new GraphExecutionException($"Invalid graph type. '{graphType.Kind}' is not supported by introspection.");
            }

            IntrospectedType foundValue = creator(graphType);
            _typeList.Add(graphType.Name, foundValue);
        }

        /// <summary>
        /// Attempts to find the <see cref="IGraphType"/> by its name. Returns null if no value
        /// is found.
        /// </summary>
        /// <param name="graphTypeName">Name of the graph type.</param>
        /// <returns>IGraphType.</returns>
        public IGraphType FindGraphType(string graphTypeName)
        {
            return _schema.KnownTypes.FindGraphType(graphTypeName);
        }

        /// <summary>
        /// Searchs the schema for an introspected graph type.
        /// </summary>
        /// <param name="graphType">The graph type to search for.</param>
        /// <returns>IIntrospectedType.</returns>
        public IntrospectedType FindIntrospectedType(IGraphType graphType)
        {
            return this.FindIntrospectedType(graphType?.Name);
        }

        /// <summary>
        /// Searchs the schema for an introspected graph type of the given name.
        /// </summary>
        /// <param name="graphTypeName">The name of the graph type in this schema.</param>
        /// <returns>IIntrospectedType.</returns>
        public IntrospectedType FindIntrospectedType(string graphTypeName)
        {
            _typeList.TryGetValue(graphTypeName, out var foundType);
            return foundType;
        }

        /// <summary>
        /// Finds the introspected graph types that implement the named interface.
        /// </summary>
        /// <param name="interfaceName">Name of the interface to search by.</param>
        /// <returns>IEnumerable&lt;IIntrospectedType&gt;.</returns>
        public IEnumerable<IntrospectedType> FindIntrospectedTypesByInterface(string interfaceName)
        {
            foreach (var graphType in _schema.KnownTypes.FindGraphTypesByInterface(interfaceName))
            {
                yield return this.FindIntrospectedType(graphType);
            }
        }

        /// <summary>
        /// Attempts to retrieve the introspected model item representing one of the primary
        /// operation types supported by the schema.  Reuses any previously created objects
        /// and will attempt to resolve a missing object on each request in an attempt to
        /// account for a run-time-updated schema instance.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>IntrospectedType.</returns>
        private IntrospectedType FindOperationType(GraphCollection collection)
        {
            return this.FindIntrospectedType(Constants.ReservedNames.FindOperationTypeNameByType(collection));
        }

        /// <summary>
        /// Gets the known types.
        /// </summary>
        /// <value>The known types.</value>
        public IEnumerable<IntrospectedType> KnownTypes => _typeList.Values;

        /// <summary>
        /// Gets the type data of the query operation type.
        /// </summary>
        /// <value>The type of the query.</value>
        public IntrospectedType QueryType => this.FindOperationType(GraphCollection.Query);

        /// <summary>
        /// Gets the type data of the mutation operation type.
        /// </summary>
        /// <value>The type of the mutation.</value>
        public IntrospectedType MutationType => this.FindOperationType(GraphCollection.Mutation);

        /// <summary>
        /// Gets the type data of the subscription type.
        /// </summary>
        /// <value>The type of the subscription.</value>
        public IntrospectedType SubscriptionType => this.FindOperationType(GraphCollection.Subscription);

        /// <summary>
        /// Gets the directives known to this schema.
        /// </summary>
        /// <value>The directives.</value>
        public IEnumerable<IntrospectedDirective> Directives => _directiveList;

        /// <summary>
        /// Gets the name of the schema this introspection object is hosting.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _schema?.Name;

        /// <summary>
        /// Gets the core schema represented by this introspected entity.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema => _schema;
    }
}