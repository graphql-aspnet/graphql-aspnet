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
            public const string UPLOAD_LIST = "UploadList";
        }
    }
}