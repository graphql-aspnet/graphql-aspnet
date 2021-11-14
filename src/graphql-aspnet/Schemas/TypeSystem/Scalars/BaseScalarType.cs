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
        /// <param name="name">The name of the scalar as it appears in a schema.</param>
        /// <param name="primaryType">The primary datatype that represents this scalar.</param>
        protected BaseScalarType(string name, Type primaryType)
        {
            this.Name = Validation.ThrowIfNullOrReturn(name, nameof(name));
            this.ObjectType = Validation.ThrowIfNullOrReturn(primaryType, nameof(primaryType));
            this.InternalName = this.ObjectType.FriendlyName();
        }

        /// <inheritdoc />
        public abstract object Resolve(ReadOnlySpan<char> data);

        /// <inheritdoc />
        public virtual bool ValidateObject(object item)
        {
            if (item == null)
                return true;

            var itemType = item.GetType();
            return itemType == this.ObjectType || this.OtherKnownTypes.Contains(itemType);
        }

        /// <inheritdoc />
        public abstract object Serialize(object item);

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <inheritdoc />
        public virtual Type ObjectType { get; }

        /// <inheritdoc />
        public virtual string InternalName { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.SCALAR;

        /// <inheritdoc />
        public bool Publish => true;

        /// <inheritdoc />
        public abstract ScalarValueType ValueType { get; }

        /// <inheritdoc />
        public virtual IScalarValueResolver SourceResolver => this;

        /// <inheritdoc />
        public virtual IScalarValueSerializer Serializer => this;

        /// <inheritdoc />
        public abstract TypeCollection OtherKnownTypes { get; }

        /// <inheritdoc />
        public virtual bool IsVirtual => false;
    }
}