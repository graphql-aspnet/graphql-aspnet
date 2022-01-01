// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A representation of a object field as it would be defined in the graph type system.
    /// </summary>
    /// <seealso cref="IGraphField" />
    [DebuggerDisplay("Field: {Route.Path}")]
    public class MethodGraphField : IGraphField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the graph.</param>
        /// <param name="typeExpression">The meta data describing the type of data this field returns.</param>
        /// <param name="route">The formal route to this field in the object graph.</param>
        /// <param name="objectType">The .NET type of the item or items that represent the graph type returned by this field.</param>
        /// <param name="declaredReturnType">The .NET type as it was declared on the property which generated this field..</param>
        /// <param name="mode">The mode in which the runtime will process this field.</param>
        /// <param name="resolver">The resolver to be invoked to produce data when this field is called.</param>
        /// <param name="securityPolicies">The security policies that apply to this field.</param>
        /// <param name="directives">The directives to apply to this field when its added to a schema.</param>
        public MethodGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            GraphFieldPath route,
            Type objectType = null,
            Type declaredReturnType = null,
            FieldResolutionMode mode = FieldResolutionMode.PerSourceItem,
            IGraphFieldResolver resolver = null,
            IEnumerable<FieldSecurityGroup> securityPolicies = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.Arguments = new GraphFieldArgumentCollection(this);
            this.SecurityGroups = securityPolicies ?? Enumerable.Empty<FieldSecurityGroup>();
            this.ObjectType = objectType;
            this.DeclaredReturnType = declaredReturnType;

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);

            this.UpdateResolver(resolver, mode);
        }

        /// <inheritdoc/>
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode? mode = null)
        {
            if (this.Route.Path.Contains("TestPerson") && this.Name == "name")
            {
                var str = "";
            }
            this.Resolver = newResolver;

            if (mode.HasValue)
                this.Mode = mode.Value;

            var unrwrappedType = GraphValidation.EliminateWrappersFromCoreType(this.Resolver?.ObjectType);
            this.IsLeaf = this.Resolver?.ObjectType != null && GraphQLProviders.ScalarProvider.IsLeaf(unrwrappedType);
        }

        /// <inheritdoc/>
        public void AssignParent(IGraphType parent)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public Type DeclaredReturnType { get; set; }

        /// <inheritdoc/>
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc/>
        public IEnumerable<FieldSecurityGroup> SecurityGroups { get; }

        /// <inheritdoc/>
        public IGraphArgumentCollection Arguments { get; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public virtual bool Publish => true;

        /// <inheritdoc/>
        public GraphFieldPath Route { get; }

        /// <inheritdoc/>
        public IGraphFieldResolver Resolver { get; private set; }

        /// <inheritdoc/>
        public FieldResolutionMode Mode { get; private set; }

        /// <inheritdoc/>
        public bool IsLeaf { get; private set; }

        /// <inheritdoc/>
        public bool IsDeprecated { get; internal set; }

        /// <inheritdoc/>
        public string DeprecationReason { get; internal set; }

        /// <inheritdoc/>
        public float? Complexity { get; internal set; }

        /// <inheritdoc/>
        public GraphFieldSource FieldSource { get; internal set; }

        /// <inheritdoc/>
        public bool IsVirtual => false;

        /// <inheritdoc/>
        public ISchemaItem Parent { get; private set; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}