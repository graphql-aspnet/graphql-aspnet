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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;

    /// <summary>
    /// A graph type reprsenting a calendar date that does include a time component.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class DateTimeScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScalarType"/> class.
        /// </summary>
        public DateTimeScalarType()
            : base(Constants.ScalarNames.DATETIME, typeof(DateTime))
        {
            this.Description = "A calendar date that does include a time component.";
            this.OtherKnownTypes = new TypeCollection(typeof(DateTime?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (DateTimeExtensions.TryParseMultiFormat(
                GraphQLStrings.UnescapeAndTrimDelimiters(data, false),
                out DateTime? dt) &&
                dt.HasValue)
            {
                if (dt.Value.Kind == DateTimeKind.Unspecified && this.DefaultKind != DateTimeKind.Unspecified)
                    dt = DateTime.SpecifyKind(dt.Value, this.DefaultKind);

                return dt.Value;
            }

            throw new UnresolvedValueException(data, typeof(DateTime));
        }

        /// <inheritdoc />
        public override string SerializeToQueryLanguage(object item)
        {
            if (item?.GetType() == typeof(DateTime?))
                return ((DateTime?)item).ToRfc3339String().AsQuotedString();

            if (item?.GetType() == typeof(DateTime))
                return ((DateTime)item).ToRfc3339String().AsQuotedString();

            return Constants.QueryLanguage.NULL;
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.StringOrNumber;

        /// <summary>
        /// Gets or sets kind of date to interprete an input result as if/when a specific kind
        /// is not determinable from the source text.
        /// </summary>
        /// <value>The default kind.</value>
        public DateTimeKind DefaultKind { get; set; } = DateTimeKind.Utc;
    }
}