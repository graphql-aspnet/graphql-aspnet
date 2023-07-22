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
    /// An interface describing data to fully populate an enumeration item into the object graph.
    /// </summary>
    public interface IEnumValue : ISchemaItem, IDeprecatable
    {
        /// <summary>
        /// Gets the parent enum graph type that owns this value.
        /// </summary>
        /// <value>The parent.</value>
        IEnumGraphType Parent { get; }

        /// <summary>
        /// Gets the declared numerical value of the enum.
        /// </summary>
        /// <value>The value of the neum.</value>
        object DeclaredValue { get; }

        /// <summary>
        /// Gets the internal name assigned to this enum value. Typically this is the same
        /// as <see cref="DeclaredLabel"/> but can be customized by the developer to provider additional
        /// context in error messages and log entries.
        /// </summary>
        /// <value>The assigned internal name of this enum value.</value>
        string InternalName { get; }

        /// <summary>
        /// Gets the declared label applied to the enum value by .NET.
        /// (e.g. 'Value1' for the enum value <c>MyEnum.Value1</c>).
        /// </summary>
        /// <value>The declared label in source code.</value>
        string DeclaredLabel { get; }
    }
}