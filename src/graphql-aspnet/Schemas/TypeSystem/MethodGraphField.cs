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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A representation of a field as it would be defined in an object graph that originated
    /// from a .NET method or property invocation.
    /// </summary>
    [DebuggerDisplay("Field: {ItemPath.Path}")]
    public class MethodGraphField : IGraphField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the graph.</param>
        /// <param name="internalName">The internal name that represents the method this field respresents.</param>
        /// <param name="typeExpression">The meta data describing the type of data this field returns.</param>
        /// <param name="itemPath">The formal path to this field in the object graph.</param>
        /// <param name="declaredReturnType">The .NET type as it was declared on the property which generated this field..</param>
        /// <param name="objectType">The .NET type of the item or items that represent the graph type returned by this field.</param>
        /// <param name="mode">The mode in which the runtime will process this field.</param>
        /// <param name="resolver">The resolver to be invoked to produce data when this field is called.</param>
        /// <param name="securityPolicies">The security policies that apply to this field.</param>
        /// <param name="directives">The directives to apply to this field when its added to a schema.</param>
        public MethodGraphField(
            string fieldName,
            string internalName,
            GraphTypeExpression typeExpression,
            ItemPath itemPath,
            Type declaredReturnType,
            Type objectType,
            FieldResolutionMode mode = FieldResolutionMode.PerSourceItem,
            IGraphFieldResolver resolver = null,
            IEnumerable<AppliedSecurityPolicyGroup> securityPolicies = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ItemPath = Validation.ThrowIfNullOrReturn(itemPath, nameof(itemPath));
            this.Arguments = new GraphFieldArgumentCollection(this);
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.DeclaredReturnType = Validation.ThrowIfNullOrReturn(declaredReturnType, nameof(declaredReturnType));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);

            this.SecurityGroups = new AppliedSecurityPolicyGroups(securityPolicies);

            this.UpdateResolver(resolver, mode);
            this.Publish = true;
        }

        /// <inheritdoc/>
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode? mode = null)
        {
            this.Resolver = newResolver;
            if (mode.HasValue)
                this.Mode = mode.Value;
        }

        /// <inheritdoc/>
        public virtual IGraphField Clone(
            ISchemaItem parent = null,
            string fieldName = null,
            GraphTypeExpression typeExpression = null)
        {
            parent = parent ?? this.Parent;
            fieldName = fieldName?.Trim() ?? this.Name;

            var clonedItem = this.CreateNewInstance();

            // paths defined on operations cannot be repathed
            // as they represent declared locations on the schema
            var path = this.ItemPath.Clone();
            if (!this.ItemPath.IsOperationRoot)
                path = parent?.ItemPath.CreateChild(this.ItemPath.Name) ?? path;

            // assign all publically alterable fields
            clonedItem.Name = fieldName;
            clonedItem.ItemPath = path;
            clonedItem.TypeExpression = typeExpression ?? this.TypeExpression.Clone();
            clonedItem.Description = this.Description;
            clonedItem.Publish = this.Publish;
            clonedItem.Complexity = this.Complexity;
            clonedItem.IsDeprecated = this.IsDeprecated;
            clonedItem.DeprecationReason = this.DeprecationReason;
            clonedItem.FieldSource = this.FieldSource;
            clonedItem.Parent = parent;

            // clone over the arguments
            foreach (var argument in this.Arguments)
                clonedItem.Arguments.AddArgument(argument.Clone(clonedItem));

            return clonedItem;
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
        /// <remarks>
        /// This method is used as the basis for new object creation during cloning.
        /// </remarks>
        /// <returns>IGraphField.</returns>
        protected virtual MethodGraphField CreateNewInstance()
        {
            return new MethodGraphField(
                this.Name,
                this.InternalName,
                this.TypeExpression.Clone(),
                this.ItemPath,
                this.DeclaredReturnType,
                this.ObjectType,
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
        public IAppliedSecurityPolicyGroups SecurityGroups { get; }

        /// <inheritdoc/>
        public IGraphArgumentCollection Arguments { get; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public virtual bool Publish { get; set; }

        /// <inheritdoc/>
        public ItemPath ItemPath { get; protected set; }

        /// <inheritdoc/>
        public IGraphFieldResolver Resolver { get; protected set; }

        /// <inheritdoc/>
        public FieldResolutionMode Mode { get; protected set; }

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
        public ISchemaItem Parent { get; protected set; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public string InternalName { get; }
    }
}