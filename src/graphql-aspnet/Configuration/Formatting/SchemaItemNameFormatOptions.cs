// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting
{
    /// <summary>
    /// A set of internally supported name formatting options for various names and values created
    /// for a schema.
    /// </summary>
    public enum TextFormatOptions
    {
        /// <summary>
        /// Text is rendered with a captial first letter (e.g. ItemNumber1, ItemNumber2).
        /// </summary>
        ProperCase,

        /// <summary>
        /// Text is rendered with a lower case first letter (e.g. itemNumber1, itemNumber2).
        /// </summary>
        CamelCase,

        /// <summary>
        /// Text is rendered in all upper case letters (e.g. ITEMNUMBER1, ITEMNUMBER2).
        /// </summary>
        UpperCase,

        /// <summary>
        /// Text is rendered in all lower case letters (e.g. itemnumber1, itemnumber2).
        /// </summary>
        LowerCase,

        /// <summary>
        /// Text is rendered as provided, no text changes are made.
        /// </summary>
        NoChanges,
    }
}