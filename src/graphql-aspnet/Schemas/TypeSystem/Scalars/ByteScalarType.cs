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
    /// A graph type representing a single unsigned byte.
    /// </summary>
    public sealed class ByteScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static ByteScalarType Instance { get; } = new ByteScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="ByteScalarType"/> class.
        /// </summary>
        static ByteScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteScalarType"/> class.
        /// </summary>
        private ByteScalarType()
            : base(Constants.ScalarNames.BYTE, typeof(byte))
        {
            this.Description = $"A unsigned byte. (Min: {byte.MinValue}, Max: {byte.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(byte?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (byte.TryParse(data.ToString(), out var i))
                return i;

            throw new UnresolvedValueException(data);
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
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}