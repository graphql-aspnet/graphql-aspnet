// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// A set of options indicating the various resolver types that may be
    /// executed in isolation. Coorisponds to the identifed <see cref="GraphFieldSource" />
    /// of a field resolver.
    /// </summary>
    [Flags]
    public enum ResolverIsolationOptions
    {
        /// <summary>
        /// Indicates no Isolation Options
        /// </summary>
        None = GraphFieldSource.None,

        /// <summary>
        /// When a controller action is encountered it will be executed
        /// in isolation, no other reslolvers will be allowed to execute while
        /// the resolver action is processing. This includes Type and Batch Extensions.
        /// </summary>
        ControllerActions = GraphFieldSource.Action,

        /// <summary>
        /// When an object method resolver is encountered it will be executed in isolation,
        /// no other reslolvers will be allowed to execute while the method resolver is processing.
        /// </summary>
        Methods = GraphFieldSource.Method,

        /// <summary>
        /// When an object property resolver is encountered it will be executed in isolation,
        /// no other reslolvers will be allowed to execute while the property resolver is processing.
        /// </summary>
        Properties = GraphFieldSource.Property,

        /// <summary>
        /// Indicates that all field resolvers are executed in isolation.
        /// </summary>
        All = ControllerActions | Methods | Properties,
    }
}