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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Describes an subscription action on a <see cref="GraphController"/>, that can be registered
    /// as a graphql field.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public class ControllerSubscriptionActionGraphFieldTemplate : ControllerActionGraphFieldTemplate
    {
        private Type _explicitlyDeclaredSubscriptionSourceType;

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

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            var fieldDeclaration = this.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            if (fieldDeclaration is SubscriptionAttribute sa)
                EventName = sa.EventName;
            else if (fieldDeclaration is SubscriptionRootAttribute sra)
                EventName = sra.EventName;

            // before parsing we have to determine the expected source data
            // when an event is raised targeting this field
            // In order we should check:
            //   1. Is Any field explicitly decorated with [SubscriptionSource]
            //   2. If the return type of this field is not a union, use the object type of the field
            //   3. If the return type is a union, the source is any ol' object (can't deteremine input, up to the user)

            // if any params are explicitly marked as the subscription source data object
            // make sure that type is set as the source  type for this field
            var sourceParam = this.Method.GetParameters().Where(x => x.HasAttribute<SubscriptionSourceAttribute>()).FirstOrDefault();
            if (sourceParam != null)
            {
                _explicitlyDeclaredSubscriptionSourceType = sourceParam.ParameterType;
            }

            base.ParseTemplateDefinition();
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            // ensure the custom event name is valid if it was supplied
            if (this.EventName != null)
            {
                if (!Constants.RegExPatterns.NameRegex.IsMatch(this.EventName))
                {
                    throw new GraphTypeDeclarationException(
                            $"Invalid subscription action declaration. The method '{this.InternalFullName}' declares " +
                            $"a custom event name of '{this.EventName}'. However, the event name must conform to " +
                            $"standard graphql naming rules. (Regex: {Constants.RegExPatterns.NameRegex} )");
                }
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

        /// <summary>
        /// Creates graph field argument for this template given the parameter info supplied.
        /// </summary>
        /// <param name="paramInfo">The parameter information.</param>
        /// <returns>IGraphFieldArgumentTemplate.</returns>
        protected override GraphFieldArgumentTemplate CreateGraphFieldArgument(ParameterInfo paramInfo)
        {
            if (this.Route.RootCollection == Execution.GraphCollection.Subscription)
            {
                return new GraphSubscriptionFieldArgumentTemplate(
                    this,
                    paramInfo,
                    _explicitlyDeclaredSubscriptionSourceType != null);
            }

            return base.CreateGraphFieldArgument(paramInfo);
        }

        /// <summary>
        /// Gets the name of the event that was defined on the action for this field.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; private set; }

        /// <summary>
        /// Gets the type of the object that owns this field. That is to say the type of source data
        /// given to this field in the object graph. This is usually the Parent's object type but not always; such
        /// is the case with type extensions.
        /// </summary>
        /// <value>The type of the source object.</value>
        public override Type SourceObjectType
        {
            get
            {
                if (_explicitlyDeclaredSubscriptionSourceType != null)
                    return _explicitlyDeclaredSubscriptionSourceType;

                if (this.UnionProxy != null)
                    return typeof(object);

                return this.ObjectType;
            }
        }
    }
}