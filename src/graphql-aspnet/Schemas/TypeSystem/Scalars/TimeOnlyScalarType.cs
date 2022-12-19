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
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A graph type reprsenting a calendar date that does include a time component.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class TimeOnlyScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOnlyScalarType"/> class.
        /// </summary>
        public TimeOnlyScalarType()
            : base(Constants.ScalarNames.TIMEONLY, typeof(TimeOnly))
        {
            this.Description = "A time of day that does not include a date component.";
            this.OtherKnownTypes = new TypeCollection(typeof(TimeOnly?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (DateTimeExtensions.TryParseMultiFormat(
                GraphQLStrings.UnescapeAndTrimDelimiters(data, false),
                out TimeOnly? dt)
                && dt.HasValue)
            {
                return dt.Value;
            }

            throw new UnresolvedValueException(data, typeof(TimeOnly));
        }

        /// <inheritdoc />
        public override string SerializeToQueryLanguage(object item)
        {
            if (item?.GetType() == typeof(TimeOnly?))
                return ((TimeOnly?)item).ToRfc3339String().AsQuotedString();

            if (item?.GetType() == typeof(TimeOnly))
                return ((TimeOnly)item).ToRfc3339String().AsQuotedString();

            return Constants.QueryLanguage.NULL;
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}
#endif