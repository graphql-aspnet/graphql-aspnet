// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.TypeCollections
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;

    /// <summary>
    /// A colleciton of <see cref="IGraphType"/> and their associated concrete .NET <see cref="Type"/>.
    /// </summary>
    internal class ConcreteTypeCollection
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<TypeKind, IGraphType>> _graphTypesByConcreteType;
        private readonly ConcurrentDictionary<IGraphType, Type> _concreteTypesByGraphType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcreteTypeCollection" /> class.
        /// </summary>
        public ConcreteTypeCollection()
        {
            _graphTypesByConcreteType = new ConcurrentDictionary<Type, ConcurrentDictionary<TypeKind, IGraphType>>();
            _concreteTypesByGraphType = new ConcurrentDictionary<IGraphType, Type>(GraphTypeEqualityComparer.Instance);
        }

        /// <summary>
        /// Attempts to find the graph type of a given kind for the supplied concrete type. Returns null if no type is found. This
        /// method will perform a <see cref="TypeKind"/> coersion if possible.
        /// </summary>
        /// <param name="type">The concrete type to search with.</param>
        /// <param name="kind">An optional kind of graph type to search for. Only used in a tie breaker scenario
        /// such as if a concrete type is registered as both an OBJECT and INPUT_OBJECT.</param>
        /// <returns>IGraphType.</returns>
        public IGraphType FindGraphType(Type type, TypeKind? kind = null)
        {
            Validation.ThrowIfNull(type, nameof(type));

            type = GraphValidation.EliminateNextWrapperFromCoreType(type);

            if (_graphTypesByConcreteType.TryGetValue(type, out var typeSet))
            {
                if (typeSet.Count == 1)
                {
                    var value = typeSet.First().Value;
                    if (!kind.HasValue || value.Kind.IsLeafKind())
                        return value;
                }

                var resolvedKind = kind ?? TypeKind.OBJECT;
                if (typeSet.TryGetValue(resolvedKind, out var graphType))
                    return graphType;
            }

            return null;
        }

        /// <summary>
        /// Finds the concrete C# type associated with the given <see cref="IGraphType"/>. If the provided graph type
        /// is not part of this schema, null is returned.
        /// </summary>
        /// <param name="graphType">The graph type to look for..</param>
        /// <returns>Type.</returns>
        public Type FindType(IGraphType graphType)
        {
            Validation.ThrowIfNull(graphType, nameof(graphType));
            if (_concreteTypesByGraphType.TryGetValue(graphType, out var foundType))
            {
                return foundType;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Ensures an association between the concrete C# <see cref="Type" /> and the graph type of the schema. If the concrete type
        /// is already reserved for a different graph type or if it cannot be associated (for instance a scalar type to an object graph type) an
        /// exception is thrown.
        /// </summary>
        /// <param name="graphType">The graph type to be associated.</param>
        /// <param name="concreteType">The concrete type to associate with. If null the system will
        /// attempt to find the correct concrete type (such as with scalars) or throw an exception if one cannot be found.</param>
        /// <returns>The graph type that was created or referenced in the relationship. May not be the instance
        /// passed when adding a <see cref="IScalarGraphType"/>.</returns>
        public IGraphType EnsureRelationship(IGraphType graphType, Type concreteType)
        {
            // the registered concrete etype of a scalar must always be the primary declaration
            if (graphType is IScalarGraphType scalarType)
                concreteType = concreteType ?? scalarType.ObjectType;

            this.EnsureGraphTypeToConcreteTypeAssociationOrThrow(graphType, concreteType);
            if (concreteType == null)
                return graphType;

            if (!_graphTypesByConcreteType.TryGetValue(concreteType, out var kindSet))
            {
                kindSet = new ConcurrentDictionary<TypeKind, IGraphType>();
                _graphTypesByConcreteType.TryAdd(concreteType, kindSet);
            }

            if (!kindSet.TryGetValue(graphType.Kind, out var foundGraphType))
            {
                kindSet.TryAdd(graphType.Kind, graphType);
            }
            else if (foundGraphType.Name == graphType.Name)
            {
                graphType = foundGraphType;
            }
            else
            {
                throw new GraphTypeDeclarationException(
                    $"The concrete type '{concreteType.FriendlyName()}' is already associated with the graph type '{foundGraphType.Name}' it " +
                    $"cannot be reassigned to graph type '{graphType.Name}'");
            }

            if (!_concreteTypesByGraphType.TryGetValue(graphType, out var currentType))
            {
                _concreteTypesByGraphType.TryAdd(graphType, concreteType);
            }
            else if (concreteType != currentType)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph type '{graphType.Name}' is already associated with type '{currentType.FriendlyName(true)}' it " +
                    $"cannot be reassigned to '{concreteType.FriendlyName(true)}'");
            }

            return graphType;
        }

        /// <summary>
        /// Determines if the given concrete type is associated to a graph type of the supplied kind. This
        /// method will perform a <see cref="TypeKind"/> coersion if possible.
        /// </summary>
        /// <param name="type">The concrete type to search for.</param>
        /// <param name="kind">The graph type kind to look for.</param>
        /// <returns><c>true</c> if a graph type exists, <c>false</c> otherwise.</returns>
        public bool Contains(Type type, TypeKind? kind = null)
        {
            Validation.ThrowIfNull(type, nameof(type));

            if (_graphTypesByConcreteType.TryGetValue(type, out var typeSet))
            {
                if (!kind.HasValue)
                    return true;

                return typeSet.ContainsKey(kind.Value);
            }

            return false;
        }

        /// <summary>
        /// Ensures that the graph type CAN be assigned to the given concrete type. A context sensitive exception is thrown if a mismatch occurs. This
        /// method does not perform any additions or associations it just provides a logic container for association rules.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        /// <param name="associatedType">Type of the associated.</param>
        private void EnsureGraphTypeToConcreteTypeAssociationOrThrow(IGraphType graphType, Type associatedType = null)
        {
            // scalars must be assigned to their pre-defined and accepted concrete type
            // instances of scalar graph types must be their pre-defined instance as well
            if (graphType is IScalarGraphType scalarType)
            {
                if (associatedType == null ||
                    (associatedType != scalarType.ObjectType &&
                    !scalarType.OtherKnownTypes.Contains(associatedType)))
                {
                    throw new GraphTypeDeclarationException(
                        $"The scalar type '{graphType.Name}' cannot be associated to the concrete type '{associatedType?.FriendlyName() ?? "-null-"}' " +
                        $"on the target schema. Only explicitly declared concrete types on the scalar declaration are " +
                        $"allowed to represent the to be used for the schema.");
                }
            }

            // enums must be assoicated to no concrete type or to a type that is itself an enum.
            if (graphType.Kind == TypeKind.ENUM)
            {
                if (associatedType == null || !associatedType.IsEnum)
                {
                    throw new GraphTypeDeclarationException(
                        $"The enum type '{graphType.Name}' cannot be added and associated to the concrete type '{associatedType?.FriendlyName() ?? "-null-"}' it is not a valid enum.");
                }
            }
            else if (associatedType != null && associatedType.IsEnum)
            {
                throw new GraphTypeDeclarationException(
                    $"The concrete type '{associatedType.FriendlyName()}' is an enum. It cannot be associated to a graph type of '{graphType.Kind.ToString()}'.");
            }

            // directives must be assigned to a concrete type and it must inherit from GraphDirective.
            if (graphType.Kind == TypeKind.DIRECTIVE && (associatedType == null || !Validation.IsCastable<GraphDirective>(associatedType)))
            {
                throw new GraphTypeDeclarationException(
                    $"The directive type '{graphType.Name}' cannnot be associated to the concrete type '{associatedType.FriendlyName()}'. Directive graph types " +
                    $"can only be associated with concrete types that inherit from '{nameof(GraphDirective)}'.");
            }
        }

        /// <summary>
        /// Gets a collection of types that are referenced in this schema.
        /// </summary>
        /// <value>The types.</value>
        public IEnumerable<Type> Types => _graphTypesByConcreteType.Keys;
    }
}