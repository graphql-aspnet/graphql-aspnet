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

    /// <summary>
    /// Explicitly marks a property or method of a class as being a field on a graph.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class GraphFieldAttribute : GraphAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAttribute"/> class.
        /// </summary>
        public GraphFieldAttribute()
            : this(false, SchemaItemCollections.Types, Constants.Routing.ACTION_METHOD_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">Name of the field.</param>
        public GraphFieldAttribute(string name)
         : this(false, SchemaItemCollections.Types, name)
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
            SchemaItemCollections fieldType,
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
            SchemaItemCollections fieldType,
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
        /// Gets a value indicating which internal subsystem of schema items this
        /// field is a part of.
        /// </summary>
        /// <value>The type of the field.</value>
        public SchemaItemCollections FieldType { get; }

        /// <summary>
        /// Gets the template for the route fragment for the field, if provided.
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
        /// pathed from its root operation or if it is intended to be nested with another fragment.
        /// </summary>
        /// <value><c>true</c> if this instance is root fragment; otherwise, <c>false</c>.</value>
        public bool IsRootFragment { get; }

        /// <summary>
        /// Gets the possible return types that were defined for this field. Useful
        /// when declaring a field that returns an INTERFACE type that may be implemented by
        /// multiple OBJECT types.
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
        /// Gets or sets a type expression, in graphql's syntax language, that defines
        /// the meta types for this field (e.g. <c>"[Type]!"</c>, <c>"Type!"</c> etc.).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The supplied type name is a placeholder; it is replaced at runtime with the
        /// actual type name of this field as its declared in the target schema.
        /// <br />
        /// For Example, <c>"[Type!]"</c>, <c>"[Song!]"</c> and
        /// <c>"[Donut!]"</c> are all equivilant values for this property.
        /// </para>
        /// <para>
        /// When <c>null</c>, the type expression extracted from source code is used.
        /// </para>
        /// </remarks>
        /// <value>The type expression to assign to this field.</value>
        public string TypeExpression { get; set; }

        /// <summary>
        /// Gets the mode indicating how the runtime should process
        /// the objects resolving this field.
        /// </summary>
        /// <value>The mode.</value>
        public virtual FieldResolutionMode ExecutionMode => FieldResolutionMode.PerSourceItem;
    }
}