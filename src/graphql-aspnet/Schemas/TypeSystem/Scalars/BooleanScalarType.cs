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
    /// A graph type representing a true|false boolean value.
    /// </summary>
    public sealed class BooleanScalarType : BaseScalarType
    {
        /// <summary>
        /// Gets the single instance of this scalar to use across all schemas.
        /// </summary>
        /// <value>The instance.</value>
        public static BooleanScalarType Instance { get; } = new BooleanScalarType();

        /// <summary>
        /// Initializes static members of the <see cref="BooleanScalarType"/> class.
        /// </summary>
        static BooleanScalarType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanScalarType"/> class.
        /// </summary>
        private BooleanScalarType()
            : base(Constants.ScalarNames.BOOLEAN, typeof(bool))
        {
            this.Description = "A boolean value (Expressed as: true | false)";
            this.OtherKnownTypes = new TypeCollection(typeof(bool?));
        }

        /// <inheritdoc />
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (bool.TryParse(data.ToString(), out var i))
                return i;

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
        public override ScalarValueType ValueType => ScalarValueType.Boolean;
    }
}