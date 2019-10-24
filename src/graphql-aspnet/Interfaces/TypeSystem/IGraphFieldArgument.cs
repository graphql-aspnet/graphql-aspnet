// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An argument/input value that can be applied to a field.
    /// </summary>
    public interface IGraphFieldArgument : ITypedItem, INamedItem
    {
        /// <summary>
        /// Gets a default value to use for any instances of this argument when one is not explicitly provided.
        /// </summary>
        /// <value>The boxed, default value, if any.</value>
        object DefaultValue { get; }

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
    }
}