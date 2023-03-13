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
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A custom implementation of a SCALAR representing a file upload.
    /// </summary>
    public class FileUploadListScalarGraphType : ScalarGraphTypeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadListScalarGraphType"/> class.
        /// </summary>
        public FileUploadListScalarGraphType()
            : base(MultipartRequestConstants.ScalarNames.UPLOAD_LIST, typeof(FileUploadList))
        {
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            if (item == null)
                return null;

            return $"{((FileUploadList)item).Files.Count} files";
        }

        /// <inheritdoc />
        public override string SerializeToQueryLanguage(object item)
        {
            if (item == null)
                return Constants.QueryLanguage.NULL;

            throw new NotSupportedException(
                $"The {MultipartRequestConstants.ScalarNames.UPLOAD_LIST} scalar " +
                $"does not support default values that are not null. A value " +
                $"must always be supplied via a variable reference or default to the explicit value <null>.");
        }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Boolean | ScalarValueType.String | ScalarValueType.Number;

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes => TypeCollection.Empty;

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            throw new NotSupportedException($"The {MultipartRequestConstants.ScalarNames.UPLOAD_LIST} scalar does not support direct resolution. It must " +
                $"be used in conjunction with a variable reference per the specification. See documentation for details.");
        }
    }
}