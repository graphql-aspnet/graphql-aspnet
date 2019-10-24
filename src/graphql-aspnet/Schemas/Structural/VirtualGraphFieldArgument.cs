// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A field argument not based on a <see cref="ParameterInfo"/> reference from a method or property.
    /// </summary>
    [DebuggerDisplay("Virtual Argument: {Name}")]
    public class VirtualGraphFieldArgument : IGraphFieldArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGraphFieldArgument" /> class.
        /// </summary>
        /// <param name="name">The name of this field in the object graph.</param>
        /// <param name="internalName">The name of this field as it exists in the .NET code.</param>
        /// <param name="typeExpression">The graph type expression representing this field.</param>
        /// <param name="concreteType">The concrete graph type in the server code that this argument is mapped to.</param>
        /// <param name="argModifiers">The argument modifiers.</param>
        /// <param name="defaultValue">The default value.</param>
        public VirtualGraphFieldArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType,
            GraphArgumentModifiers argModifiers = GraphArgumentModifiers.None,
            object defaultValue = null)
        {
            this.ObjectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.ParameterName = this.Name;
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ArgumentModifiers = argModifiers;
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the type of the object this graph type was made from.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets a default value to use for any instances of this argument when one is not explicitly provided.
        /// </summary>
        /// <value>The boxed, default value, if any.</value>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets the argument modifiers that modify how this argument is interpreted by the runtime.
        /// </summary>
        /// <value>The argument modifiers.</value>
        public GraphArgumentModifiers ArgumentModifiers { get; }

        /// <summary>
        /// Gets the type expression that represents the data of this argument (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the name of the parameter as it was defined on a concrete method.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string ParameterName { get; }
    }
}