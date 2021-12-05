// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Security
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A request to authorize a graph field to a given user.
    /// </summary>
    public class GraphFieldSecurityRequest : IGraphFieldSecurityRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldSecurityRequest" /> class.
        /// </summary>
        /// <param name="parentRequest">The parent field execution request that invoked this authorization check.</param>
        public GraphFieldSecurityRequest(IGraphFieldRequest parentRequest)
        {
            Validation.ThrowIfNull(parentRequest, nameof(parentRequest));
            this.Id = parentRequest.Id;
            this.Field = parentRequest.Field;
            this.Origin = parentRequest.Origin;
            this.Items = new MetaDataCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldSecurityRequest"/> class.
        /// </summary>
        /// <param name="invocationContext">The invocation context through which this authorization request
        /// is occuring.</param>
        public GraphFieldSecurityRequest(IGraphFieldInvocationContext invocationContext)
        {
            Validation.ThrowIfNull(invocationContext, nameof(invocationContext));
            this.Id = Guid.NewGuid().ToString("N");
            this.Field = invocationContext.Field;
            this.Origin = invocationContext.Origin;
            this.Items = new MetaDataCollection();
        }

        /// <summary>
        /// Gets the globally unique Id assigned to this individual field request.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }

        /// <summary>
        /// Gets the field being queried with this request.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field { get; }

        /// <summary>
        /// Gets any additional metadata or items assigned to this request.
        /// </summary>
        /// <value>The metadata.</value>
        public MetaDataCollection Items { get; }

        /// <summary>
        /// Gets the origin point in the source text where this request was generated.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }
    }
}