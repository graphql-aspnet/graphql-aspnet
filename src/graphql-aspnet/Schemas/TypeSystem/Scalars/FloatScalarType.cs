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
    using System.Globalization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a 32-bit, floating-point value.
    /// </summary>
    public sealed class FloatScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static FloatScalarType Instance { get; } = new FloatScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="FloatScalarType"/> class.
        /// </summary>
        static FloatScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatScalarType"/> class.
        /// </summary>
        private FloatScalarType()
            : base(Constants.ScalarNames.FLOAT, typeof(float))
        {
            this.Description = $"A 32-bit, floating-point value. (Min: {float.MinValue}, Max: {float.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(float?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (float.TryParse(data.ToString(), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var i))
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
