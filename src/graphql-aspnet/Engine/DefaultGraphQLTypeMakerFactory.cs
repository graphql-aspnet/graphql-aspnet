// *************************************************************
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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object used during schema generation to organize and expose the various
    /// template objects to the schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this maker factory is registered to handle.</typeparam>
    public class DefaultGraphQLTypeMakerFactory<TSchema> : IGraphQLTypeMakerFactory<TSchema>
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public virtual IGraphTypeTemplate MakeTemplate(Type objectType, TypeKind? kind = null)
        {
            return GraphTypeTemplates.CreateTemplate(objectType, kind);
        }

        /// <inheritdoc />
        public virtual IGraphTypeMaker CreateTypeMaker(Type objectType = null, TypeKind? kind = null)
        {
            if (objectType == null)
            {
                if (!kind.HasValue)
                    return null;
            }
            else
            {
                objectType = GlobalTypes.FindBuiltInScalarType(objectType) ?? objectType;

                if (Validation.IsCastable<IScalarGraphType>(objectType))
                    kind = TypeKind.SCALAR;
                else if (objectType.IsEnum)
                    kind = TypeKind.ENUM;
                else if (objectType.IsInterface)
                    kind = TypeKind.INTERFACE;
                else if (Validation.IsCastable<GraphDirective>(objectType))
                    kind = TypeKind.DIRECTIVE;
                else if (Validation.IsCastable<GraphController>(objectType))
                    kind = TypeKind.CONTROLLER;
                else if (Validation.IsCastable<IGraphUnionProxy>(objectType))
                    kind = TypeKind.UNION;
                else if (!kind.HasValue || kind.Value != TypeKind.INPUT_OBJECT)
                    kind = TypeKind.OBJECT;
            }

            switch (kind.Value)
            {
                case TypeKind.SCALAR:
                    return new ScalarGraphTypeMaker(this.Schema.Configuration);

                case TypeKind.INTERFACE:
                    return new InterfaceGraphTypeMaker(this.Schema.Configuration, this.CreateFieldMaker());

                case TypeKind.UNION:
                    return new UnionGraphTypeMaker(this.Schema.Configuration);

                case TypeKind.ENUM:
                    return new EnumGraphTypeMaker(this.Schema.Configuration);

                case TypeKind.INPUT_OBJECT:
                    return new InputObjectGraphTypeMaker(
                        this.Schema.Configuration,
                        this.CreateFieldMaker());

                case TypeKind.OBJECT:
                    return new ObjectGraphTypeMaker(
                        this.Schema.Configuration,
                        this.CreateFieldMaker());

                case TypeKind.DIRECTIVE:
                    return new DirectiveMaker(this.Schema.Configuration, this.CreateArgumentMaker());

                case TypeKind.CONTROLLER:
                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public virtual IGraphFieldMaker CreateFieldMaker()
        {
            return new GraphFieldMaker(this.Schema, this.CreateArgumentMaker());
        }

        /// <inheritdoc />
        public virtual IGraphArgumentMaker CreateArgumentMaker()
        {
            return new GraphArgumentMaker(this.Schema);
        }

        /// <inheritdoc />
        public virtual IUnionGraphTypeMaker CreateUnionMaker()
        {
            return new UnionGraphTypeMaker(this.Schema.Configuration);
        }

        /// <inheritdoc />
        public void Initialize(ISchema schemaInstance)
        {
            Validation.ThrowIfNull(schemaInstance, nameof(schemaInstance));

            if (this.Schema != null)
                throw new InvalidOperationException("This instance has already been initialized with a schema");

            this.Schema = schemaInstance;
        }

        /// <summary>
        /// Gets the schema this factory works against.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; private set; }
    }
}