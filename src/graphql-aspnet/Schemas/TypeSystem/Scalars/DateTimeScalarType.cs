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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type reprsenting a calendar date that does include a time component.
    /// </summary>
    public sealed class DateTimeScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static DateTimeScalarType Instance { get; } = new DateTimeScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="DateTimeScalarType"/> class.
        /// </summary>
        static DateTimeScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScalarType"/> class.
        /// </summary>
        private DateTimeScalarType()
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

        /// <summary>
        /// Gets or sets kind of date to interprete an input result as if/when a specific kind
        /// is not determinable from the source text.
        /// </summary>
        /// <value>The default kind.</value>
        public DateTimeKind DefaultKind { get; set; } = DateTimeKind.Utc;
    }
}