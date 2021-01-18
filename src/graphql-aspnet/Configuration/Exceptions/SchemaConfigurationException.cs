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
        /// Initializes a new instance of the <see cref="SchemaConfigurationException"/> class.
        /// </summary>
        public SchemaConfigurationException()
            : this("Unknown schema configuration exception")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SchemaConfigurationException(string message)
            : base(message)
        {
        }
    }
}