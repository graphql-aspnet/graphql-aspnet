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
    /// A graph type reprsenting a 64-bit integer.
    /// </summary>
    public sealed class LongScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static LongScalarType Instance { get; } = new LongScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="LongScalarType"/> class.
        /// </summary>
        static LongScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongScalarType"/> class.
        /// </summary>
        private LongScalarType()
            : base(Constants.ScalarNames.LONG, typeof(long))
        {
            this.Description = $"A 64-bit integer. (Min: {long.MinValue}, Max: {long.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(long?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (long.TryParse(data.ToString(), out var l))
                return l;

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