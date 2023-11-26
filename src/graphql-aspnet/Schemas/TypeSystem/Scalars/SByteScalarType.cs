﻿// *************************************************************
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
    /// A graph type representing a single, signed byte.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class SByteScalarType : ScalarGraphTypeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SByteScalarType"/> class.
        /// </summary>
        public SByteScalarType()
            : base(Constants.ScalarNames.SIGNED_BYTE, typeof(sbyte))
        {
            this.Description = $"A signed byte. (Min: {sbyte.MinValue}, Max: {sbyte.MaxValue})";
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (sbyte.TryParse(data.ToString(), out var i))
                return i;

            throw new UnresolvedValueException(data, typeof(sbyte));
        }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;
    }
}