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
    /// A custom SCALAR representing a file uploaded to a mutation or query.
    /// </summary>
    public class FileUploadScalarGraphType : ScalarGraphTypeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadScalarGraphType"/> class.
        /// </summary>
        public FileUploadScalarGraphType()
            : base(MultipartRequestConstants.ScalarNames.UPLOAD, typeof(FileUpload))
        {
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            if (item == null)
                return null;

            return $"{((FileUpload)item).FileName}";
        }

        /// <inheritdoc />
        public override string SerializeToQueryLanguage(object item)
        {
            if (item == null)
                return Constants.QueryLanguage.NULL;

            throw new NotSupportedException(
                $"The {MultipartRequestConstants.ScalarNames.UPLOAD} scalar " +
                $"does not support default values that are not <null>. A value " +
                $"must always be supplied via a variable reference or default to the explicit value <null>.");
        }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Boolean | ScalarValueType.String | ScalarValueType.Number;

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes => TypeCollection.Empty;

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            throw new NotSupportedException($"The {MultipartRequestConstants.ScalarNames.UPLOAD} scalar does not support direct resolution. It must " +
                $"be used in conjunction with a variable reference per the specification. See documentation for details.");
        }
    }
}