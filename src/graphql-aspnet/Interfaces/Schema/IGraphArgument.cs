// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An argument/input value that can be applied to a field.
    /// </summary>
    public interface IGraphArgument : ITypedSchemaItem, IDefaultValueSchemaItem, ISchemaItem
    {
        /// <summary>
        /// Creates a shallow clone of this instance, replacing specific argument values if supplied.
        /// </summary>
        /// <param name="parent">When not null, represents the new parent field that will own the new instance.</param>
        /// <param name="argumentName">When not null, represents the new argument name to use for the cloned instance.</param>
        /// <param name="typeExpression">When not null, represents the new type expression to use
        /// for this field.</param>
        /// <param name="defaultValueOptions">A value indicating what to do with field requirements
        /// and default values in the cloned field.</param>
        /// <param name="newDefaultValue">The new default value if so requested to be applied.</param>
        /// <returns>IGraphField.</returns>
        IGraphArgument Clone(
            ISchemaItem parent = null,
            string argumentName = null,
            GraphTypeExpression typeExpression = null,
            DefaultValueCloneOptions defaultValueOptions = DefaultValueCloneOptions.None,
            object newDefaultValue = null);

        /// <summary>
        /// Gets the type expression that represents the data of this argument (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the name of the parameter as it was defined on a concrete method.
        /// </summary>
        /// <value>The name of the parameter.</value>
        string ParameterName { get; }

        /// <summary>
        /// Gets the field that owns this argument.
        /// </summary>
        /// <value>The parent.</value>
        ISchemaItem Parent { get; }
    }
}