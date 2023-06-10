// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An interface used by the injector to expose its useschema method across
    /// multiple generic types.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema.</typeparam>
    public interface ISchemaInjector<TSchema> : ISchemaInjector
           where TSchema : class, ISchema
    {
        /// <summary>
        /// Gets the pipeline builder for the schema being tracked by this instance.
        /// </summary>
        /// <value>The pipeline builder.</value>
        ISchemaBuilder<TSchema> SchemaBuilder { get; }
    }
}