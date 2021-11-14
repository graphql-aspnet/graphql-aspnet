// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Scalars
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a UTF-8 encoded string of characters.
    /// </summary>
    public sealed class StringScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static StringScalarType Instance { get; } = new StringScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="StringScalarType"/> class.
        /// </summary>
        static StringScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringScalarType"/> class.
        /// </summary>
        private StringScalarType()
            : base(Constants.ScalarNames.STRING, typeof(string))
        {
            this.Description = "A UTF-8 encoded string of characters.";
            this.OtherKnownTypes = TypeCollection.Empty;
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            var value = GraphQLStrings.UnescapeAndTrimDelimiters(data, true);
            if (value == null)
            {
                throw new UnresolvedValueException(data);
            }

            return value;
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            return item;
        }

        /// <inheritdoc />
        public override string Description { get; }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}