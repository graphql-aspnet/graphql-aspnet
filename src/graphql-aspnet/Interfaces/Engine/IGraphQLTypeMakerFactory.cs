// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A factory used during schema generation to create templates and makers for creating
    /// <see cref="IGraphType" /> instances used to populate a schema.
    /// </summary>
    public interface IGraphQLTypeMakerFactory
    {
        /// <summary>
        /// Initializes the maker for the given schema instance providing necessary contextual information
        /// for any makers created by this factory. Not all implementers may use the provided schema.
        /// </summary>
        /// <param name="schemaInstance">The schema instance to initialize for.</param>
        void Initialize(ISchema schemaInstance);

        /// <summary>
        /// Creates a new template from the given type. This template is consumed during type generation.
        /// </summary>
        /// <param name="objectType">Type of the object to templatize.</param>
        /// <param name="kind">The typekind of template to be created. If the kind can be
        /// determined from the <paramref name="objectType"/> alone, this value is ignored.  Largely used to seperate
        /// an OBJECT template from an INPUT_OBJECT template for the same .NET type. </param>
        /// <returns>IGraphTypeTemplate.</returns>
        IGraphTypeTemplate MakeTemplate(Type objectType, TypeKind? kind = null);

        /// <summary>
        /// Parses the provided type, extracting the metadata used in type generation for the object graph.
        /// </summary>
        /// <param name="objectType">The type of the object to inspect and determine a valid type maker.</param>
        /// <param name="kind">The graph <see cref="TypeKind" /> to create a template for. If not supplied the template provider
        /// will attempt to assign the best graph type possible for the supplied <paramref name="objectType"/>.</param>
        /// <returns>IGraphTypeTemplate.</returns>
        IGraphTypeMaker CreateTypeMaker(Type objectType = null, TypeKind? kind = null);

        /// <summary>
        /// Creates a maker that can generate valid graph fields
        /// </summary>
        /// <returns>IGraphFieldMaker.</returns>
        IGraphFieldMaker CreateFieldMaker();

        /// <summary>
        /// Creates a maker that can generate arguments for fields or directives.
        /// </summary>
        /// <returns>IGraphArgumentMaker.</returns>
        IGraphArgumentMaker CreateArgumentMaker();

        /// <summary>
        /// Creates a maker that can generate special union graph types.
        /// </summary>
        /// <returns>IUnionGraphTypeMaker.</returns>
        IUnionGraphTypeMaker CreateUnionMaker();

        /// <summary>
        /// Gets the schema this factory works against.
        /// </summary>
        /// <value>The schema.</value>
        ISchema Schema { get; }
    }
}