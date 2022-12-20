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
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Describes an subscription action on a <see cref="GraphController"/>, that can be registered
    /// as a graphql field.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public class ControllerSubscriptionActionGraphFieldTemplate : ControllerActionGraphFieldTemplate
    {
        private Type _explicitlyDeclaredAsSubscriptionSourceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerSubscriptionActionGraphFieldTemplate"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="methodInfo">The method information.</param>
        public ControllerSubscriptionActionGraphFieldTemplate(
            IGraphControllerTemplate parent,
            MethodInfo methodInfo)
            : base(parent, methodInfo)
        {
            this.EventName = null;
        }

        /// <inheritdoc/>
        protected override void ParseTemplateDefinition()
        {
            // before parsing we have to determine the expected source data
            // when an event is raised targeting this field
            // In order, we should check:
            //   1. Is Any field explicitly decorated with [SubscriptionSource]
            //   2. If the return type of this field is not a union, use the object type of the field
            //   3. If the return type is a union, the source is any ol' object (can't deteremine input, up to the user)

            // if any params are explicitly marked as the subscription source data object
            // make sure that type is set as the source  type for this field
            var sourceParam = this.Method.GetParameters().Where(x => x.HasAttribute<SubscriptionSourceAttribute>()).FirstOrDefault();
            if (sourceParam != null)
            {
                _explicitlyDeclaredAsSubscriptionSourceType = sourceParam.ParameterType;
            }

            // perform base parsing
            base.ParseTemplateDefinition();

            // figure out the event name
            var fieldDeclaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            if (fieldDeclaration is SubscriptionAttribute sa)
                this.EventName = sa.EventName;
            else if (fieldDeclaration is SubscriptionRootAttribute sra)
                this.EventName = sra.EventName;

            this.EventName = this.EventName?.Trim();
            if (string.IsNullOrWhiteSpace(this.EventName))
                this.EventName = this.Method.Name;
        }

        /// <inheritdoc/>
        public override void ValidateOrThrow()
        {
            // ensure the custom event name is valid if it was supplied
            if (string.IsNullOrWhiteSpace(this.EventName))
            {
                throw new GraphTypeDeclarationException(
                        $"Invalid subscription action declaration. The method '{this.InternalFullName}' does not " +
                        $"have an event name.");
            }

            if (!Constants.RegExPatterns.NameRegex.IsMatch(this.EventName))
            {
                throw new GraphTypeDeclarationException(
                        $"Invalid subscription action declaration. The method '{this.InternalFullName}' declares " +
                        $"a custom event name of '{this.EventName}'. However, the event name must conform to " +
                        $"standard graphql naming rules. (Regex: {Constants.RegExPatterns.NameRegex} )");
            }

            // ensure that at most one param is decorated as the subscription source
            if (this.Method.GetParameters().Where(x => x.HasAttribute<SubscriptionSourceAttribute>()).Count() > 1)
            {
                throw new GraphTypeDeclarationException(
                        $"Invalid subscription action declaration. The method '{this.InternalFullName}' decorates more" +
                        $"than one parameter  with {typeof(SubscriptionSourceAttribute).FriendlyName()}. At most one parameter " +
                        $"can be attributed with {typeof(SubscriptionSourceAttribute).FriendlyName()}");
            }

            // ensure there is only one param marked as the source object
            var sourceArgument = this.Arguments.SingleOrDefault(x => x.ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
            if (sourceArgument == null)
            {
                throw new GraphTypeDeclarationException(
                        $"Invalid subscription action declaration. The method '{this.InternalFullName}' must " +
                        $"declare 1 (and only 1) parameter of type '{this.SourceObjectType.FriendlyName()}' which will be populated" +
                        "with the source data raised by a subscription event at runtime. Alternately use " +
                        $"{typeof(SubscriptionSourceAttribute).FriendlyName()} to explicitly assign a source data parameter.");
            }

            base.ValidateOrThrow();
        }

        /// <inheritdoc/>
        protected override GraphArgumentTemplate CreateInputArgument(ParameterInfo paramInfo)
        {
            if (this.Route.RootCollection == Execution.GraphCollection.Subscription)
            {
                return new GraphSubscriptionArgumentTemplate(
                    this,
                    paramInfo,
                    _explicitlyDeclaredAsSubscriptionSourceType != null);
            }

            return base.CreateInputArgument(paramInfo);
        }

        /// <inheritdoc />
        public override Type SourceObjectType
        {
            get
            {
                if (_explicitlyDeclaredAsSubscriptionSourceType != null)
                    return _explicitlyDeclaredAsSubscriptionSourceType;

                if (this.UnionProxy != null)
                    return typeof(object);

                return this.ObjectType;
            }
        }

        /// <summary>
        /// Gets the name of the event that was defined on the action for this field.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; private set; }
    }
}