// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    /// <summary>
    /// Constants pertaining to the <see cref="MultipartRequestServerExtension"/>.
    /// </summary>
    public class MultipartRequestConstants
    {
        /// <summary>
        /// The names of scalars defned by this extension.
        /// </summary>
        public static class ScalarNames
        {
            public const string UPLOAD = "Upload";
        }

        /// <summary>
        /// Constants defining the names of required form-part sections.
        /// </summary>
        public static class Web
        {
            public const string OPERATIONS_FORM_KEY = "operations";
            public const string MAP_FORM_KEY = "map";
        }

        internal class Protected
        {
            /// <summary>
            /// An internal string that uniquely identifies a "string" variable that needs to be
            /// converted to an input file during de-serialziation.
            /// </summary>
            public const string FILE_MARKER_PREFIX = "~graphql:aspnet:MultiPartRequestServerExtension:C8BE9950-2124-408F-9502-52840F0088A9";
        }
    }
}