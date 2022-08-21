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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a short.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class ShortScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortScalarType"/> class.
        /// </summary>
        public ShortScalarType()
            : base(Constants.ScalarNames.SHORT, typeof(short))
        {
            this.Description = $"A 16-bit integer. (Min: {short.MinValue}, Max: {short.MaxValue})";
            this.OtherKnownTypes = new TypeCollection(typeof(short?));
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Number;

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (short.TryParse(data.ToString(), out var i))
                return i;

            throw new UnresolvedValueException(data, typeof(short));
        }
    }
}
