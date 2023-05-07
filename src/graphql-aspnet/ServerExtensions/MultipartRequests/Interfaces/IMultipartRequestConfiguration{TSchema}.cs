// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An interface detailing the options configured for an instance of <see cref="MultipartRequestServerExtension" />
    /// for a single schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this configuration object targets.</typeparam>
    public interface IMultipartRequestConfiguration<TSchema> : IMultipartRequestConfiguration
        where TSchema : class, ISchema
    {
    }
}