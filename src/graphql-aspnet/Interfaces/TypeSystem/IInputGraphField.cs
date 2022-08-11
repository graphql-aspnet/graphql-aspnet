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
        /// Gets or sets the default value to use for this field, when not supplied,
        /// in a query. If <c>null</c> and <see cref="HasDefaultValue"/> is true, then the expected default
        /// value is <c>null</c>.
        /// </summary>
        /// <value>The default value assigned to this graph field if not supplied on a query.</value>
        object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this input field has default value.
        /// </summary>
        /// <value><c>true</c> if this instance has default value; otherwise, <c>false</c>.</value>
        bool HasDefaultValue { get; set; }
    }
}