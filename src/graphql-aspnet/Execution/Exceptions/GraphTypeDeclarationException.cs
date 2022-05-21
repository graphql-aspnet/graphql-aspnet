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

    /// <summary>
    /// Thrown when reading a graph type via reflection fails to complete.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GraphTypeDeclarationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeDeclarationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public GraphTypeDeclarationException(string message, Exception innerException = null)
            : this(message, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeDeclarationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="failedType">The .net type that failed conversion to a
        /// graph type.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public GraphTypeDeclarationException(string message, Type failedType, Exception innerException = null)
            : base(message, innerException)
        {
            this.FailedObjectType = failedType;
        }

        /// <summary>
        /// Gets the concrete .NET type that was being evaluated for conversion to a Graph Type.
        /// </summary>
        /// <value>The type that was being evaluated.</value>
        public Type FailedObjectType { get; }
    }
}