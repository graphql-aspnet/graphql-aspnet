﻿// *************************************************************
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

            this.AppliedDirectives = new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifiers { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public string ParameterName { get; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}