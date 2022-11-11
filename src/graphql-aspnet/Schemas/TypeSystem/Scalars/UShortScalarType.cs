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
    /// A graph type representing a 16-bit unsigned integer.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class UShortScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UShortScalarType"/> class.
        /// </summary>
        public UShortScalarType()
            : base(Constants.ScalarNames.USHORT, typeof(ushort))
        {
            this.Description = $"A 16-bit unsigned integer. (Min: {ushort.MinValue}, Max: {ushort.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(ushort?));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (ushort.TryParse(data.ToString(), out var i))
                return i;

            throw new UnresolvedValueException(data, typeof(ushort));
        }
    }
}