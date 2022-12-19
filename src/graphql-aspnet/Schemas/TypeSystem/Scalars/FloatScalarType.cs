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
    using System.Diagnostics;
    using System.Globalization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;

    /// <summary>
    /// A graph type representing a 32-bit, floating-point value.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class FloatScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatScalarType"/> class.
        /// </summary>
        public FloatScalarType()
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

            throw new UnresolvedValueException(data, typeof(float));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}