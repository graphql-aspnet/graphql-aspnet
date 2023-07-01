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
    using System.Threading;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template describing an argument declared a field.
    /// </summary>
    [DebuggerDisplay("{Name} (Type: {FriendlyObjectTypeName})")]
    public class GraphArgumentTemplate : IGraphArgumentTemplate
    {
        private FromGraphQLAttribute _argDeclaration;
        private bool _invalidTypeExpression;

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
            _argDeclaration = this.Parameter.SingleAttributeOrDefault<FromGraphQLAttribute>();
            string name = null;
            if (_argDeclaration != null)
                name = _argDeclaration?.ArgumentName?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                name = Constants.Routing.PARAMETER_META_NAME;

            name = name.Replace(Constants.Routing.PARAMETER_META_NAME, this.Parameter.Name);
            this.Route = new GraphArgumentFieldPath(this.Parent.Route, name);

            this.Description = this.Parameter.SingleAttributeOrDefault<DescriptionAttribute>()?.Description?.Trim();

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
            this.TypeExpression = this.TypeExpression.CloneTo(GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT));

            // when this argument accepts the same data type as the data returned by its owners target source type
            // i.e. if the source data supplied to the field for resolution is the same as this argument
            // then assume this argument is to contain the source data
            // since the source data will be an OBJECT type (not INPUT_OBJECT) there is no way the user could have supplied it
            if (this.IsSourceDataArgument())
            {
                this.ArgumentModifiers = this.ArgumentModifiers | GraphArgumentModifiers.ParentFieldResult;
            }
            else if (this.IsCancellationTokenArgument())
            {
                this.ArgumentModifiers = this.ArgumentModifiers | GraphArgumentModifiers.CancellationToken;
            }
        }

        /// <summary>
        /// Determines whether this instance represents a parameter that should be marked as the "source data"
        /// for the field its attached to.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        protected virtual bool IsSourceDataArgument()
        {
            if (this.ObjectType == this.Parent.SourceObjectType)
            {
                var sourceType = this.ObjectType;
                if (this.TypeExpression.IsListOfItems)
                {
                    sourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
                }

                if (this.Parent.Arguments.Any(x => x.ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult)))
                    return false;

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
                if (this.Parent.Arguments.All(x => !x.ArgumentModifiers.IsCancellationToken()))
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
            if (this.ArgumentModifiers.IsNotPartOfTheSchema())
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
            GraphValidation.EnsureGraphNameOrThrow(this.InternalFullName, this.Name);

            if (_invalidTypeExpression)
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalFullName}' declares an argument '{this.Name}' that " +
                    $"defines an invalid {nameof(FromGraphQLAttribute.TypeExpression)} (Value = '{_argDeclaration.TypeExpression}'). " +
                    $"The provided type expression must be a valid query language type expression or null.");
            }

            // if the user declared a custom type expression it must be compatiable with the
            // actual expected type expression of the C# code provided
            var actualTypeExpression = GraphTypeExpression
                .FromType(this.DeclaredArgumentType)
                .CloneTo(GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT));

            if (!GraphTypeExpression.AreTypesCompatiable(actualTypeExpression, this.TypeExpression))
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalFullName}' declares an argument '{this.Name}' that " +
                    $"defines a {nameof(FromGraphQLAttribute.TypeExpression)} that is incompatiable with the " +
                    $".NET parameter. (Declared '{this.TypeExpression}' is incompatiable with '{actualTypeExpression}') ");
            }

            if (this.ArgumentModifiers.IsPartOfTheSchema() && this.ObjectType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The item '{this.Parent.InternalFullName}' declares an argument '{this.Name}' of type  '{this.ObjectType.FriendlyName()}' " +
                    $"which is an interface. Interfaces cannot be used as input arguments to any type.");
            }

            foreach (var directive in this.AppliedDirectives)
                directive.ValidateOrThrow();
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
        public string InternalFullName => $"{this.Parent?.InternalFullName}.{this.Parameter.Name}";

        /// <inheritdoc />
        public string InternalName => this.Parameter.Name;

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
        public GraphArgumentModifiers ArgumentModifiers { get; protected set; }

        /// <inheritdoc />
        public string DeclaredArgumentName => this.Parameter.Name;

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