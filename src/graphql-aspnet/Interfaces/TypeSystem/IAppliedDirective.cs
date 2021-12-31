// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System;
    using GraphQL.AspNet.Directives;

    /// <summary>
    /// An instance of a directive to be applied to a system item.
    /// </summary>
    public interface IAppliedDirective
    {
        /// <summary>
        /// Gets the concrete type of the directive that has been applied.
        /// </summary>
        /// <value>The type of the directive.</value>
        Type DirectiveType { get; }

        /// <summary>
        /// Gets the collection of arguments supplied on the directive
        /// invocation. These arguments are passed to the <see cref="GraphDirective"/>
        /// created to process the <see cref="ISchemaItem"/>
        /// that owns instance.
        /// </summary>
        /// <value>The arguments.</value>
        object[] Arguments { get; }
    }
}