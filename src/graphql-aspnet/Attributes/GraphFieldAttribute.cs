// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An attribute used to mark a property as being a graph field that should appear in the object graph.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class GraphFieldAttribute : BaseGraphAttribute
    {
        private TypeExpressions? _typeModifiers;
        private MetaGraphTypes[] _typeWrappers;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAttribute"/> class.
        /// </summary>
        public GraphFieldAttribute()
            : this(false, GraphCollection.Types, Constants.Routing.ACTION_METHOD_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">Name of the field.</param>
        public GraphFieldAttribute(string name)
         : this(false, GraphCollection.Types, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAttribute" /> class.
        /// </summary>
        /// <param name="isRootFragment">if set to <c>true</c> this instance represents a a path from its assigned graph root.</param>
        /// <param name="fieldType">The top level schema type this declared field should be assigned to.</param>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="unionTypeName">Name of the union type if one is to be created, null otherwise.</param>
        /// <param name="typeSet">A collection of types that, depending on other parameters will be used to generate a union
        /// or just setup the field.</param>
        protected GraphFieldAttribute(
            bool isRootFragment,
            GraphCollection fieldType,
            string template,
            string unionTypeName,
            params Type[] typeSet)
            : this(isRootFragment, fieldType, template, typeSet)
        {
            this.UnionTypeName = unionTypeName?.Trim();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAttribute" /> class.
        /// </summary>
        /// <param name="isRootFragment">if set to <c>true</c> this instance represents a a path from its assigned graph root.</param>
        /// <param name="fieldType">The top level schema type this declared field should be assigned to.</param>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="typeSet">A collection of types that, depending on other parameters will be used to generate a union
        /// or just setup the field.</param>
        protected GraphFieldAttribute(
            bool isRootFragment,
            GraphCollection fieldType,
            string template,
            params Type[] typeSet)
        {
            this.IsRootFragment = isRootFragment;
            this.FieldType = fieldType;
            this.Template = template?.Trim();
            this.UnionTypeName = null;
            this.Types = new List<Type>(typeSet?.Where(x => x != null) ?? Enumerable.Empty<Type>());
            this.Complexity = 1;
        }

        /// <summary>
        /// Gets the type of the field being defined.
        /// </summary>
        /// <value>The type of the field.</value>
        public GraphCollection FieldType { get; }

        /// <summary>
        /// Gets the template for the route path for the graph method, if provided.
        /// </summary>
        /// <value>The name.</value>
        public string Template { get; }

        /// <summary>
        /// Gets the name of the union this field defines, if any.
        /// </summary>
        /// <value>The name of the union.</value>
        public string UnionTypeName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance represents a template
        /// pathed from its graph root or if it is intended to be nested with another fragment.
        /// </summary>
        /// <value><c>true</c> if this instance is root fragment; otherwise, <c>false</c>.</value>
        public bool IsRootFragment { get; }

        /// <summary>
        /// Gets the types that were defined by the engineer for this instance.
        /// </summary>
        /// <value>The type of the declared data.</value>
        public IReadOnlyList<Type> Types { get; }

        /// <summary>
        /// Gets or sets an estimated weight value of this field in terms of the overall impact it has on the execution of a query.
        /// See the documentation for an understanding of how query complexity is calculated.
        /// </summary>
        /// <value>The estimated complexity value for this field.</value>
        public float Complexity { get; set; }

        /// <summary>
        /// Gets or sets a short cut value to expression the set of wrappers required to to declare this graph field.
        /// For more complex list/nullability requirements use <see cref="TypeDefinition"/>.
        /// </summary>
        /// <value>The options.</value>
        public TypeExpressions TypeExpression
        {
            get => _typeModifiers ?? TypeExpressions.Auto;
            set
            {
                if (value == TypeExpressions.Auto)
                {
                    _typeModifiers = null;
                    _typeWrappers = null;
                    return;
                }

                // ensure that if the developer declared this shouldnt be a nullable list that the flag for being a list in the
                // first place was also set (the null check doesnt make sense without the list check).
                if (value.HasFlag(TypeExpressions.IsNotNullList) && !value.HasFlag(TypeExpressions.IsList))
                    value |= TypeExpressions.IsList;

                _typeModifiers = value;
                _typeWrappers = value.ToTypeWrapperSet();
            }
        }

        /// <summary>
        /// Gets or sets the actual type wrappers used to generate a type expression for this field.
        /// Setting this value overrides <see cref="TypeExpression"/>. This list represents the type requirements
        /// of the field.
        /// </summary>
        /// <value>The custom wrappers.</value>
        public MetaGraphTypes[] TypeDefinition
        {
            get => _typeWrappers;
            set
            {
                _typeWrappers = value;
                _typeModifiers = null;
            }
        }

        /// <summary>
        /// Gets the mode indicating how the type system should interprete and process the results of this method.
        /// </summary>
        /// <value>The mode.</value>
        public virtual FieldResolutionMode ExecutionMode => FieldResolutionMode.PerSourceItem;
    }
}