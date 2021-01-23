﻿// *************************************************************
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

        /// <summary>
        /// Processes the given request against this instance
        /// performing the source data conversion operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="data">The actual data read from the query document.</param>
        /// <returns>The final native value of the converted data.</returns>
        public override object Resolve(ReadOnlySpan<char> data)
        {
            if (Guid.TryParse(GraphQLStrings.UnescapeAndTrimDelimiters(data), out var guid))
                return guid;

            throw new UnresolvedValueException(data);
        }

        /// <summary>
        /// Serializes the scalar from its object representing to a serializable value. For most scalars this is
        /// a conversion to a valid string represnetation.
        /// </summary>
        /// <param name="item">The scalar to serialize.</param>
        /// <returns>System.Object.</returns>
        public override object Serialize(object item)
        {
            if (item == null)
                return item;

            return ((Guid)item).ToString();
        }

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public override string Description { get; }

        /// <summary>
        /// Gets a collection of other types that this scalar may be declared as. Scalars maybe
        /// represented in C# in multiple formats (e.g. int and int?) but these
        /// formats are still the same datatype from the perspective of graphql. This field captures the other known
        /// types of this scalar so they can be grouped and processed in a similar manner.
        /// </summary>
        /// <value>The other known types.</value>
        public override TypeCollection OtherKnownTypes { get; }

        /// <summary>
        /// Gets the type of the value as it should be supplied on an input argument. Scalar values, from a stand point of "raw data" can be submitted as
        /// strings, numbers or a boolean value. A source value resolver would then convert this raw value into its formal scalar representation.
        /// </summary>
        /// <value>The type of the value.</value>
        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}