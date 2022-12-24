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
    /// An exception thrown when an value (to a field argument or an input object)
    /// is not valid for its target graph type.
    /// </summary>
    public class UnresolvedValueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException" /> class.
        /// </summary>
        /// <param name="message">The friendly message that describes the error. This message
        /// is shared with all requestors.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference. If supplied this
        /// exception will be attached to a graphql error message and only supplied if the schema exposes exceptions.</param>
        public UnresolvedValueException(string message, Exception innerException = null)
             : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException" /> class.
        /// </summary>
        /// <param name="value">The value to pass along with the exception.</param>
        /// <param name="targetType">The target type the value was being resolved to.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference. If supplied this
        /// exception will be attached to a graphql error message and only supplied if the schema exposes exceptions.</param>
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
        /// <param name="value">The value that was left unresolved.</param>
        /// <param name="message">The friendly message that describes the error. This message
        /// is shared with all requestors.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference. If supplied this
        /// exception will be attached to a graphql error message and only supplied if the schema exposes exceptions.</param>
        public UnresolvedValueException(ReadOnlySpan<char> value, string message, Exception innerException = null)
            : this(value.ToString(), message, innerException)
        {
        }

         /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedValueException"/> class.
        /// </summary>
        /// <param name="value">The value that was left unresolved.</param>
        /// <param name="message">The friendly message that describes the error. This message
        /// is shared with all requestors.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference. If supplied this
        /// exception will be attached to a graphql error message and only supplied if the schema exposes exceptions.</param>
        public UnresolvedValueException(string value, string message, Exception innerException = null)
            : this(message, innerException)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value, from a query text, that was not resolved correctly.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the .NET type the <see cref="Value"/> was attempting to be resolved to.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type TargetType { get; }
    }
}