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

        /// <summary>
        /// Parses the template contents according to the rules of the template.
        /// </summary>
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

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public void ValidateOrThrow()
        {
            GraphValidation.EnsureGraphNameOrThrow(this.InternalFullName, this.Name);
        }

        /// <summary>
        /// Gets the name of the argument as it exists in the object graph.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Route.Name;

        /// <summary>
        /// Gets the reflected parameter data that defines this template.
        /// </summary>
        /// <value>The parameter.</value>
        public ParameterInfo Parameter { get; }

        /// <summary>
        /// Gets the return type of this argument as its declared in the C# code base with no modifications or
        /// coerions applied.
        /// </summary>
        /// <value>The type naturally returned by this argument.</value>
        public Type DeclaredArgumentType { get; private set; }

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public string InternalFullName => $"{this.Parent?.InternalFullName}.{this.Parameter.Name}";

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public string InternalName => this.Parameter.Name;

        /// <summary>
        /// Gets a value indicating whether this instance was explictly declared as a graph tiem via acceptable attribution or
        /// if it was parsed as a matter of completeness.
        /// </summary>
        /// <value><c>true</c> if this instance is explictly declared; otherwise, <c>false</c>.</value>
        public bool IsExplicitDeclaration => true;

        /// <summary>
        /// Gets the parent method template this parameter belongs to.
        /// </summary>
        /// <value>The parent.</value>
        public IGraphFieldBaseTemplate Parent { get; }

        /// <summary>
        /// Gets the description of this field in the object graph.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a the canonical path on the graph where this item sits.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; private set; }

        /// <summary>
        /// Gets the default value assigned to this parameter as part of its declaration, if any.
        /// </summary>
        /// <value>The default value.</value>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Gets the type expression that represents how this field is represented in the object graph.
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <summary>
        /// Gets the concrete type this field will return. If this field should return a list of items
        /// this property represents a single item of that list.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this argument represents the resolved data item created
        /// by the resolution of the parent field to this field. If true, this argument will not be available
        /// on the object graph.
        /// </summary>
        /// <value><c>true</c> if this instance is source data field; otherwise, <c>false</c>.</value>
        public GraphArgumentModifiers ArgumentModifiers { get; protected set; }

        /// <summary>
        /// Gets the name of the argument as its declared in the server side code.
        /// </summary>
        /// <value>The name of the declared argument.</value>
        public string DeclaredArgumentName => this.Parameter.Name;

        /// <summary>
        /// Gets a value indicating whether this instance has a defined default value.
        /// </summary>
        /// <value><c>true</c> if this instance has a default value; otherwise, <c>false</c>.</value>
        bool IGraphTypeExpressionDeclaration.HasDefaultValue => this.DefaultValue != null;

        /// <summary>
        /// Gets the actual type wrappers used to generate a type expression for this field.
        /// This list represents the type requirements  of the field.
        /// </summary>
        /// <value>The custom wrappers.</value>
        MetaGraphTypes[] IGraphTypeExpressionDeclaration.TypeWrappers => _fieldDeclaration?.TypeDefinition;

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