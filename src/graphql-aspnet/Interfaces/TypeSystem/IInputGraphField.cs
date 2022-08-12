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
    /// <summary>
    /// A field of data on an INPUT_OBJECT graph type.
    /// </summary>
    public interface IInputGraphField : IGraphFieldBase, ITypedSchemaItem
    {
        /// <summary>
        /// Gets a value indicating whether this input field has default value.
        /// </summary>
        /// <value><c>true</c> if this instance has default value; otherwise, <c>false</c>.</value>
        bool HasDefaultValue { get; }
    }
}