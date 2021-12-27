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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// Describes a directive on a <see cref="ISchema"/>, that can be registered
    /// and executed via an instruction from a query document.
    /// </summary>
    [DebuggerDisplay("Directive Template: {InternalName}")]
    public class GraphDirectiveTemplate : BaseGraphTypeTemplate, IGraphDirectiveTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveTemplate"/> class.
        /// </summary>
        /// <param name="graphDirectiveType">Type of the graph directive being described.</param>
        public GraphDirectiveTemplate(Type graphDirectiveType)
            : base(graphDirectiveType)
        {
            Validation.ThrowIfNotCastable<GraphDirective>(graphDirectiveType, nameof(graphDirectiveType));

            this.Methods = new GraphDirectiveMethodTemplateContainer(this);
            this.ObjectType = graphDirectiveType;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            this.Description = this.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
            this.Route = this.GenerateFieldPath();

            // extract all the allowed locations
            // where this directive can be applied
            var locationAttributes = this.RetrieveAttributes(x => x is DirectiveLocationsAttribute);
            var allowedLocations = DirectiveLocation.NONE;

            foreach (DirectiveLocationsAttribute attrib in locationAttributes)
                allowedLocations = allowedLocations | attrib.Locations;

            this.Locations = allowedLocations;

            foreach (var methodInfo in this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // only pay attention to those with valid method names
                if (Constants.ReservedNames.DirectiveLifeCycleMethodNames.ContainsKey(methodInfo.Name))
                {
                    var methodTemplate = new GraphDirectiveMethodTemplate(this, methodInfo);
                    methodTemplate.Parse();
                    this.Methods.RegisterMethod(methodTemplate);
                }
            }
        }

        /// <inheritdoc />
        protected override GraphFieldPath GenerateFieldPath()
        {
            var name = GraphTypeNames.ParseName(this.ObjectType, TypeKind.DIRECTIVE);
            return new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Directives, name));
        }

        /// <inheritdoc />
        public IGraphMethod FindMethod(DirectiveLifeCycleEvent lifeCycle)
        {
            return this.Methods.FindMethod(lifeCycle);
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (this.Locations == DirectiveLocation.NONE)
            {
                throw new GraphTypeDeclarationException(
                    $"The directive '{this.InternalFullName}' defines no locations to which it can be applied. You must specify at least " +
                    $"one '{typeof(DirectiveLocation)}' via the {typeof(DirectiveLocationsAttribute).FriendlyName()}.");
            }

            this.Methods.ValidateOrThrow();

            this.EnsureLifeCycleEventRepresentationOrThrow();
        }

        /// <summary>
        /// Checks each defined location where this directive may be used against all the defined methods to ensure that at least
        /// one method was declared that can process data at each location where it may appear.
        /// </summary>
        private void EnsureLifeCycleEventRepresentationOrThrow()
        {
            var allowedLocations = this.Locations.GetIndividualFlags<DirectiveLocation>();

            foreach (DirectiveLocation location in allowedLocations)
            {
                var lifeCycleEvent = location.SingleAttributeOrDefault<DirectiveLifeCycleEventAttribute>()?.LifeCycleEvent;
                if (!lifeCycleEvent.HasValue && lifeCycleEvent.Value == DirectiveLifeCycleEvent.Unknown)
                    continue;

                var individualEvents = lifeCycleEvent.Value.GetIndividualFlags<DirectiveLifeCycleEvent>();

                var canHandle = false;
                var failedEvents = new List<DirectiveLifeCycleEvent>();
                foreach (DirectiveLifeCycleEvent evt in individualEvents)
                {
                    canHandle = this.LifeCycleEvents.HasFlag(evt);
                    if (!canHandle)
                        failedEvents.Add(evt);
                    else
                        break;
                }

                // TODO: Fix this so that all missing events can be reported at once.
                if (!canHandle)
                {
                    switch (failedEvents[0])
                    {
                        case DirectiveLifeCycleEvent.AlterTypeSystem:
                            throw new GraphTypeDeclarationException(
                                $"The directive '{this.InternalFullName}' defines a usage location of '{location}' " +
                                $"but does not declare the '{Constants.ReservedNames.DIRECTIVE_ALTER_TYPE_SYSTEM_METHOD_NAME}' lifecycle method to handle it." +
                                $"Either declare the appropriate method or remove the location.");

                        case DirectiveLifeCycleEvent.BeforeResolution:
                        case DirectiveLifeCycleEvent.AfterResolution:
                            throw new GraphTypeDeclarationException(
                                $"The directive '{this.InternalFullName}' defines a usage location of '{location}' " +
                                $"but does not declare an appropriate life cycle method " +
                                $"(e.g. '{Constants.ReservedNames.DIRECTIVE_BEFORE_RESOLUTION_METHOD_NAME}', '{Constants.ReservedNames.DIRECTIVE_AFTER_RESOLUTION_METHOD_NAME}') " +
                                $"to handle it. Either declare one of the appropriate methods or remove the location.");

                        default:
                            throw new GraphTypeDeclarationException(
                                $"The directive '{this.InternalFullName}' defines a usage location of '{location}' " +
                                $"but does not declare an appropriate life cycle method " +
                                $"to handle it. Either declare the one of the appropriate methods or remove the location.");
                    }
                }
            }
        }

        /// <inheritdoc />
        public IGraphDirectiveResolver CreateResolver()
        {
            return new GraphDirectiveActionResolver(this);
        }

        /// <summary>
        /// Gets the methods this directive defines as a map of which methods are invoked for each location this directive services.
        /// </summary>
        /// <value>The methods.</value>
        public GraphDirectiveMethodTemplateContainer Methods { get; }

        /// <inheritdoc />
        public DirectiveLocation Locations { get; private set; }

        /// <inheritdoc />
        public DirectiveLifeCycleEvent LifeCycleEvents => this.Methods.LifeCycleEvents;

        /// <summary>
        /// Gets declared type of item minus any asyncronous wrappers (i.e. the T in Task{T}).
        /// </summary>
        /// <value>The type of the declared.</value>
        public Type DeclaredType => this.ObjectType;

        /// <inheritdoc />
        public override string InternalFullName => this.ObjectType?.FriendlyName(true);

        /// <inheritdoc />
        public override string InternalName => this.ObjectType?.FriendlyName();

        /// <inheritdoc />
        public override bool IsExplicitDeclaration => true;

        /// <inheritdoc />
        public override FieldSecurityGroup SecurityPolicies { get; } = FieldSecurityGroup.Empty;

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.DIRECTIVE;

        /// <inheritdoc />
        public IEnumerable<IGraphFieldArgumentTemplate> Arguments => this.Methods.ExecutionArguments;
    }
}