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
    using System.Net.Http;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Helper methods for configuring the multipart request extension.
    /// </summary>
    public static class MultipartRequestExtensionMethods
    {
        /// <summary>
        /// Adds the Multipart Request Server extension to this schema with all default options.
        /// </summary>
        /// <param name="schemaOptions">The schema options to append the extension to.</param>
        /// <returns>SchemaOptions.</returns>
        public static SchemaOptions AddMultipartRequestSupport(this SchemaOptions schemaOptions)
        {
            return AddMultipartRequestSupport(schemaOptions, null);
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
            if (configureAction == null)
                configureAction = (o) => { };

            var extension = new MultipartRequestServerExtension(configureAction);
            schemaOptions.RegisterExtension(extension);
            return schemaOptions;
        }

        /// <summary>
        /// Determines whether this context represents a valid multi-part form submited via a POST request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if the context is a vlaid multi-part form; otherwise, <c>false</c>.</returns>
        public static bool IsMultipartFormRequest(this HttpContext context)
        {
            return context?.Response != null
                && string.Equals(context.Request.Method, nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase)
                && context.Request.HasFormContentType;
        }
    }
}