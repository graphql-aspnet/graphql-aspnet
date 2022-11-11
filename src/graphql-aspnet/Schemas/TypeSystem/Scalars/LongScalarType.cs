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
    /// A graph type reprsenting a 64-bit integer.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class LongScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongScalarType"/> class.
        /// </summary>
        public LongScalarType()
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

            throw new UnresolvedValueException(data, typeof(long));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}