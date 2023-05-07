// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration
{
    using System;

    /// <summary>
    /// A set of modes indicating how the multipart request extension will handle values passed on
    /// the 'map' field.
    /// </summary>
    [Flags]
    public enum MultipartRequestMapHandlingMode
    {
        /// <summary>
        /// When declared alone, allows for no extended options.
        /// </summary>
        None = 0,

        /// <summary>
        /// When enabled, indicates that the map field will allow a dot-pathed string for a query path as opposed to
        /// requiring an array of items.
        /// <para>
        /// When Enabled: <c> "variables.var1" =>  ["variables", "var1"]</c>
        /// <br/>
        /// When Disabled:<c> "variables.var" =>  ~400: Bad Request~</c>
        /// </para>
        /// <para>
        /// When not enabled, non-array based paths will be rejected.</para>
        /// </summary>
        AllowStringPaths = 1,

        /// <summary>
        /// When enabled, indicates that the map field will treat single element arrays as a dot pathed string
        /// rather than as a reference to a single property.
        /// <para>
        /// When Enabled: <c>["variables.var1"]  =>  ["variables", "var1"]</c>
        /// <br/>
        /// When Disabled: <c>["variables.var1"]  =>  ["variables.var1"]</c>
        /// </para>
        /// </summary>
        SplitDotDelimitedSingleElementArrays = 2,

        /// <summary>
        /// Allows for string based query paths and splitting of single element array values.
        /// </summary>
        Default = AllowStringPaths | SplitDotDelimitedSingleElementArrays,
    }
}