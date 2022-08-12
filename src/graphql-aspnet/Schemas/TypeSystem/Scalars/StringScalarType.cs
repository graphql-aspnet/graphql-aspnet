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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a UTF-8 encoded string of characters.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class StringScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringScalarType"/> class.
        /// </summary>
        public StringScalarType()
            : base(Constants.ScalarNames.STRING, typeof(string))
        {
            this.Description = "A UTF-8 encoded string of characters.";
            this.OtherKnownTypes = TypeCollection.Empty;
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            var value = GraphQLStrings.UnescapeAndTrimDelimiters(data, true);
            if (value == null)
            {
                throw new UnresolvedValueException(data, typeof(string));
            }

            return value;
        }

        /// <inheritdoc />
        public override string SerializeToQueryLanguage(object item)
        {
            if (item == null || item.GetType() != typeof(string))
                return Constants.QueryLanguage.NULL;

            return GraphQLStrings.Escape(item.ToString()).AsQuotedString();
        }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}