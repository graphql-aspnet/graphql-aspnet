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
    /// Extension methods to provide helpful context to the <see cref="MultipartRequestMapHandlingMode"/>
    /// enumeration.
    /// </summary>
    public static class MultipartRequestMapHandlingModeExtensions
    {
        /// <summary>
        /// Determines if the ability to allow string values for a file map argument is allowed. By default,
        /// the specification only allows for arrays as a map argument value. However, there is a strong use case to allow
        /// strings similar to the real <c>object-path</c> npm library.
        /// </summary>
        /// <param name="mode">The mode flags to check.</param>
        /// <returns><c>true</c> if string paths are allowed, <c>false</c> otherwise.</returns>
        public static bool AllowsStringPathValues(this MultipartRequestMapHandlingMode mode)
        {
            return mode.HasFlag(MultipartRequestMapHandlingMode.AllowStringPaths);
        }

        /// <summary>
        /// Determines if arrays of a single string element are treated as a string values and split on a dot.
        /// <para>
        /// When Enabled: <c>["variables.var1"]  =>  ["variables", "var1"]</c>
        /// <br/>
        /// When Disabled: <c>["variables.var1"]  =>  ["variables.var1"]</c>
        /// </para>
        /// </summary>
        /// <param name="mode">The mode flags to check.</param>
        /// <returns><c>true</c> if single element arrays are automatically split via a dot seperator, <c>false</c> otherwise.</returns>
        public static bool ShouldSplitSingleElementArrays(this MultipartRequestMapHandlingMode mode)
        {
            return mode.HasFlag(MultipartRequestMapHandlingMode.SplitDotDelimitedSingleElementArrays);
        }
    }
}