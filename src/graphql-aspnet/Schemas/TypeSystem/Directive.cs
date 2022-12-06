// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A represention of a graphql directive in the schema. This object defines all the exposed
    /// meta data and invocation information when this directive is queried.
    /// </summary>
    [DebuggerDisplay("DIRECTIVE {Name}")]
    public class Directive : IDirective
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Directive" /> class.
        /// </summary>
        /// <param name="name">The name of the directive as it appears in the schema.</param>
        /// <param name="locations">The locations where this directive is valid.</param>
        /// <param name="directiveType">The concrete type of the directive.</param>
        /// <param name="route">The route path that identifies this directive.</param>
        /// <param name="isRepeatable">if set to <c>true</c> the directive is repeatable
        /// at a target location.</param>
        /// <param name="resolver">The resolver used to process this instance.</param>
        /// <param name="securityGroups">The security groups applied to this directive
        /// that must be passed before this directive can be invoked.</param>
        public Directive(
            string name,
            DirectiveLocation locations,
            Type directiveType,
            SchemaItemPath route,
            bool isRepeatable = false,
            IGraphDirectiveResolver resolver = null,
            IEnumerable<AppliedSecurityPolicyGroup> securityGroups = null)
        {
            this.Name = Validation.ThrowIfNullOrReturn(name, nameof(name));
            this.Arguments = new GraphFieldArgumentCollection(this);
            this.Locations = locations;
            this.Resolver = resolver;
            this.Publish = true;
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.ObjectType = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));
            this.InternalName = this.ObjectType.FriendlyName();

            this.AppliedDirectives = new AppliedDirectiveCollection(this);
            this.IsRepeatable = isRepeatable;
            this.SecurityGroups = new AppliedSecurityPolicyGroups(securityGroups);
        }

        /// <inheritdoc />
        public bool ValidateObject(object item)
        {
            return item == null || item.GetType() == this.ObjectType;
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.DIRECTIVE;

        /// <inheritdoc />
        public bool Publish { get; set; }

        /// <inheritdoc />
        public virtual IGraphArgumentCollection Arguments { get; }

        /// <inheritdoc />
        public IGraphDirectiveResolver Resolver { get; set; }

        /// <inheritdoc />
        public virtual DirectiveLocation Locations { get; }

        /// <inheritdoc />
        public virtual bool IsVirtual => false;

        /// <inheritdoc />
        public virtual IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public bool IsRepeatable { get; set; }

        /// <inheritdoc />
        public IAppliedSecurityPolicyGroups SecurityGroups { get; private set; }
    }
}