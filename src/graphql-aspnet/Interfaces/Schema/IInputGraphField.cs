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

    /// <summary>
    /// A field of data on an INPUT_OBJECT graph type.
    /// </summary>
    public interface IInputGraphField : IGraphFieldBase, IDefaultValueSchemaItem, ITypedSchemaItem
    {
        /// <summary>
        /// Creates a shallow clone of this instance, replacing specific field values if supplied.
        /// </summary>
        /// <param name="parent">When not null, represents the new parent item that will own this new field.</param>
        /// <param name="fieldName">When not null, represents the new field name to use for the cloned value.</param>
        /// <param name="typeExpression">When not null, represents the new type expression to use
        /// for this field.</param>
        /// <returns>IGraphField.</returns>
        IInputGraphField Clone(ISchemaItem parent = null, string fieldName = null, GraphTypeExpression typeExpression = null);

        /// <summary>
        /// Gets the unaltered name of the property that defines this input field in source code.
        /// </summary>
        /// <value>The property name that generated this data field.</value>
        public string DeclaredName { get; }
    }
}