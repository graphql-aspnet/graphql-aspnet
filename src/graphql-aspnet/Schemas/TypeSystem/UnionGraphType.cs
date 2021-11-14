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
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A graph type representing a UNION.
    /// spec: https://graphql.github.io/graphql-spec/June2018/#sec-Unions .
    /// </summary>
    [DebuggerDisplay("UNION {Name} (Types = {_types.Count})")]
    public class UnionGraphType : IUnionGraphType
    {
        // with methods for inspection
        private IImmutableSet<Type> _types;
        private IImmutableSet<string> _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the union as it appears in the target schema (case sensitive).</param>
        /// <param name="typeResolver">The type resolver used to match field resolve values with
        /// expected graph types in this union.</param>
        public UnionGraphType(string name, IUnionValueTypeResolver typeResolver)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));

            this.TypeResolver = Validation.ThrowIfNullOrReturn(typeResolver, nameof(TypeResolver));
            _types = ImmutableHashSet.Create<Type>();
            _names = ImmutableHashSet.Create<string>();
            this.Publish = true;
        }

        /// <inheritdoc />
        public virtual bool ValidateObject(object item)
        {
            if (item == null || _types.Contains(item.GetType()))
                return true;

            var type = item.GetType();
            return _types.Any(x => Validation.IsCastable(type, x));
        }

        /// <summary>
        /// Adds a possible graph type to the union instance.
        /// </summary>
        /// <param name="graphTypeName">The name of the graph type as it exists in the target schema (case sensitive).</param>
        /// <param name="concreteType">The concrete type representing the graph type.</param>
        public virtual void AddPossibleGraphType(string graphTypeName, Type concreteType)
        {
            Validation.ThrowIfNullWhiteSpace(graphTypeName, nameof(graphTypeName));
            Validation.ThrowIfNull(concreteType, nameof(concreteType));

            _types = _types.Add(concreteType);
            _names = _names.Add(graphTypeName);
        }

        /// <inheritdoc />
        public virtual IImmutableSet<Type> PossibleConcreteTypes => _types;

        /// <inheritdoc />
        public virtual IImmutableSet<string> PossibleGraphTypeNames => _names;

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <inheritdoc />
        public virtual string Description { get; set; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.UNION;

        /// <inheritdoc />
        public bool Publish { get; set; }

        /// <inheritdoc />
        public bool IsVirtual => false;

        /// <inheritdoc />
        public IUnionValueTypeResolver TypeResolver { get; set; }
    }
}