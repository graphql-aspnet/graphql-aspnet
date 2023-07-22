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
    using System.Threading;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A template describing an argument declared a field.
    /// </summary>
    [DebuggerDisplay("{Name} (Type: {FriendlyObjectTypeName})")]
    public class GraphArgumentTemplate : IGraphArgumentTemplate
    {
        private FromGraphQLAttribute _argDeclaration;
        private bool _invalidTypeExpression;
        private HashSet<GraphArgumentModifiers> _foundModifiers;
        private GraphSkipAttribute _argSkipDeclaration;
        private GraphSkipAttribute _argTypeSkipDeclaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphArgumentTemplate" /> class.
        /// </summary>
        /// <param name="parent">The owner of this argument.</param>
        /// <param name="parameter">The parameter on which this
        /// argument template is made.</param>
        public GraphArgumentTemplate(IGraphFieldTemplateBase parent, ParameterInfo parameter)
        {
            Validation.ThrowIfNull(parent, nameof(parent));
            Validation.ThrowIfNull(parameter, nameof(parameter));

            _foundModifiers = new HashSet<GraphArgumentModifiers>();

            this.Parent = parent;
            this.Parameter = parameter;
        }

        /// <inheritdoc />
        public virtual void Parse()
        {
            this.DeclaredArgumentType = this.Parameter.ParameterType;
            this.ObjectType = GraphValidation.EliminateWrappersFromCoreType(this.Parameter.ParameterType);
            this.AppliedDirectives = this.ExtractAppliedDirectiveTemplates();

            // set the name
            _argDeclaration = this.AttributeProvider.SingleAttributeOrDefault<FromGraphQLAttribute>();
            string name = null;
            if (_argDeclaration != null)
            {
                name = _argDeclaration?.ArgumentName?.Trim();
                _foundModifiers.Add(GraphArgumentModifiers.ExplicitSchemaItem);
                this.InternalName = _argDeclaration.InternalName;
            }

            if (string.IsNullOrWhiteSpace(this.InternalName))
                this.InternalName = $"{this.Parent?.InternalName}.{this.Parameter.Name}";

            if (string.IsNullOrWhiteSpace(name))
                name = Constants.Routing.PARAMETER_META_NAME;

            // determine if the user has explicitly said this is
            // to be consumed from a DI container
            var fromServicesAttrib = this.Parameter.SingleAttributeOfTypeOrDefault<FromServicesAttribute>();
            if (fromServicesAttrib != null)
                _foundModifiers.Add(GraphArgumentModifiers.ExplicitInjected);

            name = name.Replace(Constants.Routing.PARAMETER_META_NAME, this.Parameter.Name);
            this.Route = new GraphArgumentFieldPath(this.Parent.Route, name);

            this.Description = this.AttributeProvider.SingleAttributeOrDefault<DescriptionAttribute>()?.Description?.Trim();

            if (_argDeclaration?.TypeExpression == null)
            {
                this.DeclaredTypeWrappers = null;
            }
            else
            {
                var expression = GraphTypeExpression.FromDeclaration(_argDeclaration.TypeExpression);
                if (!expression.IsValid)
                {
                    _invalidTypeExpression = true;
                }
                else
                {
                    this.DeclaredTypeWrappers = expression.Wrappers;
                }
            }

            this.HasDefaultValue = this.Parameter.HasDefaultValue;
            this.DefaultValue = null;

            if (this.HasDefaultValue)
            {
                if (this.Parameter.DefaultValue != null)
                {
                    // enums will present their default value as a raw int
                    // convert it to a labelled value
                    if (this.ObjectType.IsEnum)
                    {
                        this.DefaultValue = Enum.ToObject(this.ObjectType, this.Parameter.DefaultValue);
                    }
                    else
                    {
                        this.DefaultValue = this.Parameter.DefaultValue;
                    }
                }
            }

            // set appropriate meta data about this parameter for inclusion in the type system
            this.TypeExpression = GraphTypeExpression.FromType(this.DeclaredArgumentType, this.DeclaredTypeWrappers);
            this.TypeExpression = this.TypeExpression.CloneTo(Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME);

            if (this.IsSourceDataArgument())
                _foundModifiers.Add(GraphArgumentModifiers.ParentFieldResult);

            if (this.IsCancellationTokenArgument())
                _foundModifiers.Add(GraphArgumentModifiers.CancellationToken);

            if (this.IsResolutionContext())
                _foundModifiers.Add(GraphArgumentModifiers.ResolutionContext);

            if (this.IsHttpContext())
                _foundModifiers.Add(GraphArgumentModifiers.HttpContext);

            if (_foundModifiers.Count == 1)
                this.ArgumentModifier = _foundModifiers.First();

            _argSkipDeclaration = this.AttributeProvider.FirstAttributeOfTypeOrDefault<GraphSkipAttribute>();
            _argTypeSkipDeclaration = this.Parameter.ParameterType.FirstAttributeOfTypeOrDefault<GraphSkipAttribute>();
        }

        /// <summary>
        /// Determines whether this instance represents a parameter that should be marked as a "resolution context"
        /// and filled with the active context when possible.
        /// </summary>
        /// <returns><c>true</c> if the ; otherwise, <c>false</c>.</returns>
        public virtual bool IsResolutionContext()
        {
            if (Validation.IsCastable(this.ObjectType, typeof(SchemaItemResolutionContext)))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether this instance represents a parameter that should be marked as a "resolution context"
        /// and filled with the active context when possible.
        /// </summary>
        /// <returns><c>true</c> if the ; otherwise, <c>false</c>.</returns>
        public virtual bool IsHttpContext()
        {
            if (Validation.IsCastable(this.ObjectType, typeof(HttpContext)))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether this instance represents a parameter that should be marked as the "source data"
        /// for the field its attached to.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        protected virtual bool IsSourceDataArgument()
        {
            // there can only ever be one source argument
            if (this.Parent.Arguments.Any(x => x.ArgumentModifier.IsSourceParameter()))
                return false;

            if (_foundModifiers.Count > 0)
                return false;

            // when this argument accepts the same data type as the data returned by its owner's resolver
            // i.e. if the source data supplied to the field for resolution is the same as this parameter
            // then assume this argument is to contain the source data
            // since the source data will be an OBJECT type (not INPUT_OBJECT)
            // there is no way the user could have supplied it
            if (this.ObjectType == this.Parent.SourceObjectType)
            {
                var sourceType = this.ObjectType;
                if (this.TypeExpression.IsListOfItems)
                {
                    sourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
                }

                return sourceType == this.DeclaredArgumentType;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this argument is (or can be) the cancellation token for this
        /// argument's parent field.
        /// </summary>
        /// <returns><c>true</c> if this argument is the dedicated cancellation token; otherwise, <c>false</c>.</returns>
        protected virtual bool IsCancellationTokenArgument()
        {
            if (this.ObjectType == typeof(CancellationToken))
            {
                // only one captured cancel token allowed
                if (this.Parent.Arguments.All(x => !x.ArgumentModifier.IsCancellationToken()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the concrete types that this instance may return or make use of in a graph query.
        /// </summary>
        /// <returns>IEnumerable&lt;Type&gt;.</returns>
        public IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            if (!this.ArgumentModifier.CouldBePartOfTheSchema())
            {
                // internal parameters should not be injected into the object graph
                // so they have no dependents
                return Enumerable.Empty<DependentType>();
            }

            var types = new List<DependentType>();
            var expectedTypeKind = GraphValidation.ResolveTypeKind(this.ObjectType, TypeKind.INPUT_OBJECT);
            types.Add(new DependentType(this.ObjectType, expectedTypeKind));

            if (this.AppliedDirectives != null)
            {
                var directiveTypes = this.AppliedDirectives
                    .Where(x => x.DirectiveType != null)
                    .Select(x => new DependentType(x.DirectiveType, TypeKind.DIRECTIVE));

                types.AddRange(directiveTypes);
            }

            return types;
        }

        /// <inheritdoc />
        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(this.InternalName))
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalName}' declares a parameter '{this.Parameter.Name}' " +
                    "that did not not declare a valid internal name. Templating cannot continue.");
            }

            GraphValidation.EnsureGraphNameOrThrow(this.InternalName, this.Name);

            if (_invalidTypeExpression)
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalName}' declares a parameter '{this.Name}' that " +
                    $"defines an invalid {nameof(FromGraphQLAttribute.TypeExpression)} (Value = '{_argDeclaration.TypeExpression}'). " +
                    $"The provided type expression must be a valid query language type expression or null.");
            }

            // if the user declared a custom type expression it must be compatiable with the
            // actual expected type expression of the C# code provided
            var actualTypeExpression = GraphTypeExpression
                .FromType(this.DeclaredArgumentType)
                .CloneTo(GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT));

            if (!GraphTypeExpression.AreTypesCompatiable(actualTypeExpression, this.TypeExpression, false))
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalName}' declares a parameter '{this.Name}' that " +
                    $"defines a {nameof(FromGraphQLAttribute.TypeExpression)} that is incompatiable with the " +
                    $".NET parameter. (Declared '{this.TypeExpression}' is incompatiable with '{actualTypeExpression}') ");
            }

            // the most common scenario, throw an exception with explicit text
            // on how to fix it
            if (_foundModifiers.Contains(GraphArgumentModifiers.ExplicitInjected)
                && _foundModifiers.Contains(GraphArgumentModifiers.ExplicitSchemaItem))
            {
                throw new GraphTypeDeclarationException(
                       $"The item '{this.Parent.InternalName}' declares a parameter '{this.Name}' that " +
                       $"is defined to be supplied from a graphql query AND from a DI services container. " +
                       $"An argument can not be supplied from a graphql query and from a DI container. If declaring argument attributes, supply " +
                       $"{nameof(FromGraphQLAttribute)} or {nameof(FromServicesAttribute)}, but not both.");
            }

            if (_foundModifiers.Count > 1)
            {
                var flags = string.Join(", ", _foundModifiers);
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalName}' declares a parameter '{this.Name}' that " +
                    $"declares more than one behavior modification flag. Each parameter must declare only one " +
                    $"behavioral role within a given resolver method. Flags Declared: {flags}");
            }

            if (_argSkipDeclaration != null && this.ArgumentModifier.CouldBePartOfTheSchema())
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalName}' contains a parameter '{this.Name}' that " +
                    $"declares the {nameof(GraphSkipAttribute)}. However, this argument may be included in the schema in some scenarios. " +
                    $"If this argument is intended to be served from a service provider try adding {typeof(FromServicesAttribute)} to its declaration.");
            }

            if (_argTypeSkipDeclaration != null && this.ArgumentModifier.CouldBePartOfTheSchema())
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalName}' contains a parameter '{this.Name}' that " +
                    $"is of type {this.Parameter.ParameterType.FriendlyName()} . This type declares the {nameof(GraphSkipAttribute)} and is " +
                    $"not allowed to appear in any schema but is currently being interpreted as an INPUT_OBJECT. If the parameter value is intended to be served " +
                    $"from a service provider try adding {typeof(FromServicesAttribute)} to its declaration.");
            }

            foreach (var directive in this.AppliedDirectives)
                directive.ValidateOrThrow();
        }

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaData CreateResolverMetaData()
        {
            var isValidList = this.TypeExpression.IsListOfItems;
            if (!isValidList && this.ArgumentModifier.IsSourceParameter())
            {
                if (this.Parent is IGraphFieldTemplate gft)
                    isValidList = gft.Mode == FieldResolutionMode.Batch;
            }

            return new FieldResolverParameterMetaData(
                this.Parameter,
                this.InternalName,
                this.Parent.InternalName,
                this.ArgumentModifier,
                isValidList,
                this.HasDefaultValue,
                this.DefaultValue);
        }

        /// <inheritdoc />
        public string Name => this.Route.Name;

        /// <summary>
        /// Gets the reflected parameter data that defines this template.
        /// </summary>
        /// <value>The parameter.</value>
        public ParameterInfo Parameter { get; }

        /// <inheritdoc />
        public Type DeclaredArgumentType { get; private set; }

        /// <inheritdoc />
        public string InternalName { get; private set; }

        /// <inheritdoc />
        public bool IsExplicitDeclaration => true;

        /// <inheritdoc />
        public IGraphFieldTemplateBase Parent { get; }

        /// <inheritdoc />
        public string Description { get; private set; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; private set; }

        /// <inheritdoc />
        public object DefaultValue { get; private set; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <inheritdoc />
        public Type ObjectType { get; private set; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifier { get; protected set; }

        /// <inheritdoc />
        public string ParameterName => this.Parameter.Name;

        /// <inheritdoc />
        public MetaGraphTypes[] DeclaredTypeWrappers { get; private set; }

        /// <inheritdoc />
        public ICustomAttributeProvider AttributeProvider => this.Parameter;

        /// <inheritdoc />
        public IEnumerable<IAppliedDirectiveTemplate> AppliedDirectives { get; private set; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; private set; }

        /// <summary>
        /// Gets a string representing the name of the parameter's concrete type.
        /// This is an an internal helper property for helpful debugging information only.
        /// </summary>
        /// <value>The name of the parameter type friendly.</value>
        public string FriendlyObjectTypeName => this.Parameter.ParameterType.FriendlyName();
    }
}