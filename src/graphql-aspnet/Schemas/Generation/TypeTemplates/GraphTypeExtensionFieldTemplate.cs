// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
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
        /// Initializes a new instance of the <see cref="GraphTypeExtensionFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="attributeProvider">A custom, external attribute provider to use instead for extracting
        /// configuration attributes instead of the provider on <paramref name="methodInfo" />.</param>
        public GraphTypeExtensionFieldTemplate(IGraphTypeTemplate parent, MethodInfo methodInfo, ICustomAttributeProvider attributeProvider)
            : base(parent, methodInfo, attributeProvider)
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

            // rebuild the returned object type and the type expression of the field
            // to account for the extra values required when dealing with an explictly
            // declared batch extension
            var returnType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredReturnType);
            if (!Validation.IsCastable<IGraphActionResult>(returnType) && _typeAttrib.ExecutionMode == FieldResolutionMode.Batch)
            {
                // inspect the return type, if its a valid dictionary extract the return type from the value
                // and set the type modifiers and method type based on the value of each dictionary entry
                returnType = returnType.GetValueTypeOfDictionary();
                var wrappers = GraphTypeExpression.FromType(returnType).Wrappers;
                this.ObjectType = GraphValidation.EliminateWrappersFromCoreType(returnType);
                this.TypeExpression = GraphTypeExpression
                    .FromType(returnType, this.DeclaredTypeWrappers ?? wrappers)
                    .Clone(Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME);

                this.PossibleObjectTypes.Insert(0, this.ObjectType);
            }
        }

        /// <inheritdoc />
        public override IGraphFieldResolver CreateResolver()
        {
            return new GraphControllerActionResolver(this.CreateResolverMetaData());
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            if (_typeAttrib == null)
            {
                // should be an impossible situation but just in case someone manually invokes this template bypassing global checks.
                throw new GraphTypeDeclarationException(
                    $"The type extension '{this.InternalName}' does not define a {typeof(TypeExtensionAttribute).FriendlyName()} or defines more than one instance. " +
                    "All methods wishing to be treated as type extensions must define one instance of this attribute to properly configure the runtime.");
            }

            if (!this.SourceObjectType.IsClass && !this.SourceObjectType.IsStruct() && !this.SourceObjectType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The type extension '{this.InternalName}' is attempting to extend '{this.SourceObjectType.FriendlyName()}'. " +
                    "Only classes, structs and  interfaces can be extended.");
            }

            // a specialized redeclaration of this rule on the type extension to
            // better contextualize the message to be just the template value
            if (this.ItemPath == null || !this.ItemPath.IsValid)
            {
                throw new GraphTypeDeclarationException(
                        $"The type extension '{this.InternalName}' declares an invalid field name of '{_typeAttrib.Template ?? "<null>"}'. " +
                        $"Each segment of the item path must conform to standard graphql naming rules. (Regex: {Constants.RegExPatterns.NameRegex} )",
                        this.ObjectType);
            }

            base.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        protected override ItemPath GenerateFieldPath()
        {
            // extract the parent name from the global meta data about the type being extended
            var parentName = GraphTypeNames.ParseName(_typeAttrib.TypeToExtend, TypeKind.OBJECT);

            // an object method cannot contain any pathing or nesting like controller methods can
            // before creating the item path, ensure that the declared name, by itself, is valid for graphql
            var graphName = _typeAttrib.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Method.Name).Trim();

            return new ItemPath(ItemPath.Join(ItemPathRoots.Types, parentName, graphName));
        }

        /// <inheritdoc />
        public override TypeKind OwnerTypeKind => TypeKind.OBJECT;

        /// <inheritdoc />
        public override Type SourceObjectType => _sourceType;
    }
}