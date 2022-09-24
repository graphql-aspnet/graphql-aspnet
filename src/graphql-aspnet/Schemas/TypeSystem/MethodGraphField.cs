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
        private IGraphType _parent = null;

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
            SchemaItemPath route,
            Type objectType = null,
            Type declaredReturnType = null,
            FieldResolutionMode mode = FieldResolutionMode.PerSourceItem,
            IGraphFieldResolver resolver = null,
            IEnumerable<AppliedSecurityPolicyGroup> securityPolicies = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.Arguments = new GraphFieldArgumentCollection(this);
            this.ObjectType = objectType;
            this.DeclaredReturnType = declaredReturnType;

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);

            securityPolicies = securityPolicies ?? Enumerable.Empty<AppliedSecurityPolicyGroup>();
            this.SecurityGroups = new List<AppliedSecurityPolicyGroup>(securityPolicies);

            this.UpdateResolver(resolver, mode);
            this.Publish = true;
        }

        /// <inheritdoc/>
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode? mode = null)
        {
            this.Resolver = newResolver;
            if (mode.HasValue)
                this.Mode = mode.Value;

            var unrwrappedType = GraphValidation.EliminateWrappersFromCoreType(this.Resolver?.ObjectType);
            this.IsLeaf = this.Resolver?.ObjectType != null && GraphQLProviders.ScalarProvider.IsLeaf(unrwrappedType);
        }

        /// <inheritdoc/>
        public void AssignParent(IGraphType parent)
        {
            Validation.ThrowIfNull(parent, nameof(parent));
            _parent = parent;
        }

        /// <inheritdoc/>
        public virtual IGraphField Clone(IGraphType parent)
        {
            Validation.ThrowIfNull(parent, nameof(parent));

            var newField = this.CreateNewInstance(parent);

            newField.Description = this.Description;
            newField.Publish = this.Publish;
            newField.Complexity = this.Complexity;
            newField.IsDeprecated = this.IsDeprecated;
            newField.DeprecationReason = this.DeprecationReason;
            newField.FieldSource = this.FieldSource;

            newField.AssignParent(parent);

            foreach (var argument in this.Arguments)
                newField.Arguments.AddArgument(argument.Clone(newField));

            return newField;
        }

        /// <inheritdoc/>
        public virtual bool CanResolveForGraphType(IGraphType graphType)
        {
            // if the provided graphtype owns this field
            // then yes it can resolve for it
            if (graphType is IGraphFieldContainer fieldContainer)
            {
                if (fieldContainer.Fields.Contains(this))
                    return true;
            }

            // if the target graph type is an object and this field points to
            // an interface that said object implements, its also allowed.
            if (graphType is IObjectGraphType obj)
            {
                if (this.Parent is IInterfaceGraphType igt)
                {
                    if (obj.InterfaceNames.Contains(igt.Name))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new instance of a graph field from this type.
        /// </summary>
        /// <param name="parent">The item to assign as the parent of the new field.</param>
        /// <returns>IGraphField.</returns>
        protected virtual MethodGraphField CreateNewInstance(IGraphType parent)
        {
            return new MethodGraphField(
                this.Name,
                this.TypeExpression.Clone(),
                parent.Route.CreateChild(this.Name),
                this.ObjectType,
                this.DeclaredReturnType,
                this.Mode,
                this.Resolver,
                this.SecurityGroups,
                this.AppliedDirectives);
        }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public Type DeclaredReturnType { get; protected set; }

        /// <inheritdoc/>
        public GraphTypeExpression TypeExpression { get; protected set; }

        /// <inheritdoc/>
        public IEnumerable<AppliedSecurityPolicyGroup> SecurityGroups { get; }

        /// <inheritdoc/>
        public IGraphArgumentCollection Arguments { get; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public virtual bool Publish { get; set; }

        /// <inheritdoc/>
        public SchemaItemPath Route { get; }

        /// <inheritdoc/>
        public IGraphFieldResolver Resolver { get; protected set; }

        /// <inheritdoc/>
        public FieldResolutionMode Mode { get; protected set; }

        /// <inheritdoc/>
        public bool IsLeaf { get; protected set; }

        /// <inheritdoc/>
        public bool IsDeprecated { get; set; }

        /// <inheritdoc/>
        public string DeprecationReason { get; set; }

        /// <inheritdoc/>
        public float? Complexity { get; set; }

        /// <inheritdoc/>
        public GraphFieldSource FieldSource { get; internal set; }

        /// <inheritdoc/>
        public bool IsVirtual => false;

        /// <inheritdoc/>
        public ISchemaItem Parent => _parent;

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}