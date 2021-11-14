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
    /// A graph type representing a 32-bit, unsigned integer.
    /// </summary>
    public sealed class UIntScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static UIntScalarType Instance { get; } = new UIntScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="UIntScalarType"/> class.
        /// </summary>
        static UIntScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UIntScalarType"/> class.
        /// </summary>
        private UIntScalarType()
            : base(Constants.ScalarNames.UINT, typeof(uint))
        {
            this.Description = $"A 32-bit, unsigned integer. (Min: {uint.MinValue}, Max: {uint.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(uint?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (uint.TryParse(data.ToString(), out var i))
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