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
        /// <param name="directives">The directives to apply to this argument
        /// when its added to a schema.</param>
        public GraphFieldArgument(
            string argumentName,
            GraphTypeExpression typeExpression,
            GraphArgumentModifiers modifiers,
            string parameterName,
            string internalname,
            Type objectType,
            object defaultValue = null,
            string description = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(argumentName, nameof(argumentName));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalname, nameof(internalname));
            this.ParameterName = Validation.ThrowIfNullWhiteSpaceOrReturn(parameterName, nameof(parameterName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.ArgumentModifiers = modifiers;
            this.DefaultValue = defaultValue;
            this.Description = description?.Trim();

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifiers { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public string ParameterName { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}