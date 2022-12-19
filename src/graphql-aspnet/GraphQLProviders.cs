// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A global set of providers used throughout GraphQL.AspNet. These objects are static, unchanged and expected to
    /// not change at runtime. Do not alter the contents of the static properties after calling <c>.AddGraphQL()</c>.
    /// </summary>
    public static class GraphQLProviders
    {
        /// <summary>
        /// Gets or sets the globally available template provider used by this graphql server. This object manages the schema agnostic collection
        /// of meta data for all .NET types being used as a graph type in a schema; be that controllers, interfaces, unions model/data POCOs.
        /// </summary>
        /// <value>The global template provider.</value>
        public static IGraphTypeTemplateProvider TemplateProvider { get; set; } = new DefaultTypeTemplateProvider();

        /// <summary>
        /// Gets or sets the globally available provider for managing scalars. This object manages all known scalars
        /// across all schemas registered to this application domain.
        /// </summary>
        /// <value>The global scalar provider.</value>
        public static IScalarTypeProvider ScalarProvider { get; set; } = new DefaultScalarTypeProvider();

        /// <summary>
        /// Gets or sets an abstract factory that generates "type makers" that can create a new instance of
        /// any <see cref="IGraphType"/> from a template for use in a schema.
        /// </summary>
        /// <value>The graph type maker provider.</value>
        public static IGraphTypeMakerProvider GraphTypeMakerProvider { get; set; } = new DefaultGraphTypeMakerProvider();
    }
}