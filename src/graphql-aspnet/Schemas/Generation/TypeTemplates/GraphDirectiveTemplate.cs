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
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A template describing a directive used on a given schema.
    /// </summary>
    [DebuggerDisplay("Directive Template: {InternalName}")]
    public class GraphDirectiveTemplate : GraphTypeTemplateBase, IGraphDirectiveTemplate
    {
        private AppliedSecurityPolicyGroup _securityPolicies;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="graphDirectiveType">Type of the graph directive being described.</param>
        /// <param name="attributeProvider">The attribute provider that will supply the attributes needed to parse
        /// and configure this template.  <paramref name="graphDirectiveType"/> is used if this parameter
        /// is not supplied.</param>
        public GraphDirectiveTemplate(Type graphDirectiveType, ICustomAttributeProvider attributeProvider = null)
            : base(attributeProvider ?? graphDirectiveType)
        {
            Validation.ThrowIfNotCastable<GraphDirective>(graphDirectiveType, nameof(graphDirectiveType));

            this.Methods = new GraphDirectiveMethodTemplateContainer(this);
            this.ObjectType = graphDirectiveType;
            _securityPolicies = AppliedSecurityPolicyGroup.Empty;
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
            this.IsRepeatable = this.AttributeProvider.SingleAttributeOrDefault<RepeatableAttribute>() != null;

            var routeName = this.DetermineDirectiveName();
            this.Route = new SchemaItemPath(SchemaItemPath.Join(SchemaItemPathCollections.Directives, routeName));
            var potentialMethods = this.GatherPossibleDirectiveExecutionMethods();
            foreach (var methodData in potentialMethods)
            {
                if (this.CouldBeDirectiveExecutionMethod(methodData))
                {
                    var methodTemplate = new GraphDirectiveMethodTemplate(this, methodData.MemberInfo as MethodInfo, methodData.AttributeProvider);
                    methodTemplate.Parse();
                    this.Methods.RegisterMethod(methodTemplate);
                }
            }

            _securityPolicies = AppliedSecurityPolicyGroup.FromAttributeCollection(this.AttributeProvider);
        }

        /// <summary>
        /// Determines the name of the directive. (e.g. <c>@name</c>).
        /// </summary>
        /// <returns>System.String.</returns>
        protected virtual string DetermineDirectiveName()
        {
            return GraphTypeNames.ParseName(this.ObjectType, TypeKind.DIRECTIVE);
        }

        /// <summary>
        /// Inspects the templated object and gathers all the methods with the right attribute declarations such that
        /// they should be come execution methods on the directive instance.
        /// </summary>
        /// <returns>IEnumerable&lt;IMemberInfoProvider&gt;.</returns>
        protected virtual IEnumerable<IMemberInfoProvider> GatherPossibleDirectiveExecutionMethods()
        {
            return this.ObjectType
                   .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                   .Select(x => new MemberInfoProvider(x));
        }

        /// <summary>
        /// Inspects the member data and determines if it can be successfully parsed into a directive action method
        /// for thi template.
        /// </summary>
        /// <param name="memberInfo">The member information to inspect.</param>
        /// <returns><c>true</c> if the member data can be parsed, <c>false</c> otherwise.</returns>
        protected virtual bool CouldBeDirectiveExecutionMethod(IMemberInfoProvider memberInfo)
        {
            return memberInfo?.MemberInfo is MethodInfo &&
                   memberInfo
                    .AttributeProvider
                    .FirstAttributeOfTypeOrDefault<DirectiveLocationsAttribute>() != null;
        }

        /// <inheritdoc />
        public IGraphFieldResolverMetaData FindMetaData(DirectiveLocation location)
        {
            return this.Methods.FindMetaData(location);
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            base.ValidateOrThrow(validateChildren);

            if (this.Locations == DirectiveLocation.NONE)
            {
                throw new GraphTypeDeclarationException(
                    $"The directive '{this.InternalName}' defines no locations to which it can be applied. You must specify at least " +
                    $"one '{typeof(DirectiveLocation)}' via the {typeof(DirectiveLocationsAttribute).FriendlyName()}.");
            }

            if (this.AppliedDirectives.Any())
            {
                throw new GraphTypeDeclarationException(
                    $"The directive {this.InternalName} defines an {nameof(ApplyDirectiveAttribute)}. " +
                    $"Directives cannot have applied directives.");
            }

            if (validateChildren)
                this.Methods.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        public IGraphDirectiveResolver CreateResolver()
        {
            var allMetadata = this.Methods.CreateMetadataCollection();
            return new GraphDirectiveActionResolver(allMetadata);
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
        public override bool IsExplicitDeclaration => true;

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies => _securityPolicies;

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.DIRECTIVE;

        /// <inheritdoc />
        public IEnumerable<IGraphArgumentTemplate> Arguments => this.Methods.Arguments;

        /// <inheritdoc />
        public bool IsRepeatable { get; private set; }
    }
}