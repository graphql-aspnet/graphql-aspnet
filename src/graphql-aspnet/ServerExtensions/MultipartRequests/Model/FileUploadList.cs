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
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// An object representing the an arbitrary collection of files uploaded to a mutation or query.
    /// </summary>
    /// <remarks>
    /// Attempting to use this class without registering the <see cref="MultipartRequestServerExtension"/>
    /// will result in a schema failure. This class cannot be used as a regular INPUT_OBJECT. See documentation
    /// for details.
    /// </remarks>
    [GraphSkip]
    [GraphType(PreventAutoInclusion = true)]
    public class FileUploadList
    {
        private readonly List<FileUpload> _files;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadList"/> class.
        /// </summary>
        public FileUploadList()
        {
            _files = new List<FileUpload>();
        }

        /// <summary>
        /// Gets the files found on this instance.
        /// </summary>
        /// <value>The collection of files assembled from the query.</value>
        [GraphSkip]
        public IReadOnlyList<FileUpload> Files => _files;
    }
}