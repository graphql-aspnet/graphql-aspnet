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
        public UnionGraphType(string name)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
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
            return item == null || _types.Contains(item.GetType());
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
    }
}