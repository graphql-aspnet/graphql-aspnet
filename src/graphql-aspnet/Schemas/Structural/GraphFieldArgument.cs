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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An argument defined on the object graph but not tied to any concrete item. It exists as a result
    /// of a programatic delcaration. The parameters of action methods on <see cref="GraphController"/> are generally mapped
    /// into a <see cref="GraphFieldArgument"/> for purposes of mapping, data coersion and introspection.
    /// </summary>
    [DebuggerDisplay("Argument: {Name}")]
    public class GraphFieldArgument : IGraphFieldArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldArgument" /> class.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="typeExpression">The type expression.</param>
        /// <param name="modifiers">The modifiers.</param>
        /// <param name="parameterName">Name of the parameter as it is declared in the source code.</param>
        /// <param name="internalname">The fully qualified internal name identifiying this argument.</param>
        /// <param name="objectType">The concrete type of the object representing this argument.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="description">The description about this argument.</param>
        public GraphFieldArgument(
            string argumentName,
            GraphTypeExpression typeExpression,
            GraphArgumentModifiers modifiers,
            string parameterName,
            string internalname,
            Type objectType,
            object defaultValue = null,
            string description = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(argumentName, nameof(argumentName));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalname, nameof(internalname));
            this.ParameterName = Validation.ThrowIfNullWhiteSpaceOrReturn(parameterName, nameof(parameterName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.ArgumentModifiers = modifiers;
            this.DefaultValue = defaultValue;
            this.Description = description?.Trim();
        }

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
        /// Gets the argument modifiers that alter the behavior of this argument at execution time.
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
        /// Gets a value to use for any instances of this argument when one is not explicitly provided.
        /// </summary>
        /// <value>The boxed, default value, if any.</value>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string ParameterName { get; }

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
    }
}