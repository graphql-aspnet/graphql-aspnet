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
    /// A graph type representing a single, signed byte.
    /// </summary>
    public sealed class SByteScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static SByteScalarType Instance { get; } = new SByteScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="SByteScalarType"/> class.
        /// </summary>
        static SByteScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SByteScalarType"/> class.
        /// </summary>
        private SByteScalarType()
            : base(Constants.ScalarNames.SIGNED_BYTE, typeof(sbyte))
        {
            this.Description = $"A signed byte. (Min: {sbyte.MinValue}, Max: {sbyte.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(sbyte?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (sbyte.TryParse(data.ToString(), out var i))
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