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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a standard guid.
    /// </summary>
    public sealed class GuidScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static GuidScalarType Instance { get; } = new GuidScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="GuidScalarType"/> class.
        /// </summary>
        static GuidScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidScalarType"/> class.
        /// </summary>
        private GuidScalarType()
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

            throw new UnresolvedValueException(data);
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            if (item == null)
                return item;

            return ((Guid)item).ToString();
        }

        /// <inheritdoc />
        public override string Description { get; }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}