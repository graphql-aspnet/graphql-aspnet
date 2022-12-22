// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template describing the metadata bout an enum graph type.
    /// </summary>
    public interface IGraphDirectiveTemplate : IGraphTypeTemplate
    {
        /// <summary>
        /// Attempts to find a declared graph method that can handle processing of the life cycle and location
        /// requested of the directive.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>IGraphMethod.</returns>
        IGraphFieldResolverMethod FindMethod(DirectiveLocation location);

        /// <summary>
        /// Creates a resolver capable of completing a resolution of this directive.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        IGraphDirectiveResolver CreateResolver();

        /// <summary>
        /// Gets the locations where this directive has been defined for usage.
        /// </summary>
        /// <value>The locations.</value>
        DirectiveLocation Locations { get; }

        /// <summary>
        /// Gets the argument collection this directive exposes during the execution phase.
        /// </summary>
        /// <value>The arguments.</value>
        IEnumerable<IGraphArgumentTemplate> Arguments { get; }

        /// <summary>
        /// Gets a value indicating whether the directive is defined to be repeatable at a
        /// given target location.
        /// </summary>
        /// <value><c>true</c> if this instance is repeatable; otherwise, <c>false</c>.</value>
        bool IsRepeatable { get; }
    }
}