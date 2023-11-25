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
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template describing a controller that will be used to resolve top level fields
    /// on the root operation types.
    /// </summary>
    [DebuggerDisplay("Controller: '{ObjectType.Name}', Route: '{Route.Path}'")]
    public class GraphControllerTemplate : NonLeafGraphTypeTemplateBase, IGraphControllerTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphControllerTemplate"/> class.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        public GraphControllerTemplate(Type controllerType)
            : base(controllerType)
        {
            this.AllowedSchemaItemCollections = new HashSet<SchemaItemPathCollections>
            {
                SchemaItemPathCollections.Query,
                SchemaItemPathCollections.Mutation,
                SchemaItemPathCollections.Subscription,
                SchemaItemPathCollections.Types,
            };
        }

        /// <inheritdoc />
        protected override IEnumerable<IMemberInfoProvider> GatherPossibleFieldTemplates()
        {
            return this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
               .Where(x =>
                    !x.IsAbstract &&
                    !x.IsGenericMethod &&
                    !x.IsSpecialName &&
                    x.DeclaringType != typeof(object) &&
                    x.DeclaringType != typeof(ValueType))
               .Cast<MemberInfo>()
               .Concat(this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
             .Select(x => new MemberInfoProvider(x));
        }

        /// <inheritdoc />
        protected override SchemaItemPath GenerateFieldPath()
        {
            var skipControllerLevelField = this.ObjectType.SingleAttributeOrDefault<GraphRootAttribute>();
            if (skipControllerLevelField != null)
                return SchemaItemPath.Empty;

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

            return new SchemaItemPath(template);
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            // cant use type naming on controllers (they arent real types and arent included directly in the object graph)
            if (this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphTypeAttribute>() != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph controller '{this.ObjectType.FriendlyName()}' defines a {typeof(GraphTypeAttribute).FriendlyName()} but should not. This attribute" +
                    $"is reserved for object types. To alter the naming scheme of a controller use '{nameof(GraphRouteAttribute)}' instead.");
            }

            // ensure that root and a route path aren't defined (its one or the other)
            var skipControllerLevelField = this.AttributeProvider.SingleAttributeOrDefault<GraphRootAttribute>();
            var graphRoute = this.AttributeProvider.SingleAttributeOrDefault<GraphRouteAttribute>();
            if (skipControllerLevelField != null && graphRoute != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph controller '{this.ObjectType.FriendlyName()}' defines a '{typeof(GraphRootAttribute).FriendlyName()}' and " +
                    $"a '{typeof(GraphRouteAttribute).FriendlyName()}'. These options are mutually exclusive for {typeof(GraphController).FriendlyName()}. Choose" +
                    "one or the other.");
            }

            base.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        protected override bool CouldBeGraphField(IMemberInfoProvider fieldProvider)
        {
            if (fieldProvider?.MemberInfo == null || !(fieldProvider.MemberInfo is MethodInfo methodInfo))
                return false;

            if (!base.CouldBeGraphField(fieldProvider))
                return false;

            // always require explicit attribution from controller action methods
            return fieldProvider.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>() != null;
        }

        /// <inheritdoc />
        protected override IGraphFieldTemplate CreateFieldTemplate(IMemberInfoProvider member)
        {
            // safety check to ensure properites on controllers can never be parsed as fields
            if (member?.MemberInfo == null || !(member.MemberInfo is MethodInfo))
                return null;

            if (member.AttributeProvider.HasAttribute<TypeExtensionAttribute>())
                return new GraphTypeExtensionFieldTemplate(this, (MethodInfo)member.MemberInfo, member.AttributeProvider);
            else
                return new ControllerActionGraphFieldTemplate(this, (MethodInfo)member.MemberInfo, member.AttributeProvider);
        }

        /// <summary>
        /// Gets the actions that have been parsed and defined for the controller.
        /// </summary>
        /// <value>The fields.</value>
        public IEnumerable<IGraphFieldTemplate> Actions =>
            this.FieldTemplates.OfType<ControllerActionGraphFieldTemplate>();

        /// <summary>
        /// Gets operation types to which this object can declare a field.
        /// </summary>
        /// <value>The allowed operation types.</value>
        protected override HashSet<SchemaItemPathCollections> AllowedSchemaItemCollections { get; }

        /// <summary>
        /// Gets an enumeration of the extension methods this controller defines.
        /// </summary>
        /// <value>The extensions.</value>
        public IEnumerable<IGraphFieldTemplate> Extensions =>
            this.FieldTemplates.OfType<GraphTypeExtensionFieldTemplate>();

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.CONTROLLER;
    }
}