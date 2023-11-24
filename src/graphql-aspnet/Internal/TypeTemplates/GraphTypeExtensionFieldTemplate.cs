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
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template defining a field for OBJECT or INTERFACE graph types of which the C# code
    /// for the field is not part of the target graph type's defining class or struct.
    /// </summary>
    public class GraphTypeExtensionFieldTemplate : MethodGraphFieldTemplateBase
    {
        private Type _sourceType;
        private TypeExtensionAttribute _typeAttrib;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeExtensionFieldTemplate"/> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        public GraphTypeExtensionFieldTemplate(IGraphTypeTemplate parent, MethodInfo methodInfo)
            : base(parent, methodInfo)
        {
        }

        /// <summary>
        /// Builds out this field template parsing out the required type declarations unions, nameing scheme etc.  This method should be
        /// overridden in any child classes for additional decalration requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            // ensure type extension right out of the gate
            // the field can't be built otherwise
            _typeAttrib = this.AttributeProvider.SingleAttributeOfTypeOrDefault<TypeExtensionAttribute>();
            if (_typeAttrib == null)
                return;

            _sourceType = _typeAttrib.TypeToExtend;

            base.ParseTemplateDefinition();

            var returnType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredReturnType);
            if (returnType != typeof(IGraphActionResult))
            {
                // inspect the return type, if its a valid dictionary extract the return type from the value
                // and set the type modifiers and method type based on the value of each dictionary entry
                if (_typeAttrib.ExecutionMode == FieldResolutionMode.Batch)
                {
                    returnType = returnType.GetValueTypeOfDictionary();
                    this.ObjectType = GraphValidation.EliminateWrappersFromCoreType(returnType);
                    this.TypeExpression = GraphTypeExpression.FromType(returnType, this.DeclaredTypeWrappers);
                    this.PossibleTypes.Insert(0, this.ObjectType);
                }
            }
        }

        /// <inheritdoc />
        public override IGraphFieldResolver CreateResolver()
        {
            return new GraphControllerActionResolver(this);
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            if (_typeAttrib == null)
            {
                // should be an impossible situation but just in case someone manually invokes this template bypassing global checks.
                throw new GraphTypeDeclarationException(
                    $"The type extension '{this.InternalFullName}' does not define a {typeof(TypeExtensionAttribute).FriendlyName()} or defines more than one instance. " +
                    "All methods wishing to be treated as type extensions must define one instance of this attribute to properly configure the runtime.");
            }

            if (GraphQLProviders.ScalarProvider.IsLeaf(this.SourceObjectType))
            {
                throw new GraphTypeDeclarationException(
                    $"The type extension '{this.InternalFullName}' is attempting to extend '{this.SourceObjectType.FriendlyName()}' which is a leaf type ({nameof(TypeKind.SCALAR)}, {nameof(TypeKind.ENUM)}). " +
                    "Leaf types cannot be extended.");
            }

            base.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        protected override SchemaItemPath GenerateFieldPath()
        {
            // extract the parent name from the global meta data about the type being extended
            var parentName = GraphTypeNames.ParseName(_typeAttrib.TypeToExtend, TypeKind.OBJECT);

            // an object method cannot contain any route pathing or nesting like controller methods can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = _typeAttrib.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Method.Name).Trim();

            return new SchemaItemPath(SchemaItemPath.Join(SchemaItemCollections.Types, parentName, graphName));
        }

        /// <inheritdoc />
        public override TypeKind OwnerTypeKind => TypeKind.OBJECT;

        /// <inheritdoc />
        public override Type SourceObjectType => _sourceType;
    }
}