﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Collections.Concurrent;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A singular collection of all the graph type templates currently in the loaded app domain. Templates are
    /// schema agnostic and expected to be reused across multiple schema instances.
    /// </summary>
    public class DefaultTypeTemplateProvider : IGraphTypeTemplateProvider
    {
        // maintain a collection of any already parsed
        // templates to speed up any dynamic construction operations that may occur at run time.
        private readonly ConcurrentDictionary<Tuple<TypeKind, Type>, IGraphTypeTemplate> _knownObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTypeTemplateProvider"/> class.
        /// </summary>
        public DefaultTypeTemplateProvider()
        {
            _knownObjects = new ConcurrentDictionary<Tuple<TypeKind, Type>, IGraphTypeTemplate>();
            this.CacheTemplates = true;
        }

        /// <inheritdoc />
        public IGraphItemTemplate ParseType<TObjectType>(TypeKind? kind = null)
        {
            return this.ParseType(typeof(TObjectType), kind);
        }

        /// <inheritdoc />
        public IGraphTypeTemplate ParseType(Type objectType, TypeKind? kind = null)
        {
            Validation.ThrowIfNull(objectType, nameof(objectType));

            var typeKind = GraphValidation.ResolveTypeKind(objectType, kind);
            var typeKey = Tuple.Create(typeKind, objectType);

            if (_knownObjects.TryGetValue(typeKey, out var template) && this.CacheTemplates)
                return template;

            if (GraphQLProviders.ScalarProvider.IsScalar(objectType))
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{objectType.FriendlyName()}' is a known scalar type. Scalars must be explicitly defined and cannot be templated.",
                    objectType);
            }

            if (Validation.IsCastable<IGraphUnionProxy>(objectType))
            {
                throw new GraphTypeDeclarationException(
                    $"The union proxy '{objectType.FriendlyName()}' cannot be directly parsed as a graph type. Double check " +
                    "your field attribute declarations.",
                    objectType);
            }

            GraphValidation.IsValidGraphType(objectType, true);

            template = this.MakeTemplate(objectType, typeKind);
            template.Parse();
            template.ValidateOrThrow();

            if (this.CacheTemplates)
                _knownObjects.TryAdd(typeKey, template);

            return template;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _knownObjects.Clear();
        }

        /// <summary>
        /// Makes a graph template from the given type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="kind">The kind of graph type to parse for.</param>
        /// <returns>IGraphItemTemplate.</returns>
        protected virtual IGraphTypeTemplate MakeTemplate(Type objectType, TypeKind kind)
        {
            return MakeTemplateInternal(objectType, kind);
        }

        /// <summary>
        /// Internal overload of the default factory method for creating template objects. Used in various aspects of testing.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="kind">The kind.</param>
        /// <returns>IGraphTypeTemplate.</returns>
        internal static IGraphTypeTemplate MakeTemplateInternal(Type objectType, TypeKind kind)
        {
            if (objectType.IsInterface)
                return new InterfaceGraphTypeTemplate(objectType);
            if (objectType.IsEnum)
                return new EnumGraphTypeTemplate(objectType);
            if (Validation.IsCastable<GraphController>(objectType))
                return new GraphControllerTemplate(objectType);
            if (Validation.IsCastable<GraphDirective>(objectType))
                return new GraphDirectiveTemplate(objectType);
            if (kind == TypeKind.INPUT_OBJECT)
                return new InputObjectGraphTypeTemplate(objectType);

            return new ObjectGraphTypeTemplate(objectType);
        }

        /// <inheritdoc />
        public int Count => _knownObjects.Count;

        /// <inheritdoc />
        public bool CacheTemplates { get; set; }
    }
}