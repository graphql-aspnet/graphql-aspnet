// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Exceptions
{
    using System;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// An exception thrown during query parsing when an input value is not valid for
    /// its target argument or input object field.
    /// </summary>
    public class UnresolvedValueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException" /> class.
        /// </summary>
        /// <param name="message">A friendly message that describes the error. This message
        /// is shared with the requestor.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null. If supplied this
        /// exception will shared with the requestor if the current schema exposes exceptions.</param>
        public UnresolvedValueException(string message, Exception innerException = null)
             : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException" /> class.
        /// </summary>
        /// <param name="value">The value from the query text that was unresolvable.</param>
        /// <param name="targetType">The target type the value was being resolved to (e.g. int, string, Donut etc.).</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null. If supplied this
        /// exception will shared with the requestor if the current schema exposes exceptions.</param>
        public UnresolvedValueException(ReadOnlySpan<char> value, Type targetType, Exception innerException = null)
             : this(
                   value.ToString(),
                   $"The value '{value.ToString()}' cannot be coerced or resolved for the target location (System Type: {targetType?.FriendlyName() ?? "unknown"}).",
                   innerException)
        {
            this.TargetType = targetType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException"/> class.
        /// </summary>
        /// <param name="value">The value from the query text that was unresolvable.</param>
        /// <param name="message">A friendly message that describes the error. This message
        /// is shared with the requestor.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null. If supplied this
        /// exception will shared with the requestor if the current schema exposes exceptions.</param>
        public UnresolvedValueException(ReadOnlySpan<char> value, string message, Exception innerException = null)
            : this(value.ToString(), message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException"/> class.
        /// </summary>
        /// <param name="value">The value from the query text that was unresolvable.</param>
        /// <param name="message">A friendly message that describes the error. This message
        /// is shared with the requestor.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null. If supplied this
        /// exception will shared with the requestor if the current schema exposes exceptions.</param>
        public UnresolvedValueException(string value, string message, Exception innerException = null)
            : this(message, innerException)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value, from a query text, that was not resolved correctly.
        /// </summary>
        /// <value>The value that was left unresolved in the source text, if supplied.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the .NET type the <see cref="Value"/> was attempting to be resolved to.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type TargetType { get; }
    }
}