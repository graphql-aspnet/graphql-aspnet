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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A factory used during schema generation to create templates and makers for creating
    /// <see cref="IGraphType" /> instances used to populate a schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this maker factory is registered to .</typeparam>
    public interface IGraphQLTypeMakerFactory<TSchema> : IGraphQLTypeMakerFactory
        where TSchema : class, ISchema
    {
    }
}