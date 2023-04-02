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
    using System.Data;

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
            public const string UPLOAD_LIST = "UploadList";
        }

        /// <summary>
        /// Constants defining the names of required form part sections.
        /// </summary>
        public static class Web
        {
            public const string OPERATIONS_FORM_KEY = "operations";
            public const string MAP_FORM_KEY = "map";
        }

        /// <summary>
        /// A collection of keywords defined by the mutli-part specification
        /// defining the segments of an individual query within the "operations" part.
        /// </summary>
        public static class QueryPayloadKeywords
        {
            public const string QUERY_KEY = "query";
            public const string VARIABLE_KEY = "variables";
            public const string OPERATION_KEY = "operation";
        }

        internal class Protected
        {
            public const string FILE_MARKER_PREFIX = "~graphql:aspnet:MultiPartRequestServerExtension:C8BE9950-2124-408F-9502-52840F0088A9";
        }
    }
}