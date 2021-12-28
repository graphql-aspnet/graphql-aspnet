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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A request, resolved by a <see cref="IDirectiveGraphType"/> to perform some augmented
    /// or conditional processing on a segment of a query document or schema item.
    /// </summary>
    public interface IGraphDirectiveRequest : IDataRequest
    {
        /// <summary>
        /// Clones this request setting the given data items.
        /// </summary>
        /// <param name="phase">The phase of execution the directive is currently
        /// processing.</param>
        /// <param name="targetData">The data item being passed through this directive, if any.
        /// Typically a field result during execution or a schema item during schema building.</param>
        /// <returns>IGraphDirectiveRequest.</returns>
        IGraphDirectiveRequest AtPhase(
            DirectiveInvocationPhase phase,
            object targetData = null);

        /// <summary>
        /// Gets the directive being executed.
        /// </summary>
        /// <value>The directive.</value>
        IDirectiveGraphType Directive { get; }

        /// <summary>
        /// Gets the <see cref="DirectiveLocation"/> where the directive was invoked.
        /// </summary>
        /// <value>The location.</value>
        DirectiveLocation DirectiveLocation { get; }

        /// <summary>
        /// Gets the current phase of execution this directive is processing.
        /// </summary>
        /// <value>The directive phase.</value>
        DirectiveInvocationPhase DirectivePhase { get; }

        /// <summary>
        /// Gets or sets the target object this directive is being executed for. This is
        /// usually the result of a field resolution during execution or a <see cref="ISchemaItem"/>
        /// during schema generation and setup.
        /// </summary>
        /// <value>The directive target.</value>
        object DirectiveTarget { get; set; }
    }
}