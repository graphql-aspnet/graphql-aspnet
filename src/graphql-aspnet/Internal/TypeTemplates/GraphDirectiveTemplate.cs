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
        public override IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            var list = new List<DependentType>();
            list.AddRange(this.Methods.RetrieveRequiredTypes());

            return list;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            this.Description = this.AttributeProvider.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;

            var routeName = GraphTypeNames.ParseName(this.ObjectType, TypeKind.DIRECTIVE);
            this.Route = new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Directives, routeName));

            var phases = DirectiveInvocationPhase.DefaultPhases;
            var phaseAttrib = this.AttributeProvider.SingleAttributeOrDefault<DirectiveInvocationAttribute>();
            if (phaseAttrib != null)
                phases = phaseAttrib.Phases;

            this.InvocationPhases = phases;

            foreach (var methodInfo in this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (methodInfo.FirstAttributeOfTypeOrDefault<DirectiveLocationsAttribute>() != null)
                {
                    var methodTemplate = new GraphDirectiveMethodTemplate(this, methodInfo);
                    methodTemplate.Parse();
                    this.Methods.RegisterMethod(methodTemplate);
                }
            }
        }

        /// <inheritdoc />
        public IGraphMethod FindMethod(DirectiveLocation location)
        {
            return this.Methods.FindMethod(location);
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
        public DirectiveLocation Locations => this.Methods.Locations;

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
        public IEnumerable<IGraphArgumentTemplate> Arguments => this.Methods.Arguments;

        /// <inheritdoc />
        public DirectiveInvocationPhase InvocationPhases { get; private set;  }
    }
}