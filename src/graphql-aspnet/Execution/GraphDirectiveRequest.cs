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
            this.LifeCycle = DirectiveLifeCycle.BeforeResolution;
            this.DirectiveLocation = location;
            this.Origin = origin ?? SourceOrigin.None;
            this.Items = requestMetaData ?? new MetaDataCollection();
        }

        /// <summary>
        /// Clones this request for the given lifecycle location.
        /// </summary>
        /// <param name="lifecycle">The lifecycle point at which the directive request should be pointed.</param>
        /// <param name="dataSource">The data source being passed to the field this directive is attached to, if any.</param>
        /// <returns>GraphDirectiveRequest.</returns>
        public IGraphDirectiveRequest ForLifeCycle(
            DirectiveLifeCycle lifecycle,
            GraphFieldDataSource dataSource)
        {
            var request = new GraphDirectiveRequest(
                this.Directive,
                this.DirectiveLocation,
                this.Origin,
                this.Items);

            request.Id = this.Id;
            request.LifeCycle = lifecycle;
            request.DataSource = dataSource;
            return request;
        }

        /// <summary>
        /// Gets a globally unique identifier assigned to this request when it was created.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the life cycle method being invoked.
        /// </summary>
        /// <value>The life cycle.</value>
        public DirectiveLifeCycle LifeCycle { get; private set; }

        /// <summary>
        /// Gets the source data, if any, that was made available to this request.
        /// </summary>
        /// <value>The source data.</value>
        public GraphFieldDataSource DataSource { get; private set; }

        /// <summary>
        /// Gets the directive being executed.
        /// </summary>
        /// <value>The directive.</value>
        public IDirectiveGraphType Directive { get;  }

        /// <summary>
        /// Gets the origin point in the source text where this request was generated.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }

        /// <summary>
        /// Gets any additional metadata or items assigned to this request.
        /// </summary>
        /// <value>The metadata.</value>
        public MetaDataCollection Items { get; }

        /// <summary>
        /// Gets the <see cref="DirectiveLocation" /> where the directive was declared in the source document.
        /// </summary>
        /// <value>The location.</value>
        public DirectiveLocation DirectiveLocation { get; }
    }
}