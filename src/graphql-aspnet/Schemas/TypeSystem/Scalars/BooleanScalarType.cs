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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A graph type representing a true|false boolean value.
    /// </summary>
    [DebuggerDisplay("SCALAR: {Name}")]
    public sealed class BooleanScalarType : BaseScalarType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanScalarType"/> class.
        /// </summary>
        public BooleanScalarType()
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
        public override TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public override ScalarValueType ValueType => ScalarValueType.Boolean;
    }
}