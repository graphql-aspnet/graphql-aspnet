// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#if NET6_0_OR_GREATER
namespace GraphQL.AspNet.Schemas.TypeSystem.Scalars
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type reprsenting a calendar date that does include a time component.
    /// </summary>
    public sealed class DateOnlyScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static DateOnlyScalarType Instance { get; } = new DateOnlyScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="DateOnlyScalarType"/> class.
        /// </summary>
        static DateOnlyScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyScalarType"/> class.
        /// </summary>
        private DateOnlyScalarType()
            : base(Constants.ScalarNames.DATEONLY, typeof(DateOnly))
        {
            this.Description = "A calendar date that does not include a time component.";
            this.OtherKnownTypes = new TypeCollection(typeof(DateOnly?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (DateTimeExtensions.TryParseMultiFormat(
                GraphQLStrings.UnescapeAndTrimDelimiters(data, false),
                out DateOnly? dt)
                && dt.HasValue)
            {
                return dt.Value;
            }

            throw new UnresolvedValueException(data);
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            return item;
        }

        /// <inheritdoc />
        public override string Description { get; }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.StringOrNumber;
    }
}
#endif