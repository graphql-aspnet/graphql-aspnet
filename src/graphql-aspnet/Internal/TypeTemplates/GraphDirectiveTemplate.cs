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
    using System.Linq;
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
                if (DirectiveLifeCycleEvents.Instance[methodInfo.Name] != null)
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
            var requiredLifeCycleEvents = DirectiveLifeCycleEvents.Instance[this.Locations]
                .GroupBy(x => x.Phase);

            // at least one method must be declared to handle each required phase
            foreach (var phaseGroup in requiredLifeCycleEvents)
            {
                var canHandlePhase = this.Methods
                    .Any(x => x.LifeCycleEvent.Phase == phaseGroup.Key);

                if (!canHandlePhase)
                {
                    var allowedMethods = string.Join(", ", DirectiveLifeCycleEvents.Instance[phaseGroup.Key]
                        .Select(x => $"'{x.MethodName}'"));

                    var phaseDescription = phaseGroup.Key.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
                    if (string.IsNullOrWhiteSpace(phaseDescription))
                        phaseDescription = "-unknown-";

                    throw new GraphTypeDeclarationException(
                        $"The directive '{this.InternalFullName}' defines a usage location encountered during '{phaseDescription}' " +
                        "but does not declare an appropriate lifecycle method to handle it. " +
                        $"At least one of the following methods is required: {allowedMethods}.");
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