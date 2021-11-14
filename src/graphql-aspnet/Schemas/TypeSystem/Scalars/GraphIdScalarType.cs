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
    /// This servers implementation of the required "ID" scalar type of graphql. Maps to the concrete C# object type of <see cref="GraphId"/>.
    /// </summary>
    public sealed class GraphIdScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static GraphIdScalarType Instance { get; } = new GraphIdScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="GraphIdScalarType"/> class.
        /// </summary>
        static GraphIdScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphIdScalarType"/> class.
        /// </summary>
        private GraphIdScalarType()
            : base(Constants.ScalarNames.ID, typeof(GraphId))
        {
            this.Description = "The id scalar type represents a unique identifier in graphql.";
            this.OtherKnownTypes = TypeCollection.Empty;
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            var output = GraphQLStrings.UnescapeAndTrimDelimiters(data, true);

            if (output == null)
                throw new UnresolvedValueException(data);

            return new GraphId(output);
        }

        /// <inheritdoc />
        public override object Serialize(object item)
        {
            if (item == null)
                return item;

            return ((GraphId)item).Value;
        }

        /// <inheritdoc />
        public override string Description { get; }

        /// <inheritdoc />
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}