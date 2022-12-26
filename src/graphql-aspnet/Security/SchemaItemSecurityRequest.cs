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
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A request to authorize and authenticate given user security context against
    /// a secured schema item.
    /// </summary>
    public sealed class SchemaItemSecurityRequest : ISchemaItemSecurityRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemSecurityRequest"/> class.
        /// </summary>
        /// <param name="parentRequest">The parent directive execution request that invoked this authorization check.</param>
        public SchemaItemSecurityRequest(IGraphDirectiveRequest parentRequest)
        {
            Validation.ThrowIfNull(parentRequest, nameof(parentRequest));
            this.Id = parentRequest.Id;
            this.SecureSchemaItem = parentRequest.Directive;
            this.Origin = parentRequest.Origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemSecurityRequest" /> class.
        /// </summary>
        /// <param name="parentRequest">The parent field execution request that invoked this authorization check.</param>
        public SchemaItemSecurityRequest(IGraphFieldRequest parentRequest)
        {
            Validation.ThrowIfNull(parentRequest, nameof(parentRequest));
            this.Id = parentRequest.Id;
            this.SecureSchemaItem = parentRequest.Field;
            this.Origin = parentRequest.Origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemSecurityRequest"/> class.
        /// </summary>
        /// <param name="invocationContext">The invocation context through which this authorization request
        /// is occuring.</param>
        public SchemaItemSecurityRequest(IGraphFieldInvocationContext invocationContext)
        {
            Validation.ThrowIfNull(invocationContext, nameof(invocationContext));
            this.Id = Guid.NewGuid();
            this.SecureSchemaItem = invocationContext.Field;
            this.Origin = invocationContext.Origin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemSecurityRequest" /> class.
        /// </summary>
        /// <param name="securedDocumentPart">The secured document part that must
        /// be authorized.</param>
        public SchemaItemSecurityRequest(ISecurableDocumentPart securedDocumentPart)
        {
            Validation.ThrowIfNull(securedDocumentPart, nameof(securedDocumentPart));
            this.Id = Guid.NewGuid();
            this.SecureSchemaItem = securedDocumentPart.SecureItem;
            this.Origin = securedDocumentPart.SourceLocation.AsOrigin();
        }

        /// <summary>
        /// Gets the globally unique Id assigned to this individual field request.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; }

        /// <summary>
        /// Gets the secured item being checked with this request.
        /// </summary>
        /// <value>The field.</value>
        public ISecurableSchemaItem SecureSchemaItem { get; }

        /// <summary>
        /// Gets the origin point in the source text where this request was generated.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }
    }
}