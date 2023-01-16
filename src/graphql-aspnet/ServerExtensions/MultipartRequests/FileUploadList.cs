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
    public class FileUploadList : IReadOnlyList<FileUpload>
    {
        private readonly List<FileUpload> _files;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadList"/> class.
        /// </summary>
        public FileUploadList()
        {
            _files = new List<FileUpload>();
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<FileUpload> IEnumerable<FileUpload>.GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        /// <inheritdoc />
        public FileUpload this[int index] => _files[index];

        /// <inheritdoc />
        public int Count => _files.Count;
    }
}