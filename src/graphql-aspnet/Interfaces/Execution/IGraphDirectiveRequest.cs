// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A request, resolved by a <see cref="IDirectiveGraphType"/> to perform some augmented
    /// or conditional processing on a segment of a query document.
    /// </summary>
    public interface IGraphDirectiveRequest : IInvocationRequest
    {
        /// <summary>
        /// Clones this request for the given lifecycle location.
        /// </summary>
        /// <param name="lifecycle">The lifecycle point at which the directive request should be pointed.</param>
        /// <param name="dataSource">The data source being passed to the field this directive is attached to, if any.</param>
        /// <returns>GraphDirectiveRequest.</returns>
        IGraphDirectiveRequest ForLifeCycle(
            DirectiveLifeCycle lifecycle,
            GraphFieldDataSource dataSource);

        /// <summary>
        /// Gets the directive being executed.
        /// </summary>
        /// <value>The directive.</value>
        IDirectiveGraphType Directive { get; }

        /// <summary>
        /// Gets the life cycle method being invoked.
        /// </summary>
        /// <value>The life cycle.</value>
        DirectiveLifeCycle LifeCycle { get; }

        /// <summary>
        /// Gets the <see cref="DirectiveLocation"/> where the directive was declared in the source document.
        /// </summary>
        /// <value>The location.</value>
        DirectiveLocation DirectiveLocation { get; }
    }
}