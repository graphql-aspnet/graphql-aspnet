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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A request to perform some augmented or conditional processing
    /// on a segment of a query document or schema item.
    /// </summary>
    public interface IGraphDirectiveRequest : IDataRequest
    {
        /// <summary>
        /// Gets the original operation request under which this directive is executing.
        /// </summary>
        /// <remarks>
        /// This value will be null during the execution of Type System Directives at startup.
        /// </remarks>
        /// <value>The operation request.</value>
        IQueryExecutionRequest QueryRequest { get; }

        /// <summary>
        /// Gets the invocation context containing the specific details
        /// of the directive to be processed against the <see cref="DirectiveTarget"/>.
        /// </summary>
        /// <value>The invocation context.</value>
        IDirectiveInvocationContext InvocationContext { get; }

        /// <summary>
        /// Gets or sets the target object this directive is being executed for. This is
        /// usually a <see cref="IDocumentPart"/> during execution or a <see cref="ISchemaItem"/>
        /// during schema generation and setup.
        /// </summary>
        /// <value>The directive target.</value>
        object DirectiveTarget { get; set; }

        /// <summary>
        /// Gets a value indicating the directive execution phase this request is scoped under.
        /// </summary>
        /// <value>The directive phase.</value>
        DirectiveInvocationPhase DirectivePhase { get; }

        /// <summary>
        /// Gets the directive graph type that is targeted by this request.
        /// </summary>
        /// <value>The directive.</value>
        IDirective Directive { get; }
    }
}