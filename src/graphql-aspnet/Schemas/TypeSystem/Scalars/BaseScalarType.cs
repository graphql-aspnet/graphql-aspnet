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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A base class for all scalar types. This base class serves as the scalar type itself, the value resolver and the serializer
    /// with abstract methods that must be implemented for the later two.
    /// </summary>
    public abstract class BaseScalarType : IScalarGraphType, IScalarValueResolver, IScalarValueSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseScalarType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="primaryType">Type of the primary.</param>
        protected BaseScalarType(string name, Type primaryType)
        {
            this.Name = Validation.ThrowIfNullOrReturn(name, nameof(name));
            this.ObjectType = Validation.ThrowIfNullOrReturn(primaryType, nameof(primaryType));
            this.InternalName = this.ObjectType.FriendlyName();
        }

        /// <summary>
        /// Processes the given request against this instance
        /// performing the source data conversion operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="data">The actual data read from the query document.</param>
        /// <returns>The final native value of the converted data.</returns>
        public abstract object Resolve(ReadOnlySpan<char> data);

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public virtual bool ValidateObject(object item)
        {
            if (item == null)
                return true;

            var itemType = item.GetType();
            return itemType == this.ObjectType || this.OtherKnownTypes.Contains(itemType);
        }

        /// <summary>
        /// Serializes the scalar from its object representing to a serializable value. For most scalars this is
        /// a conversion to a valid string represnetation.
        /// </summary>
        /// <param name="item">The scalar to serialize.</param>
        /// <returns>System.Object.</returns>
        public abstract object Serialize(object item);

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the primary object type used for this scalar.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the value indicating what type of graph type this instance is in the type system. (object, scalar etc.)
        /// </summary>
        /// <value>The kind.</value>
        public TypeKind Kind => TypeKind.SCALAR;

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGraphType" /> is published on an introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish => true;

        /// <summary>
        /// Gets the type of the value as it should be supplied on an input argument. Scalar values, from a stand point of "raw data" can be submitted as
        /// strings, numbers or a boolean value. A source value resolver would then convert this raw value into its formal scalar representation.
        /// </summary>
        /// <value>The type of the value.</value>
        public abstract ScalarValueType ValueType { get; }

        /// <summary>
        /// Gets an object that will perform a conversion of raw data into the type
        /// defined on this instance.
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        public IScalarValueResolver SourceResolver => this;

        /// <summary>
        /// Gets an object that will perform the conversion of internal data value to a span of characters
        /// that can be written to a response stream.
        /// </summary>
        /// <value>The serializer.</value>
        public IScalarValueSerializer Serializer => this;

        /// <summary>
        /// Gets a collection of other types that this scalar may be declared as. Scalars maybe
        /// represented in C# in multiple formats (e.g. int and int?) but these
        /// formats are still the same datatype from the perspective of graphql. This field captures the other known
        /// types of this scalar so they can be grouped and processed in a similar manner.
        /// </summary>
        /// <value>The other known types.</value>
        public abstract TypeCollection OtherKnownTypes { get; }
    }
}