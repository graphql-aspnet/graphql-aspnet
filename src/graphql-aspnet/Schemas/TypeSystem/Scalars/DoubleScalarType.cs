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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a 64-bit, floating-point value.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class DoubleScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleScalarType"/> class.
        /// </summary>
        public DoubleScalarType()
            : base(Constants.ScalarNames.DOUBLE, typeof(double))
        {
            this.Description = $"A 64-bit, floating-point value. (Min: {double.MinValue}, Max: {double.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(double?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (double.TryParse(data.ToString(), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var i))
                return i;

            throw new UnresolvedValueException(data, typeof(double));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}
