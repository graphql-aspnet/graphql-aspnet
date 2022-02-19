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
    /// Describes an directive on a <see cref="ISchema"/>, that can be registered
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

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            this.Description = this.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
            this.Route = this.GenerateFieldPath();

            var executableLocations = this.SingleAttributeOrDefault<DirectiveLocationsAttribute>()?.Locations ?? ExecutableDirectiveLocation.AllFieldSelections;
            this.Locations = (DirectiveLocation)executableLocations;

            foreach (var methodInfo in this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (!GraphDirectiveMethodTemplate.IsDirectiveMethod(methodInfo))
                    continue;

                var methodTemplate = new GraphDirectiveMethodTemplate(this, methodInfo);
                methodTemplate.Parse();
                this.Methods.RegisterMethod(methodTemplate);
            }
        }

        /// <summary>
        /// When overridden in a child class, this method builds the unique graph route that will be assigned to this instance
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            var name = GraphTypeNames.ParseName(this.ObjectType, TypeKind.DIRECTIVE);
            return new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Directives, name));
        }

        /// <summary>
        /// Attempts to find a declared graph method that can handle processing of the life cycle and location
        /// requested of the directive.
        /// </summary>
        /// <param name="lifeCycle">The life cycle.</param>
        /// <returns>IGraphMethod.</returns>
        public IGraphMethod FindMethod(DirectiveLifeCycle lifeCycle)
        {
            return this.Methods.FindMethod(lifeCycle);
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (this.Locations == DirectiveLocation.NONE)
            {
                throw new GraphTypeDeclarationException(
                    $"The directive '{this.InternalFullName}' defines no locations to which it can be applied. You must specify at least" +
                    $"one '{typeof(ExecutableDirectiveLocation)}' or remove the {typeof(DirectiveLocationsAttribute).FriendlyName()} attribute.");
            }

            this.Methods.ValidateOrThrow();
        }

        /// <summary>
        /// Creates a resolver capable of completing a resolution of this directive.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        public IGraphDirectiveResolver CreateResolver()
        {
            return new GraphDirectiveActionResolver(this);
        }

        /// <summary>
        /// Gets the methods this directive defines as a map of which methods are invoked for each location this directive services.
        /// </summary>
        /// <value>The methods.</value>
        public GraphDirectiveMethodTemplateContainer Methods { get; }

        /// <summary>
        /// Gets the locations where this directive has been defined for usage.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; private set; }

        /// <summary>
        /// Gets declared type of item minus any asyncronous wrappers (i.e. the T in Task{T}).
        /// </summary>
        /// <value>The type of the declared.</value>
        public Type DeclaredType => this.ObjectType;

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalFullName => this.ObjectType?.FriendlyName(true);

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalName => this.ObjectType?.FriendlyName();

        /// <summary>
        /// Gets a value indicating whether this instance was explictly declared as a graph item via acceptable attribution or
        /// if it was parsed as a matter of completeness.
        /// </summary>
        /// <value><c>true</c> if this instance is explictly declared; otherwise, <c>false</c>.</value>
        public override bool IsExplicitDeclaration => true;

        /// <summary>
        /// Gets the security policies found via defined attributes on the item that need to be enforced.
        /// </summary>
        /// <value>The security policies.</value>
        public override AppliedSecurityPolicyGroup SecurityPolicies { get; } = AppliedSecurityPolicyGroup.Empty;

        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.DIRECTIVE;

        /// <summary>
        /// Gets the argument collection this directive contains.
        /// </summary>
        /// <value>The arguments.</value>
        public IEnumerable<IGraphFieldArgumentTemplate> Arguments => this.Methods.Arguments;
    }
}