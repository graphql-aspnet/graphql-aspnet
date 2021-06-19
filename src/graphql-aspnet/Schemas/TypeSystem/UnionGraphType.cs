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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A graph type representing a graph ql union.
    /// spec: https://graphql.github.io/graphql-spec/June2018/#sec-Unions .
    /// </summary>
    [DebuggerDisplay("UNION {Name} (Types = {_types.Count})")]
    public class UnionGraphType : IUnionGraphType
    {
        // with methods for inspection
        private readonly HashSet<Type> _types;
        private readonly List<string> _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionGraphType" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unionProxy">The proxy object which defined
        /// the union type at design t.</param>
        public UnionGraphType(string name, IGraphUnionProxy unionProxy)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Proxy = Validation.ThrowIfNullOrReturn(unionProxy, nameof(unionProxy));
            _types = new HashSet<Type>();
            _names = new List<string>();
            this.Publish = true;
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public bool ValidateObject(object item)
        {
            if (item == null || _types.Contains(item.GetType()))
                return true;

            var type = item.GetType();
            return _types.Any(x => Validation.IsCastable(type, x));
        }

        /// <summary>
        /// Adds a possible graph type to the union instance.
        /// </summary>
        /// <param name="name">The name, as it should appear in the target schema.</param>
        /// <param name="concreteType">The concrete type representing the graph type.</param>
        public void AddPossibleGraphType(string name, Type concreteType)
        {
            Validation.ThrowIfNullWhiteSpace(name, nameof(name));
            Validation.ThrowIfNull(concreteType, nameof(concreteType));

            _types.Add(concreteType);
            _names.Add(name);
        }

        /// <summary>
        /// Gets the possible graph types this union could be.
        /// </summary>
        /// <value>The possible graph type names.</value>
        public IEnumerable<Type> PossibleConcreteTypes => _types;

        /// <summary>
        /// Gets the possible graph type names this union could be.
        /// </summary>
        /// <value>The possible graph type names.</value>
        public IEnumerable<string> PossibleGraphTypeNames => _names;

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the proxy object that was defined at design type which created this union type.
        /// </summary>
        /// <value>The proxy.</value>
        public IGraphUnionProxy Proxy { get; }

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
        public TypeKind Kind => TypeKind.UNION;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphType" /> is published on an introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this graph types points to a concrete type
        /// defined by a developer.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public virtual bool IsVirtual => false;
    }
}