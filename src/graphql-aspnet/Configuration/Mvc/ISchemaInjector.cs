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
    using System;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// An interface used by the injector to expose its useschema method across
    /// multiple generic types.
    /// </summary>
    internal interface ISchemaInjector
    {
        /// <summary>
        /// Performs a final setup and initial cache build for the schema in the app domain.
        /// </summary>
        /// <param name="serviceProvider">The generated service provider from which to extract
        /// services and perform a final schema setup.</param>
        void UseSchema(IServiceProvider serviceProvider);

        /// <summary>
        /// Performs a final setup and initial cache build for the schema in the app domain. Also registers
        /// the route to the application if required.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        void UseSchema(IApplicationBuilder appBuilder);
    }
}