// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;

    /// <summary>
    /// Helper methods for configuring the multipart request extension.
    /// </summary>
    public static class MultipartRequestSchemaOptionsExtension
    {
        /// <summary>
        /// Adds the Multipart Request Server extension to this schema with all default options.
        /// </summary>
        /// <param name="schemaOptions">The schema options to append the extension to.</param>
        /// <returns>SchemaOptions.</returns>
        public static SchemaOptions AddMultipartRequestSupport(this SchemaOptions schemaOptions)
        {
            return AddMultipartRequestSupport(schemaOptions, (o) => { });
        }

        /// <summary>
        /// Adds the Multipart Request Server extension to this schema with a set of custom configured options.
        /// </summary>
        /// <param name="schemaOptions">The schema options to append the extension to.</param>
        /// <param name="configureAction">An action that, when executed, will configure
        /// all available options for this extension.</param>
        /// <returns>SchemaOptions.</returns>
        public static SchemaOptions AddMultipartRequestSupport(this SchemaOptions schemaOptions, Action<MultipartRequestConfiguration> configureAction)
        {
            var extension = new MultipartRequestServerExtension(configureAction);
            schemaOptions.RegisterExtension(extension);
            return schemaOptions;
        }
    }
}