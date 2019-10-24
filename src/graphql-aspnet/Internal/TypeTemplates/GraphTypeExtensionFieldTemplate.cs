// *************************************************************
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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A method template containing the metadata for a controller method flaged as a special type extension
    /// and not a normal controller action.
    /// </summary>
    public class GraphTypeExtensionFieldTemplate : MethodGraphFieldTemplate, IGraphTypeExpressionDeclaration
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
            _typeAttrib = this.SingleAttributeOfTypeOrDefault<TypeExtensionAttribute>();
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
                    this.TypeExpression = GraphValidation.GenerateTypeExpression(returnType, this);
                    this.PossibleTypes.Insert(0, this.ObjectType);
                }
            }
        }

        /// <summary>
        /// Creates a resolver capable of resolving this field.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        public override IGraphFieldResolver CreateResolver()
        {
            return new GraphControllerActionResolver(this);
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            if (_typeAttrib == null)
            {
                // should be an impossible situation but just in case someone manually invokes this template bypassing global checks.
                throw new GraphTypeDeclarationException(
                    $"The type extension '{this.InternalFullName}' does not define a {typeof(TypeExtensionAttribute).FriendlyName()} or defines more than one instance. " +
                    "All methods wishing to be treated as type extensions must define one instance of this attribute to properly configure the runtime.");
            }

            base.ValidateOrThrow();
        }

        /// <summary>
        /// When overridden in a child class, this method builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            // extract the parent name from the global meta data about the type being extended
            var parentName = GraphTypeNames.ParseName(_typeAttrib.TypeToExtend, TypeKind.OBJECT);

            // an object method cannot contain any route pathing or nesting like controller methods can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = _typeAttrib.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Method.Name).Trim();

            return new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Types, parentName, graphName));
        }

        /// <summary>
        /// Gets the kind of graph type that should own fields created from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.OBJECT;

        /// <summary>
        /// Gets the type of the object that owns this field; that is the type which supplies source data
        /// to this field in the object graph. This is usually the <see cref="GraphTypeFieldTemplate.Parent"/>'s object type but not always; such
        /// is the case with type extensions.
        /// </summary>
        /// <value>The type of the source object.</value>
        public override Type SourceObjectType => _sourceType;

        /// <summary>
        /// Gets a value indicating whether this instance has a defined default value.
        /// </summary>
        /// <value><c>true</c> if this instance has a default value; otherwise, <c>false</c>.</value>
        bool IGraphTypeExpressionDeclaration.HasDefaultValue => false;

        /// <summary>
        /// Gets the actual type wrappers used to generate a type expression for this field.
        /// This list represents the type requirements  of the field.
        /// </summary>
        /// <value>The custom wrappers.</value>
        MetaGraphTypes[] IGraphTypeExpressionDeclaration.TypeWrappers => _typeAttrib?.TypeDefinition;
    }
}