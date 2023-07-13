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
    /// A graph type representing a 32-bit integer.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class IntScalarType : ScalarGraphTypeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntScalarType"/> class.
        /// </summary>
        public IntScalarType()
            : base(Constants.ScalarNames.INT, typeof(int))
        {
            this.Description = $"A 32-bit integer. (Min: {int.MinValue}, Max: {int.MaxValue})";
        }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (int.TryParse(data.ToString(), out var i))
                return i;

            throw new UnresolvedValueException(data, typeof(int));
        }
    }
}