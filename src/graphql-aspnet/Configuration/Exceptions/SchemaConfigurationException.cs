// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Exceptions
{
    using System;

    /// <summary>
    /// An exception thrown when some aspect of a schema's configuration is invalid
    /// or unacceptable in the current context.
    /// </summary>
    public class SchemaConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SchemaConfigurationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}