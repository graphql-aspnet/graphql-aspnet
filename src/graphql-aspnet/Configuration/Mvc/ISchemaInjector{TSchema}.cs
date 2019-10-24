// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Mvc
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface used by the injector to expose its useschema method across
    /// multiple generic types.
    /// </summary>
        /// <typeparam name="TSchema">The type of the schema.</typeparam>
    internal interface ISchemaInjector<TSchema> : ISchemaInjector
           where TSchema : class, ISchema
    {
    }
}