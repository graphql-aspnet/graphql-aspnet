// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A qualified subscription name representing both the unique path in a schema
    /// and the optional short name of said subscription event.
    /// </summary>
    [DebuggerDisplay("Event Name: {EventName}")]
    public readonly struct SubscriptionEventName
    {
        /// <summary>
        /// Creates a set of event names representing all the possible forms of the event as defined
        /// by the grpah field.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the graph field exists in.</typeparam>
        /// <param name="field">The field to create names for.</param>
        /// <returns>IEnumerable&lt;SubscriptionEventName&gt;.</returns>
        public static SubscriptionEventName FromGraphField<TSchema>(ISubscriptionGraphField field)
            where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(field, nameof(field));
            return SubscriptionEventName.FromSchemaTypeAndField(typeof(TSchema), field);
        }

        /// <summary>
        /// Creates a set of event names representing all the possible forms of the event as defined
        /// by the graph field.
        /// </summary>
        /// <param name="schema">The schema owning the field definition.</param>
        /// <param name="field">The field to create names for.</param>
        /// <returns>IEnumerable&lt;SubscriptionEventName&gt;.</returns>
        public static SubscriptionEventName FromGraphField(ISchema schema, ISubscriptionGraphField field)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            Validation.ThrowIfNull(field, nameof(field));
            return SubscriptionEventName.FromSchemaTypeAndField(schema.GetType(), field);
        }

        /// <summary>
        /// Creates a set of event names representing all the possible forms of the event as defined
        /// by the graph field.
        /// </summary>
        /// <param name="schemaType">The raw data type of the target schema.</param>
        /// <param name="field">The field to create names for.</param>
        /// <returns>IEnumerable&lt;SubscriptionEventName&gt;.</returns>
        private static SubscriptionEventName FromSchemaTypeAndField(Type schemaType, ISubscriptionGraphField field)
        {
            return new SubscriptionEventName(schemaType, field.EventName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventName" /> struct.
        /// </summary>
        /// <param name="schemaType">Type of the schema which owns this name.</param>
        /// <param name="eventName">Name the event.</param>
        public SubscriptionEventName(Type schemaType, string eventName)
            : this(SchemaExtensions.RetrieveFullyQualifiedTypeName(schemaType), eventName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventName" /> struct.
        /// </summary>
        /// <param name="fullyQualifiedSchemaTypeName">A string representing the "FullName" of the schema type.</param>
        /// <param name="eventName">Name the event.</param>
        public SubscriptionEventName(string fullyQualifiedSchemaTypeName, string eventName)
        {
            fullyQualifiedSchemaTypeName = Validation.ThrowIfNullWhiteSpaceOrReturn(
                fullyQualifiedSchemaTypeName,
                nameof(fullyQualifiedSchemaTypeName));

            eventName = Validation.ThrowIfNullWhiteSpaceOrReturn(eventName, nameof(eventName));

            this.OwnerSchemaType = fullyQualifiedSchemaTypeName;
            this.EventName = eventName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventName"/> struct.
        /// </summary>
        public SubscriptionEventName()
        {
            this.OwnerSchemaType = null;
            this.EventName = null;
        }

        /// <summary>
        /// Gets the full name of the owner schema that created this event name.
        /// </summary>
        /// <value>The type of the owner schema.</value>
        public string OwnerSchemaType { get; }

        /// <summary>
        /// Gets root event name of this event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{this.OwnerSchemaType}:{this.EventName}";
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(SubscriptionEventName other)
        {
            return string.Compare(this.EventName, other.EventName) == 0
                && string.Compare(this.OwnerSchemaType, other.OwnerSchemaType) == 0;

        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (obj is SubscriptionEventName subEventName)
            {
                return this.Equals(subEventName);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return (this.OwnerSchemaType, this.EventName).GetHashCode();
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SubscriptionEventName ls, SubscriptionEventName rs)
        {
            return ls.Equals(rs);
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SubscriptionEventName ls, SubscriptionEventName rs)
        {
            return !(ls == rs);
        }
    }
}