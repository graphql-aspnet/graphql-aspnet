// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.Interfaces
{
    /// <summary>
    /// An interface describing an ENUM_OPTION templated
    /// from an <see cref="System.Enum"/> value label.
    /// </summary>
    public interface IEnumValueTemplate : IGraphItemTemplate
    {
        /// <summary>
        /// Gets the parent enum template that contains this option.
        /// </summary>
        /// <value>The parent.</value>
        IEnumGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the actual value of this enum option.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; }

        /// <summary>
        /// Gets the numeric value (the underlying number)
        /// of the enum option as a string.
        /// </summary>
        /// <value>The numeric value as string.</value>
        string NumericValueAsString { get;  }
    }
}