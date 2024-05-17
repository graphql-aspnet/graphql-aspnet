// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting
{
    /// <summary>
    /// The different categories of entities that support
    /// distinct name formatting within a schema.
    /// </summary>
    public enum NameFormatCategory
    {
        /// <summary>
        /// The string is being formatted as the name of a field, be it a field on an INPUT_OBJECT, INTERFACE
        /// or OBJECT type.
        /// </summary>
        Field,

        /// <summary>
        /// The string is being formatted as the name of a formal graph type in the schema.
        /// </summary>
        GraphType,

        /// <summary>
        /// The string is being formatted as a single enum value label within an ENUM graph type.
        /// </summary>
        EnumValue,

        /// <summary>
        /// The string is being formatted as the name of a directive used in a query without the preceeding '@'
        /// symbol.
        /// </summary>
        Directive,
    }
}
