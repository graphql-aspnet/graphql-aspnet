// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// Represents a scalar supported by a schema.
    /// </summary>
    /// <seealso cref="INamedItem" />
    public interface IScalarGraphType : IGraphType, ITypedItem
    {
        /// <summary>
        /// Gets a collection of other types that this scalar may be declared as. Scalars maybe
        /// represented in C# in multiple formats (e.g. int and int?) but these
        /// formats are still the same datatype from the perspective of graphql. This field captures the other known
        /// types of this scalar so they can be grouped and processed in a similar manner.
        /// </summary>
        /// <value>The other known types.</value>
        TypeCollection OtherKnownTypes { get; }

        /// <summary>
        /// Gets the type of the value as it should be supplied on an input argument. Scalar values, from a stand point of "raw data" can be submitted as
        /// strings, numbers or a boolean value. A source value resolver would then convert this raw value into its formal scalar representation.
        /// </summary>
        /// <value>The type of the value.</value>
        ScalarValueType ValueType { get; }

        /// <summary>
        /// Gets an object that will perform a conversion of raw data into the type
        /// defined on this instance.
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        IScalarValueResolver SourceResolver { get; }

        /// <summary>
        /// Gets an object that will perform the conversion of internal data value to a span of characters
        /// that can be written to a response stream.
        /// </summary>
        /// <value>The serializer.</value>
        IScalarValueSerializer Serializer { get; }
    }
}