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
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A global set of objects used throughout graphql. These objects are static, unchanged and expected to be
    /// intact and not reset at runtime. Do not alter the contents of the static properties beyond application startup.
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
        /// Gets or sets the globally available scalar manager used by this graphql server. This object manages all known scalars
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

        /// <summary>
        /// Gets or sets the global configuration options that effect all <see cref="ISchema"/> instances
        /// registered to this server instance.
        /// </summary>
        /// <value>The global configuration settings.</value>
        public static IGraphQLGlobalConfiguration GlobalConfiguration { get; set; } = new DefaultGraphQLGLobalConfiguration();
    }
}