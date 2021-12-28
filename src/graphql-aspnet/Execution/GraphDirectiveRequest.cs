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
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Execution.FieldResolution;

    /// <summary>
    /// A request, resolved by a <see cref="GraphDirective"/> to perform some augmented
    /// or conditional processing on a segment of a query document.
    /// </summary>
    [DebuggerDisplay("@{Directive.Name}  (LifeCylce = {LifeCycle})")]
    public class GraphDirectiveRequest : IGraphDirectiveRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveRequest" /> class.
        /// </summary>
        /// <param name="targetDirective">The target directive.</param>
        /// <param name="location">The location.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="requestMetaData">The request meta data.</param>
        public GraphDirectiveRequest(
            IDirectiveGraphType targetDirective,
            DirectiveLocation location,
            SourceOrigin origin,
            MetaDataCollection requestMetaData = null)
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.Directive = Validation.ThrowIfNullOrReturn(targetDirective, nameof(targetDirective));
            this.DirectiveLocation = location;
            this.Origin = origin ?? SourceOrigin.None;
            this.Items = requestMetaData ?? new MetaDataCollection();
        }

        /// <inheritdoc/>
        public IGraphDirectiveRequest AtPhase(
            DirectiveInvocationPhase phase,
            object targetData = null)
        {
            var request = new GraphDirectiveRequest(
                this.Directive,
                this.DirectiveLocation,
                this.Origin,
                this.Items);

            request.Id = this.Id;
            request.DirectiveTarget = targetData;
            request.DirectivePhase = phase;
            return request;
        }

        /// <inheritdoc />
        public string Id { get; private set; }

        /// <inheritdoc />
        public IDirectiveGraphType Directive { get;  }

        /// <inheritdoc />
        public SourceOrigin Origin { get; }

        /// <inheritdoc />
        public MetaDataCollection Items { get; }

        /// <inheritdoc />
        public DirectiveLocation DirectiveLocation { get; }

        /// <inheritdoc />
        public DirectiveInvocationPhase DirectivePhase { get; private set; }

        /// <inheritdoc />
        public object DirectiveTarget { get; set; }
    }
}