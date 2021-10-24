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
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A parsed description of the meta data of any "general method" that should be represented
    /// as a field on a type in an <see cref="ISchema"/>.
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
        /// When overridden in a child class, this metyhod builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            // an object method cannot contain any route pathing or nesting like controller methods can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = this.Method.SingleAttributeOrDefault<GraphFieldAttribute>()?.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Method.Name).Trim();

            GraphValidation.EnsureGraphNameOrThrow(this.InternalFullName, graphName);
            return new GraphFieldPath(GraphFieldPath.Join(this.Parent.Route.Path, graphName));
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            // as a matter of convention enforcement
            // for methods delcared as fields on POCOs (not controller actions)
            // force them to use [GraphField] or throw an exception
            // do not allow [Query] [Mutation] etc. and keep those reserved for controllers
            var declaration = this.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>()?.GetType();
            if (declaration != null && declaration != typeof(GraphFieldAttribute))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid graph method declaration. The method '{this.InternalFullName}' declares a '{declaration.FriendlyName()}'. This " +
                    $"attribute is reserved for controller actions. For a general object type use '{nameof(GraphFieldAttribute)}' instead.");
            }
        }

        /// <summary>
        /// Gets the kind of graph type that should own fields created from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind OwnerTypeKind { get; }
    }
}