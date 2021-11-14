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
    /// A graph type reprsenting a 64-bit, unsigned integer.
    /// </summary>
    public sealed class ULongScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static ULongScalarType Instance { get; } = new ULongScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="ULongScalarType"/> class.
        /// </summary>
        static ULongScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ULongScalarType"/> class.
        /// </summary>
        private ULongScalarType()
            : base(Constants.ScalarNames.ULONG, typeof(ulong))
        {
            this.Description = $"A 64-bit, unsigned integer. (Min: {ulong.MinValue}, Max: {ulong.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(ulong?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (ulong.TryParse(data.ToString(), out var u))
                return u;

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