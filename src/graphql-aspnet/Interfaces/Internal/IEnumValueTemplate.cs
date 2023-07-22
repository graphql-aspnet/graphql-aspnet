// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Internal
{
    /// <summary>
    /// An interface describing an ENUM_OPTION templated
    /// from an <see cref="System.Enum"/> value label.
    /// </summary>
    public interface IEnumValueTemplate : ISchemaItemTemplate
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
        /// of the enum option as a string (e.g. "3", "4").
        /// </summary>
        /// <value>The numeric value as string.</value>
        string NumericValueAsString { get;  }

        /// <summary>
        /// Gets the label assigned to this enum value in the source code. (e.g. <c>"Value1"</c> in an enum value named <c>MyEnum.Value1</c>).
        /// </summary>
        /// <value>The declared label.</value>
        string DeclaredLabel { get; }
    }
}