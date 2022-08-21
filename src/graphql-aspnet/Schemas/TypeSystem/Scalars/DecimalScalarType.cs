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
    /// A graph type representing a 128-bit, floating-point value.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class DecimalScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalScalarType"/> class.
        /// </summary>
        public DecimalScalarType()
            : base(Constants.ScalarNames.DECIMAL, typeof(decimal))
        {
            this.Description = "A 128-bit, floating point value that offers greater local " +
                               "precision, with a smaller range, than other floating-point types. " +
                               $"(Min: {decimal.MinValue}, Max: {decimal.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(decimal?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (decimal.TryParse(data.ToString(), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var i))
                return i;

            throw new UnresolvedValueException(data, typeof(decimal));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}
