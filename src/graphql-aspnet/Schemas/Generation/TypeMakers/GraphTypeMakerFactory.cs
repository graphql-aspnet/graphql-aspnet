// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object used during schema generation to organize and expose the various
    /// template objects to the schema.
    /// </summary>
    public class GraphTypeMakerFactory
    {
        private ISchema _schemaInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeMakerFactory"/> class.
        /// </summary>
        /// <param name="schemaInstance">The schema instance to reference when making
        /// types.</param>
        public GraphTypeMakerFactory(ISchema schemaInstance)
        {
            Validation.ThrowIfNull(schemaInstance, nameof(schemaInstance));

            _schemaInstance = schemaInstance;
        }

        /// <summary>
        /// Creates a new template from the given type. This template is consumed during type generation.
        /// </summary>
        /// <param name="objectType">Type of the object to templatize.</param>
        /// <param name="kind">The typekind of template to be created. If the kind can be
        /// determined from the <paramref name="objectType"/> alone, this value is ignored.  Largely used to seperate
        /// an OBJECT template from an INPUT_OBJECT template for the same .NET type. </param>
        /// <returns>IGraphTypeTemplate.</returns>
        public virtual IGraphTypeTemplate MakeTemplate(Type objectType, TypeKind? kind = null)
        {
            return GraphTypeTemplates.CreateTemplate(objectType, kind);
        }

        /// <summary>
        /// Parses the provided type, extracting the metadata used in type generation for the object graph.
        /// </summary>
        /// <param name="objectType">The type of the object to inspect and determine a valid type maker.</param>
        /// <param name="kind">The graph <see cref="TypeKind" /> to create a template for. If not supplied the template provider
        /// will attempt to assign the best graph type possible for the supplied <paramref name="objectType"/>.</param>
        /// <returns>IGraphTypeTemplate.</returns>
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
                    return new ScalarGraphTypeMaker(_schemaInstance.Configuration);

                case TypeKind.INTERFACE:
                    return new InterfaceGraphTypeMaker(_schemaInstance.Configuration, this.CreateFieldMaker());

                case TypeKind.UNION:
                    return new UnionGraphTypeMaker(_schemaInstance.Configuration);

                case TypeKind.ENUM:
                    return new EnumGraphTypeMaker(_schemaInstance.Configuration);

                case TypeKind.INPUT_OBJECT:
                    return new InputObjectGraphTypeMaker(
                        _schemaInstance.Configuration,
                        this.CreateFieldMaker());

                case TypeKind.OBJECT:
                    return new ObjectGraphTypeMaker(
                        _schemaInstance.Configuration,
                        this.CreateFieldMaker());

                case TypeKind.DIRECTIVE:
                    return new DirectiveMaker(_schemaInstance, this.CreateArgumentMaker());

                case TypeKind.CONTROLLER:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a maker that can generate valid graph fields
        /// </summary>
        /// <returns>IGraphFieldMaker.</returns>
        public virtual IGraphFieldMaker CreateFieldMaker()
        {
            return new GraphFieldMaker(_schemaInstance, this.CreateArgumentMaker());
        }

        /// <summary>
        /// Creates a maker that can generate arguments for fields or directives.
        /// </summary>
        /// <returns>IGraphArgumentMaker.</returns>
        public virtual IGraphArgumentMaker CreateArgumentMaker()
        {
            return new GraphArgumentMaker(_schemaInstance);
        }

        /// <summary>
        /// Creates a maker that can generate special union graph types.
        /// </summary>
        /// <returns>IUnionGraphTypeMaker.</returns>
        public virtual IUnionGraphTypeMaker CreateUnionMaker()
        {
            return new UnionGraphTypeMaker(_schemaInstance.Configuration);
        }
    }
}