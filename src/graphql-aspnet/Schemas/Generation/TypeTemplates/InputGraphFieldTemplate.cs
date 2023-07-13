﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An template describing a field on an INPUT_OBJECT graph type that
    /// is created from a C# object property.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public class InputGraphFieldTemplate : SchemaItemTemplateBase, IInputGraphFieldTemplate
    {
        private GraphFieldAttribute _fieldDeclaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The graph type templaate that owns this field.</param>
        /// <param name="propInfo">The property information describing the field.</param>
        public InputGraphFieldTemplate(IInputObjectGraphTypeTemplate parent, PropertyInfo propInfo)
            : base(propInfo)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Property = Validation.ThrowIfNullOrReturn(propInfo, nameof(propInfo));
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            _fieldDeclaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();

            this.Route = this.GenerateFieldPath();
            this.Description = this.AttributeProvider.SingleAttributeOfTypeOrDefault<DescriptionAttribute>()?.Description;

            var objectType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredReturnType);
            this.ObjectType = objectType;

            var typeExpression = GraphTypeExpression.FromType(this.DeclaredReturnType, this.DeclaredTypeWrappers);
            typeExpression = typeExpression.CloneTo(GraphTypeNames.ParseName(objectType, this.Parent.Kind));

            this.IsRequired = this.AttributeProvider.SingleAttributeOrDefault<RequiredAttribute>() != null;
            this.TypeExpression = typeExpression;
        }

        /// <inheritdoc />
        public override IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            var list = new List<DependentType>();
            list.AddRange(base.RetrieveRequiredTypes());
            list.Add(new DependentType(this.ObjectType, GraphValidation.ResolveTypeKind(this.ObjectType, this.OwnerTypeKind)));

            return list;
        }

        private SchemaItemPath GenerateFieldPath()
        {
            var graphName = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>()?.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Property.Name).Trim();

            return new SchemaItemPath(SchemaItemPath.Join(this.Parent.Route.Path, graphName));
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (Validation.IsCastable<Task>(this.DeclaredReturnType))
            {
                throw new GraphTypeDeclarationException(
                    $"The input field  '{this.InternalFullName}' defines a return type of'{nameof(Task)}'.  " +
                    $"Input fields must not be asyncronous.");
            }

            if (this.ObjectType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The input field '{this.InternalFullName}' returns '{this.ObjectType.FriendlyName()}' which is an interface. " +
                    $"Input fields must not return interface objects.");
            }

            if (Validation.IsCastable<IGraphUnionProxy>(this.ObjectType))
            {
                throw new GraphTypeDeclarationException(
                    $"The input field '{this.InternalFullName}' returns '{this.ObjectType.FriendlyName()}' which implements {nameof(IGraphUnionProxy)}. " +
                    $"Input fields must not implement {nameof(IGraphUnionProxy)}.");
            }

            if (Validation.IsCastable<IGraphActionResult>(this.ObjectType))
            {
                throw new GraphTypeDeclarationException(
                    $"The input field '{this.InternalFullName}' returns '{this.ObjectType.FriendlyName()}' which implements {nameof(IGraphActionResult)}. " +
                    $"Input fields must not implement {nameof(IGraphActionResult)}.");
            }
        }

        /// <summary>
        /// Gets the core property information about this template.
        /// </summary>
        /// <value>The property.</value>
        private PropertyInfo Property { get; }

        /// <inheritdoc />
        public bool IsRequired { get; private set; }

        /// <inheritdoc />
        public Type DeclaredReturnType => this.Property.PropertyType;

        /// <inheritdoc />
        public string DeclaredName => this.Property.Name;

        /// <inheritdoc />
        public IGraphTypeTemplate Parent { get; private set; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <inheritdoc />
        public TypeKind OwnerTypeKind => TypeKind.INPUT_OBJECT;

        /// <inheritdoc />
        public override string InternalFullName => $"{this.Parent.InternalFullName}.{this.Property.Name}";

        /// <inheritdoc />
        public override string InternalName => this.Property.Name;

        /// <inheritdoc />
        public MetaGraphTypes[] DeclaredTypeWrappers
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fieldDeclaration?.TypeExpression))
                    return null;

                return GraphTypeExpression
                    .FromDeclaration(_fieldDeclaration.TypeExpression)
                    .Wrappers;
            }
        }
    }
}