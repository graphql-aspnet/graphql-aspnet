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
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Describes an action on a <see cref="GraphController"/>, that can be registered
    /// as a graphql field.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public class ControllerActionGraphFieldTemplate : MethodGraphFieldTemplateBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerActionGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="methodInfo">The method information.</param>
        public ControllerActionGraphFieldTemplate(IGraphControllerTemplate parent, MethodInfo methodInfo)
            : base(parent, methodInfo)
        {
        }

        /// <summary>
        /// When overridden in a child class, this metyhod builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override SchemaItemPath GenerateFieldPath()
        {
            // Various meta data fields about the method
            // -------------------------------------------
            var graphMethodAttrib = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            var fieldType = graphMethodAttrib?.FieldType ?? SchemaItemCollections.Unknown;

            var routeFragment = graphMethodAttrib?.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            routeFragment = routeFragment.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Method.Name).Trim();

            // remove the parent fragment this method should be nested under if this method is marked as a root entry
            var parentRouteFragment = (graphMethodAttrib?.IsRootFragment ?? false) ? string.Empty : this.Parent.Route.Path;
            return new SchemaItemPath(SchemaItemPath.Join(fieldType, parentRouteFragment, routeFragment));
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            base.ValidateOrThrow(validateChildren);

            // as a matter of convention enforcement
            // for action methods on controllers
            // force them to use [Query], [Mutation] etc.
            // throw an exception if an attempt to use [GraphField] is made as its reserved for POCO classes
            var declaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>()?.GetType();
            if (declaration != null && declaration == typeof(GraphFieldAttribute))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid action declaration. The controller action method '{this.InternalFullName}' declares " +
                    $"a '{nameof(GraphFieldAttribute)}'. This attribute is reserved for model classes. Controller " +
                    $"actions must declare an operation specific attribute such as '{nameof(QueryAttribute)}', '{nameof(MutationAttribute)}' etc.");
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
        /// Gets the kind of graph type that should own fields created from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind OwnerTypeKind => TypeKind.OBJECT;

        /// <summary>
        /// Gets a value indicating whether this instance was explictly declared as a graph item via acceptable attribution or
        /// if it was parsed as a matter of completeness.
        /// </summary>
        /// <value><c>true</c> if this instance is explictly declared; otherwise, <c>false</c>.</value>
        public override bool IsExplicitDeclaration => true;

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        public override GraphFieldSource FieldSource => GraphFieldSource.Action;
    }
}