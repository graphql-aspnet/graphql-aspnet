// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template describing the metadata bout an enum graph type.
    /// </summary>
    public interface IGraphDirectiveTemplate : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the locations where this directive has been defined for usage.
        /// </summary>
        /// <value>The locations.</value>
        DirectiveLocation Locations { get; }

        /// <summary>
        /// Attempts to find a declared graph method that can handle processing of the life cycle and location
        /// requested of the directive.
        /// </summary>
        /// <param name="lifeCycle">The life cycle.</param>
        /// <returns>IGraphMethod.</returns>
        IGraphMethod FindMethod(DirectiveLifeCycle lifeCycle);

        /// <summary>
        /// Creates a resolver capable of completing a resolution of this directive.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        IGraphDirectiveResolver CreateResolver();

        /// <summary>
        /// Gets the argument collection this directive contains.
        /// </summary>
        /// <value>The arguments.</value>
        IEnumerable<IGraphFieldArgumentTemplate> Arguments { get; }
    }
}