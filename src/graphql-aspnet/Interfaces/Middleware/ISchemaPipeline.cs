// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Middleware
{
    using System.Collections.Generic;

    /// <summary>
    /// A base collection of components describing a pipeline that performs some chain of work.
    /// </summary>
    public interface ISchemaPipeline
    {
        /// <summary>
        /// Gets the friendly name assigned to this pipeline.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a list, in execution order, of the middleware component names that are
        /// registered to this pipeline.
        /// </summary>
        /// <value>The middleware component names.</value>
        IReadOnlyList<string> MiddlewareComponentNames { get; }
    }
}