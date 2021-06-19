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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Performs an analysis of a runtime type to determine known and allowed concrete types for a
    /// <see cref="ISchemaTypeCollection"/>. Typically invoked when a developer attempts to return an object of a <see cref="Type"/>
    /// extended from the <see cref="Type"/> they declared on a controller or field.
    /// </summary>
    internal class SchemaRuntimeTypeAnalyzer
    {
        private readonly ISchemaTypeCollection _schema;
        private readonly ConcurrentDictionary<(IGraphType, Type), Type[]> _foundTypeCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRuntimeTypeAnalyzer"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public SchemaRuntimeTypeAnalyzer(ISchemaTypeCollection schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _foundTypeCache = new ConcurrentDictionary<(IGraphType, Type), Type[]>();
        }

        /// <summary>
        /// Inspects all allowed types for the given <paramref name="graphType"/> and determines which of them the
        /// <paramref name="typeToCheck"/> could be used for.
        /// </summary>
        /// <param name="graphType">The graph type being inspected.</param>
        /// <param name="typeToCheck">The concrete type to check against.</param>
        /// <returns>Type[].</returns>
        public Type[] FindAllowedTypes(IGraphType graphType, Type typeToCheck)
        {
            Validation.ThrowIfNull(graphType, nameof(graphType));
            Validation.ThrowIfNull(typeToCheck, nameof(typeToCheck));

            if (!_schema.Contains(graphType))
                return new Type[0];

            switch (graphType.Kind)
            {
                // never cache low level type kinds, they always, at most, map to one concrete type
                // already known to and managed by the schema
                case TypeKind.SCALAR:
                case TypeKind.ENUM:
                case TypeKind.INPUT_OBJECT:
                    var singleType = _schema.FindConcreteType(graphType);
                    return this.CreateAndCacheTypeMap(graphType, typeToCheck, singleType);

                case TypeKind.OBJECT:
                    // for a given object graph type, if the typeToCheck IS the known type to the schema
                    // just return it
                    var foundType = _schema.FindConcreteType(graphType);
                    if (foundType == typeToCheck)
                        return new Type[] { foundType };

                    // when its not a direct match (such as then typeToCheck inherits from foundType)
                    // then proceed to analyze and validate a mapping.
                    break;

                case TypeKind.INTERFACE:
                case TypeKind.UNION:
                    // we want to handle maps to interfaces, unions for sure.
                    break;

                case TypeKind.LIST:
                case TypeKind.NON_NULL:
                case TypeKind.DIRECTIVE:
                default:
                    // abstract type kinds can't map to concrete types in the schema
                    return new Type[0];
            }

            // check the local cache for an already mapped pair
            var key = this.CreateKey(graphType, typeToCheck);
            if (_foundTypeCache.TryGetValue(key, out var result))
                return result;

            var types = this.FindAcceptableTypes(graphType, typeToCheck);
            return this.CreateAndCacheTypeMap(graphType, typeToCheck, types);
        }

        private Type[] FindAcceptableTypes(IGraphType graphType, Type typeToCheck)
        {
            var allowedTypes = _schema.FindConcreteTypes(graphType);
            var list = new List<Type>();
            foreach (var allowedType in allowedTypes)
            {
                if (allowedType == typeToCheck)
                {
                    // a union may match explicitly to one of the allowed types
                    // if so infer it to be an exact match (always)
                    list.Clear();
                    list.Add(allowedType);
                    break;
                }

                if (Validation.IsCastable(typeToCheck, allowedType))
                    list.Add(allowedType);
            }

            // if the graph type is a union and an exact match wasn't found
            // allow intervention by the user's defined proxy
            // to try a hail mary to determine the allowed type.
            if (list.Count != 1 && graphType is IUnionGraphType ugt)
            {
                var mappedType = ugt.Proxy.ResolveType(typeToCheck);
                if (mappedType != null && mappedType != typeToCheck)
                {
                    list.Clear();

                    if (ugt.Proxy.Types.Contains(mappedType))
                        list.Add(mappedType);
                }
            }

            return list.ToArray();
        }

        private Type[] CreateAndCacheTypeMap(IGraphType graphType, Type typeToCheck, params Type[] foundTypes)
        {
            var key = this.CreateKey(graphType, typeToCheck);
            foundTypes = foundTypes ?? new Type[0];
            if (_foundTypeCache.TryAdd(key, foundTypes))
                return foundTypes;

            return _foundTypeCache[key];
        }

        private (IGraphType, Type) CreateKey(IGraphType graphType, Type typeToCheck)
        {
            return (graphType, typeToCheck);
        }
    }
}