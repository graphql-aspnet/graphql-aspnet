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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A marker class for parameters of a method to configure them to respond to
    /// different parameters on a query.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromGraphQLAttribute : BaseGraphAttribute
    {
        private MetaGraphTypes[] _typeWrappers;
        private TypeExpressions? _typeModifiers;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromGraphQLAttribute"/> class.
        /// </summary>/// <param name="typeExpression">The type expression to apply to this value.</param>
        public FromGraphQLAttribute(TypeExpressions typeExpression)
            : this(Constants.Routing.PARAMETER_META_NAME, typeExpression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromGraphQLAttribute"/> class.
        /// </summary>
        /// <param name="argumentName">The name of the input argument to extract the value from.</param>
        public FromGraphQLAttribute(string argumentName)
            : this(argumentName, TypeExpressions.Auto)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromGraphQLAttribute"/> class.
        /// </summary>
        /// <param name="argumentName">The name of the input argument to extract the value from.</param>
        /// <param name="typeExpression">The type expression to apply to this value.</param>
        public FromGraphQLAttribute(string argumentName, TypeExpressions typeExpression)
        {
            this.ArgumentName = argumentName?.Trim();
            this.TypeExpression = typeExpression;
        }

        /// <summary>
        /// Gets the argument name as it should be declared in a graphql document.
        /// </summary>
        /// <value>The template.</value>
        public string ArgumentName { get; }

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
                _typeModifiers = TypeExpressions.Auto;
            }
        }
    }
}