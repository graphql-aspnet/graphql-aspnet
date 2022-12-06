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
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a standard guid.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class GuidScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuidScalarType"/> class.
        /// </summary>
        public GuidScalarType()
            : base(Constants.ScalarNames.GUID, typeof(Guid))
        {
            this.Description = "A standard guid (e.g. '6dd43342-ffe6-4964-bb6f-e31c8e50ec86').";
            this.OtherKnownTypes = TypeCollection.Empty;
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (Guid.TryParse(GraphQLStrings.UnescapeAndTrimDelimiters(data), out var guid))
                return guid;

            throw new UnresolvedValueException(data, typeof(Guid));
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            if (item == null)
                return item;

            return ((Guid)item).ToString();
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}