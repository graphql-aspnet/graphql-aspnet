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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A set of parsed metadata, read from a declared <see cref="GraphController"/>, to properly
    /// populate graphQL fields from action methods.
    /// </summary>
    [DebuggerDisplay("Controller: '{ObjectType.Name}', Route: '{Route.Path}'")]
    public class GraphControllerTemplate : BaseObjectGraphTypeTemplate, IGraphControllerTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphControllerTemplate"/> class.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        public GraphControllerTemplate(Type controllerType)
            : base(controllerType)
        {
        }

        /// <summary>
        /// When overridden in a child class, this metyhod builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            var skipControllerLevelField = this.ObjectType.SingleAttributeOrDefault<GraphRootAttribute>();
            if (skipControllerLevelField != null)
                return GraphFieldPath.Empty;

            string template = null;
            var graphRoute = this.ObjectType.SingleAttributeOrDefault<GraphRouteAttribute>();
            var symanticName = this.ObjectType.Name.ReplaceLastInstanceOfCaseInvariant(Constants.CommonSuffix.CONTROLLER_SUFFIX, string.Empty);

            // pull the route fragment from the controller level attribute, replacing any meta tags as appropriate
            // if not found, default the name to the symantic name of the controller type itself (e.g. 'HumansController' becomes 'Humans')
            if (graphRoute != null)
            {
                template = graphRoute.Template?.Trim() ?? string.Empty;
                template = template.Replace(Constants.Routing.CONTOLLER_META_NAME, symanticName);
            }

            if (string.IsNullOrWhiteSpace(template))
            {
                template = symanticName;
            }

            return new GraphFieldPath(template);
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            // cant use type naming on controllers (they arent real types and arent included directly in the object graph)
            if (this.SingleAttributeOfTypeOrDefault<GraphTypeAttribute>() != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph controller '{this.ObjectType.FriendlyName()}' defines a {typeof(GraphTypeAttribute).FriendlyName()} but should not. This attribute" +
                    $"is reserved for object types. To alter the naming schema of a controller use '{nameof(GraphRouteAttribute)}' instead.");
            }

            // ensure that root and a route path aren't defined (its one or the other)
            var skipControllerLevelField = this.SingleAttributeOrDefault<GraphRootAttribute>();
            var graphRoute = this.SingleAttributeOrDefault<GraphRouteAttribute>();
            if (skipControllerLevelField != null && graphRoute != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph controller '{this.ObjectType.FriendlyName()}' defines a '{typeof(GraphRootAttribute).FriendlyName()}' and " +
                    $"a '{typeof(GraphRouteAttribute).FriendlyName()}'. These options are mutually exclusive for {typeof(GraphController).FriendlyName()}. Choose" +
                    "one or the other.");
            }

            base.ValidateOrThrow();
        }

        /// <summary>
        /// Determines whether the given container could be used as a graph field either because it is
        /// explictly declared as such or that it conformed to the required parameters of being
        /// a field.
        /// </summary>
        /// <param name="memberInfo">The member information to check.</param>
        /// <returns><c>true</c> if the info represents a possible graph field; otherwise, <c>false</c>.</returns>
        protected override bool CouldBeGraphField(MemberInfo memberInfo)
        {
            if (memberInfo == null || memberInfo is PropertyInfo)
                return false;

            if (!base.CouldBeGraphField(memberInfo))
                return false;

            // always require explicit attribution from controller action methods
            return memberInfo.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>() != null;
        }

        /// <summary>
        /// When overridden in a child, allows the class to create custom template that inherit from <see cref="MethodGraphFieldTemplate" />
        /// to provide additional functionality or garuntee a certian type structure for all methods on this object template.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>IGraphFieldTemplate.</returns>
        protected override IGraphTypeFieldTemplate CreateMethodFieldTemplate(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return null;

            if (methodInfo.HasAttribute<TypeExtensionAttribute>())
                return new GraphTypeExtensionFieldTemplate(this, methodInfo);
            else
                return new ControllerActionGraphFieldTemplate(this, methodInfo);
        }

        /// <summary>
        /// When overridden in a child, allows the class to create custom templates to provide additional functionality or
        /// guarantee a certian type structure for all properties on this object template.
        /// </summary>
        /// <param name="prop">The property information.</param>
        /// <returns>IGraphFieldTemplate.</returns>
        protected override IGraphTypeFieldTemplate CreatePropertyFieldTemplate(PropertyInfo prop)
        {
            // safety check to ensure properites on controllers can never be parsed as fields
            return null;
        }

        /// <summary>
        /// Gets the actions that have been parsed and defined for the controller.
        /// </summary>
        /// <value>The fields.</value>
        public IEnumerable<IGraphTypeFieldTemplate> Actions =>
            this.FieldTemplates.Values.OfType<ControllerActionGraphFieldTemplate>();

        /// <summary>
        /// Gets operation types to which this object can declare a field.
        /// </summary>
        /// <value>The allowed operation types.</value>
        protected override HashSet<GraphCollection> AllowedGraphCollectionTypes { get; } = new HashSet<GraphCollection>
        {
            GraphCollection.Query,
            GraphCollection.Mutation,
            GraphCollection.Types,
        };

        /// <summary>
        /// Gets an enumeration of the extension methods this controller defines.
        /// </summary>
        /// <value>The extensions.</value>
        public IEnumerable<IGraphTypeFieldTemplate> Extensions =>
            this.FieldTemplates.Values.OfType<GraphTypeExtensionFieldTemplate>();

        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.NONE;
    }
}