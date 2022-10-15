// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A request, resolved by a <see cref="GraphDirective"/> to perform some augmented
    /// or conditional processing on a segment of a query document.
    /// </summary>
    [DebuggerDisplay("@{InvocationContext.Directive.Name}  (Phase = {DirectivePhase})")]
    public class GraphDirectiveRequest : IGraphDirectiveRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveRequest" /> class.
        /// </summary>
        /// <param name="operationRequest">The operation request governing
        /// this execution directive invocation.</param>
        /// <param name="invocationContext">The context detailing the specifics
        /// of what directive needs to be processed to fulfill this request.</param>
        /// <param name="invocationPhase">The invocation current phase that the <paramref name="invocationContext" />
        /// is being processed through.</param>
        /// <param name="targetData">The real data object that is the target of this request.</param>
        public GraphDirectiveRequest(
            IGraphOperationRequest operationRequest,
            IDirectiveInvocationContext invocationContext,
            DirectiveInvocationPhase invocationPhase,
            object targetData)
            : this(invocationContext, invocationPhase, targetData)
        {
            this.OperationRequest = Validation.ThrowIfNullOrReturn(operationRequest, nameof(operationRequest));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveRequest" /> class.
        /// </summary>
        /// <param name="invocationContext">The context detailing the specifics
        /// of what directive needs to be processed to fulfill this request.</param>
        /// <param name="invocationPhase">The invocation current phase that the <paramref name="invocationContext" />
        /// is being processed through.</param>
        /// <param name="targetData">The real data object that is the target of this request.</param>
        public GraphDirectiveRequest(
            IDirectiveInvocationContext invocationContext,
            DirectiveInvocationPhase invocationPhase,
            object targetData)
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.InvocationContext = Validation.ThrowIfNullOrReturn(invocationContext, nameof(invocationContext));
            this.DirectivePhase = invocationPhase;
            this.DirectiveTarget = targetData;
            this.OperationRequest = null;
        }

        /// <inheritdoc />
        public string Id { get; private set; }

        /// <inheritdoc />
        public IDirectiveInvocationContext InvocationContext { get; }

        /// <inheritdoc />
        public object DirectiveTarget { get; set; }

        /// <inheritdoc />
        public DirectiveInvocationPhase DirectivePhase { get; }

        /// <inheritdoc />
        public SourceOrigin Origin => this.InvocationContext?.Origin ?? SourceOrigin.None;

        /// <inheritdoc />
        public IDirective Directive => this.InvocationContext.Directive;

        /// <inheritdoc />
        public IGraphOperationRequest OperationRequest { get; }
    }
}