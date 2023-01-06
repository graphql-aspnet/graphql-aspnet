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
        /// Clones this instance to a new argument.
        /// </summary>
        /// <param name="parent">The parent item to assign the newly cloned argument to.</param>
        /// <returns>IGraphField.</returns>
        IGraphArgument Clone(ISchemaItem parent);

        /// <summary>
        /// Gets the argument modifiers that modify how this argument is interpreted by the runtime.
        /// </summary>
        /// <value>The argument modifiers.</value>
        GraphArgumentModifiers ArgumentModifiers { get; }

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