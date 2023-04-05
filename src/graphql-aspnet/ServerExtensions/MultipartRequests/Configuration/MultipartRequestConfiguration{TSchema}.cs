// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration
{
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;

    /// <summary>
    /// A configuration class to set runtime options related to <see cref="MultipartRequestServerExtension" /> for
    /// a specific schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this configuration targets.</typeparam>
    public class MultipartRequestConfiguration<TSchema> : MultipartRequestConfiguration, IMultipartRequestConfiguration<TSchema>
        where TSchema : class, ISchema
    {
    }
}