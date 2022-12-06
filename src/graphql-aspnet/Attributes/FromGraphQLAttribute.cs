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
        /// <summary>
        /// Initializes a new instance of the <see cref="FromGraphQLAttribute"/> class.
        /// </summary>
        public FromGraphQLAttribute()
            : this(Constants.Routing.PARAMETER_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromGraphQLAttribute"/> class.
        /// </summary>
        /// <param name="argumentName">The name of the input argument to extract the value from.</param>
        public FromGraphQLAttribute(string argumentName)
        {
            this.ArgumentName = argumentName?.Trim();
        }

        /// <summary>
        /// Gets the argument name as it should be declared in a graphql document.
        /// </summary>
        /// <value>The template.</value>
        public string ArgumentName { get; }

        /// <summary>
        /// Gets or sets a type expression, in graphql's syntax language, that defines
        /// the meta types for this argument (e.g. <c>"[Type]!"</c>, <c>"Type!"</c> etc.).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The supplied type name is a placeholder; it is replaced at runtime with the
        /// actual type name of this argument as its declared in the target schema.
        /// <br />
        /// For Example, <c>"[Type!]"</c>, <c>"[Song!]"</c> and
        /// <c>"[Donut!]"</c> are all equivilant values for this property. (Default: <c>null</c>).
        /// </para>
        /// <para>
        /// When <c>null</c>, the type expression extracted from source code is used.
        /// </para>
        /// </remarks>
        /// <value>The type expression to assign to this argument.</value>
        public string TypeExpression { get; set; }
    }
}