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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A fully qualified description of a single input parameter to a <see cref="ControllerActionGraphFieldTemplate"/>.
    /// This template can be added as an input argument to a field.
    /// </summary>
    [DebuggerDisplay("{Name} (Type: {FriendlyObjectTypeName})")]
    public class GraphFieldArgumentTemplate : IGraphFieldArgumentTemplate
    {
        private FromGraphQLAttribute _fieldDeclaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldArgumentTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="parameter">The parameter.</param>
        public GraphFieldArgumentTemplate(IGraphFieldBaseTemplate parent, ParameterInfo parameter)
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

            // set the name
            _fieldDeclaration = this.Parameter.SingleAttributeOrDefault<FromGraphQLAttribute>();
            string name = null;
            if (_fieldDeclaration != null)
            {
                name = _fieldDeclaration?.ArgumentName?.Trim();
            }

            if (string.IsNullOrWhiteSpace(name))
                name = Constants.Routing.PARAMETER_META_NAME;

            name = name.Replace(Constants.Routing.PARAMETER_META_NAME, this.Parameter.Name);
            this.Route = new GraphArgumentFieldPath(this.Parent.Route, name);

            this.Description = this.Parameter.SingleAttributeOrDefault<DescriptionAttribute>()?.Description?.Trim();

            if (this.Parameter.HasDefaultValue && this.Parameter.DefaultValue != null)
            {
                // enums will present their default value as a raw int
                if (this.ObjectType.IsEnum)
                {
                    this.DefaultValue = Enum.ToObject(this.ObjectType, this.Parameter.DefaultValue);
                }
                else
                {
                    this.DefaultValue = this.Parameter.DefaultValue;
                }
            }

            // set appropriate meta data about this parameter for inclusion in the type system
            this.TypeExpression = GraphValidation.GenerateTypeExpression(this.DeclaredArgumentType, this);
            this.TypeExpression = this.TypeExpression.CloneTo(GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT));

            // when this argument accepts the same data type as the data returned by its owners target source type
            // i.e. if the source data supplied to the field for resolution is the same as this argument
            // then assume this argument is to contain the source data
            // since the source data will be an OBJECT type (not INPUT_OBJECT) there is no way the user could have supplied it
            if (this.IsSourceDataArgument())
            {
                this.ArgumentModifiers = this.ArgumentModifiers
                                         | GraphArgumentModifiers.ParentFieldResult
                                         | GraphArgumentModifiers.Internal;
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
        /// Retrieves the concrete types that this instance may return or make use of in a graph query.
        /// </summary>
        /// <returns>IEnumerable&lt;Type&gt;.</returns>
        public IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            if (this.ArgumentModifiers.IsSourceParameter())
            {
                // source parameters should not be injected into the object graph
                // so they have no dependents
                return Enumerable.Empty<DependentType>();
            }
            else
            {
                var expectedTypeKind = GraphValidation.ResolveTypeKind(this.ObjectType, TypeKind.INPUT_OBJECT);
                return new DependentType(this.ObjectType, expectedTypeKind).AsEnumerable();
            }
        }

        /// <inheritdoc />
        public void ValidateOrThrow()
        {
            GraphValidation.EnsureGraphNameOrThrow(this.InternalFullName, this.Name);
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
        public IGraphFieldBaseTemplate Parent { get; }

        /// <inheritdoc />
        public string Description { get; private set; }

        /// <inheritdoc />
        public GraphFieldPath Route { get; private set; }

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
        public bool HasDefaultValue => this.DefaultValue != null;

        /// <inheritdoc />
        public MetaGraphTypes[] TypeWrappers => _fieldDeclaration?.TypeDefinition;

#if DEBUG
        /// <summary>
        /// Gets a string representing the name of the parameter's concrete type.
        /// This is an an internal helper property for helpful debugging information only.
        /// </summary>
        /// <value>The name of the parameter type friendly.</value>
        public string FriendlyObjectTypeName => this.Parameter.ParameterType.FriendlyName();
#endif
    }
}