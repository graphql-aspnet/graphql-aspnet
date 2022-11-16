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
    using GraphQL.AspNet.Parsing2;

    /// <summary>
    /// Represents a scalar supported by a schema.
    /// </summary>
    /// <seealso cref="ISchemaItem" />
    public interface IScalarGraphType : IGraphType, ITypedSchemaItem
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
        /// Gets the type of the value as it should be supplied on an input argument. Scalar values, from a standpoint of "raw data" can be submitted as
        /// strings, numbers or a boolean value. A source value resolver would then convert this raw value into its formal scalar representation.
        /// </summary>
        /// <value>The type of the value.</value>
        ScalarValueType ValueType { get; }

        /// <summary>
        /// Gets or sets an object that will perform a conversion of raw data into the type
        /// defined on this instance.
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        ILeafValueResolver SourceResolver { get; set; }

        /// <summary>
        /// Gets or sets a URL pointing to a specification detailing the behavior
        /// of this scalar.
        /// </summary>
        /// <value>The specified by URL.</value>
        string SpecifiedByUrl { get; set; }

        /// <summary>
        /// Serializes the scalar from its object representation to a
        /// value that can be used in JSON serialziation. For most scalars this is
        /// a conversion to a valid string represnetation.
        /// </summary>
        /// <param name="item">The scalar to serialize.</param>
        /// <returns>System.Object.</returns>
        object Serialize(object item);

        /// <summary>
        /// Serializes the scalar from its object representation to its query language representation.
        /// </summary>
        /// <param name="item">The scalar to serialize.</param>
        /// <remarks>
        /// <b>Note:</b> If the scalar is represented as a quoted string in query language, be sure to return the value as a valid graphql string. (e.g. <c>"\"Value\""</c>).</remarks>
        /// <returns>System.Object.</returns>
        string SerializeToQueryLanguage(object item);
    }
}