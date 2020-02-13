// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A represention of a graphql directive in the schema. This object defines all the exposed
    /// meta data and invocation information when this directive is queried.
    /// </summary>
    [DebuggerDisplay("DIRECTIVE {Name}")]
    public class DirectiveGraphType : IDirectiveGraphType
    {
        private readonly Type _directiveType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the directive as it appears in the schema.</param>
        /// <param name="locations">The locations where this directive is valid.</param>
        /// <param name="directiveType">The concrete type of the directive.</param>
        /// <param name="resolver">The resolver used to process this instance.</param>
        public DirectiveGraphType(
            string name,
            DirectiveLocation locations,
            Type directiveType,
            IGraphDirectiveResolver resolver = null)
        {
            this.Name = Validation.ThrowIfNullOrReturn(name, nameof(name));
            this.Arguments = new GraphFieldArgumentCollection();
            this.Locations = locations;
            this.Resolver = resolver;
            this.Publish = true;
            _directiveType = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public bool ValidateObject(object item)
        {
            return item == null || item.GetType() == _directiveType;
        }

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the value indicating what type of graph type this instance is in the type system. (object, scalar etc.)
        /// </summary>
        /// <value>The kind.</value>
        public TypeKind Kind => TypeKind.DIRECTIVE;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphType"/> is published on an introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish { get; set; }

        /// <summary>
        /// Gets a collection of arguments this instance can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        public IGraphFieldArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets or sets the resolver asssigned to this directive type to process any invocations.
        /// </summary>
        /// <value>The resolver.</value>
        public IGraphDirectiveResolver Resolver { get; set; }

        /// <summary>
        /// Gets the locations this directive can be defined.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this graph types points to a concrete type
        /// defined by a developer.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public virtual bool IsVirtual => false;
    }
}