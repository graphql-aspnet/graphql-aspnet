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
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An template describing a field on an OBJECT or INTERFACE graph type that
    /// is created from a C# object method.
    /// </summary>
    public class MethodGraphFieldTemplate : MethodGraphFieldTemplateBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="ownerTypeKind">The kind of object that will own this field.</param>
        public MethodGraphFieldTemplate(IGraphTypeTemplate parent, MethodInfo methodInfo, TypeKind ownerTypeKind)
            : base(parent, methodInfo)
        {
            this.OwnerTypeKind = ownerTypeKind;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="attributeProvider">A custom, external attribute provider to use instead for extracting
        /// configuration attributes instead of the provider on <paramref name="methodInfo"/>.</param>
        /// <param name="ownerTypeKind">The kind of object that will own this field.</param>
        public MethodGraphFieldTemplate(IGraphTypeTemplate parent, MethodInfo methodInfo, ICustomAttributeProvider attributeProvider, TypeKind ownerTypeKind)
            : base(parent, methodInfo, attributeProvider)
        {
            this.OwnerTypeKind = ownerTypeKind;
        }

        /// <inheritdoc />
        protected override SchemaItemPath GenerateFieldPath()
        {
            // an object method cannot contain any route pathing or nesting like controller methods can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = this.Method.SingleAttributeOrDefault<GraphFieldAttribute>()?.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Method.Name).Trim();

            GraphValidation.EnsureGraphNameOrThrow(this.InternalFullName, graphName);
            return new SchemaItemPath(SchemaItemPath.Join(this.Parent.Route.Path, graphName));
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            // as a matter of convention enforcement
            // for methods delcared as fields on POCOs (not controller actions)
            // force them to use [GraphField] or throw an exception
            // do not allow [Query] [Mutation] etc. and keep those reserved for controllers
            var declaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>()?.GetType();
            if (declaration != null && declaration != typeof(GraphFieldAttribute))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid graph method declaration. The method '{this.InternalFullName}' declares a '{declaration.FriendlyName()}'. This " +
                    $"attribute is reserved for controller actions. For a general object type use '{nameof(GraphFieldAttribute)}' instead.");
            }
        }

        /// <inheritdoc />
        public override TypeKind OwnerTypeKind { get; }
    }
}