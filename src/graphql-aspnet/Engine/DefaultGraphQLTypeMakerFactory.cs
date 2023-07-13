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
        public virtual IGraphTypeMaker CreateTypeMaker(Type objectType, TypeKind? kind = null)
        {
            if (objectType == null)
                return null;

            objectType = GlobalTypes.FindBuiltInScalarType(objectType) ?? objectType;

            if (Validation.IsCastable<IScalarGraphType>(objectType))
                return new ScalarGraphTypeMaker(this.Schema.Configuration);

            if (objectType.IsEnum)
                return new EnumGraphTypeMaker(this.Schema.Configuration);

            if (objectType.IsInterface)
                return new InterfaceGraphTypeMaker(this.Schema.Configuration, this.CreateFieldMaker());

            if (Validation.IsCastable<GraphDirective>(objectType))
                return new DirectiveMaker(this.Schema.Configuration, this.CreateArgumentMaker());

            if (Validation.IsCastable<GraphController>(objectType))
                return null;

            if (Validation.IsCastable<IGraphUnionProxy>(objectType))
                return new UnionGraphTypeMaker(this.Schema.Configuration);

            if (kind.HasValue && kind.Value == TypeKind.INPUT_OBJECT)
            {
                return new InputObjectGraphTypeMaker(
                    this.Schema.Configuration,
                    this.CreateFieldMaker());
            }

            // when all else fails just use an object maker
            return new ObjectGraphTypeMaker(
                this.Schema.Configuration,
                this.CreateFieldMaker());
        }

        /// <inheritdoc />
        public virtual IGraphFieldMaker CreateFieldMaker()
        {
            return new GraphFieldMaker(this.Schema, this);
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