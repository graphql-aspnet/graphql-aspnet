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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing2;

    /// <summary>
    /// A graph type reprsenting a 64-bit, unsigned integer.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class ULongScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ULongScalarType"/> class.
        /// </summary>
        public ULongScalarType()
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

            throw new UnresolvedValueException(data, typeof(ulong));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}