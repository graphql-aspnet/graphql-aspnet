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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type reprsenting a calendar date that does include a time component.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class DateOnlyScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyScalarType"/> class.
        /// </summary>
        public DateOnlyScalarType()
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

            throw new UnresolvedValueException(data, typeof(DateOnly));
        }

        /// <inheritdoc />
        public override string SerializeToQueryLanguage(object item)
        {
            if (item?.GetType() == typeof(DateOnly?))
                return ((DateOnly?)item).ToRfc3339String().AsQuotedString();

            if (item?.GetType() == typeof(DateOnly))
                return ((DateOnly)item).ToRfc3339String().AsQuotedString();

            return Constants.QueryLanguage.NULL;
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.StringOrNumber;
    }
}
#endif