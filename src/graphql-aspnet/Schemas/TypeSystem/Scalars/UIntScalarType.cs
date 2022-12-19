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

    /// <summary>
    /// A graph type representing a 32-bit, unsigned integer.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class UIntScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIntScalarType"/> class.
        /// </summary>
        public UIntScalarType()
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

            throw new UnresolvedValueException(data, typeof(uint));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}