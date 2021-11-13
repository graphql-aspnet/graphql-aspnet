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
    /// A graph type representing a 32-bit integer.
    /// </summary>
    public sealed class IntScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static IntScalarType Instance { get; } = new IntScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="IntScalarType"/> class.
        /// </summary>
        static IntScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntScalarType"/> class.
        /// </summary>
        private IntScalarType()
            : base(Constants.ScalarNames.INT, typeof(int))
        {
            this.Description = $"A 32-bit integer. (Min: {int.MinValue}, Max: {int.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(int?));
        }

        /// <inheritdoc />
        public override string Description { get; }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (int.TryParse(data.ToString(), out var i))
                return i;

            throw new UnresolvedValueException(data);
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            return item;
        }
    }
}