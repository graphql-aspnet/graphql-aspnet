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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A request to authorize and authenticate given user security context against
    /// a secured schema item.
    /// </summary>
    public class GraphSchemaItemSecurityRequest : IGraphSchemaItemSecurityRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaItemSecurityRequest"/> class.
        /// </summary>
        /// <param name="parentRequest">The parent directive execution request that invoked this authorization check.</param>
        public GraphSchemaItemSecurityRequest(IGraphDirectiveRequest parentRequest)
        {
            Validation.ThrowIfNull(parentRequest, nameof(parentRequest));
            this.Id = parentRequest.Id;
            this.SecureSchemaItem = parentRequest.Directive;
            this.Origin = parentRequest.Origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaItemSecurityRequest" /> class.
        /// </summary>
        /// <param name="parentRequest">The parent field execution request that invoked this authorization check.</param>
        public GraphSchemaItemSecurityRequest(IGraphFieldRequest parentRequest)
        {
            Validation.ThrowIfNull(parentRequest, nameof(parentRequest));
            this.Id = parentRequest.Id;
            this.SecureSchemaItem = parentRequest.Field;
            this.Origin = parentRequest.Origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaItemSecurityRequest"/> class.
        /// </summary>
        /// <param name="invocationContext">The invocation context through which this authorization request
        /// is occuring.</param>
        public GraphSchemaItemSecurityRequest(IGraphFieldInvocationContext invocationContext)
        {
            Validation.ThrowIfNull(invocationContext, nameof(invocationContext));
            this.Id = Guid.NewGuid().ToString("N");
            this.SecureSchemaItem = invocationContext.Field;
            this.Origin = invocationContext.Origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaItemSecurityRequest" /> class.
        /// </summary>
        /// <param name="securedDocumentPart">The secured document part that must
        /// be authorized.</param>
        public GraphSchemaItemSecurityRequest(ISecureDocumentPart securedDocumentPart)
        {
            Validation.ThrowIfNull(securedDocumentPart, nameof(securedDocumentPart));
            this.Id = Guid.NewGuid().ToString("N");
            this.SecureSchemaItem = securedDocumentPart.SecureItem;
            this.Origin = securedDocumentPart.Node.Location.AsOrigin();
        }

        /// <summary>
        /// Gets the globally unique Id assigned to this individual field request.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }

        /// <summary>
        /// Gets the secured item being checked with this request.
        /// </summary>
        /// <value>The field.</value>
        public ISecureSchemaItem SecureSchemaItem { get; }

        /// <summary>
        /// Gets the origin point in the source text where this request was generated.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }
    }
}