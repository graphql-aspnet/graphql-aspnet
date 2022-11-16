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
    /// <summary>
    /// A field of data on an INPUT_OBJECT graph type.
    /// </summary>
    public interface IInputGraphField : IGraphFieldBase, ITypedSchemaItem
    {
        /// <summary>
        /// Gets a value indicating whether this instance was explictly marked as required, meaning it
        /// must be supplied on a query.
        /// </summary>
        /// <value><c>true</c> if this instance is required; otherwise, <c>false</c>.</value>
        bool IsRequired { get; }
    }
}