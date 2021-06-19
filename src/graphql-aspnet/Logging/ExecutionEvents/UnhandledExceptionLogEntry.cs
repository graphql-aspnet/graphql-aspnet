// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// A general event raised when an unhandled exception is not in any way
    /// handled by other parts of the runtime.
    /// </summary>
    public class UnhandledExceptionLogEntry : GraphLogEntry
    {
        private readonly string _shortTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionLogEntry" /> class.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        public UnhandledExceptionLogEntry(Exception exception)
            : base(LogEventIds.UnhandledException)
        {
            this.ExceptionMessage = exception.Message;
            this.StackTrace = exception.StackTrace;
            this.TypeName = exception.GetType().FriendlyName(true);
            _shortTypeName = exception.GetType().FriendlyName();
        }

        /// <summary>
        /// Gets the common message supplied on the exception.
        /// </summary>
        /// <value>The message.</value>
        public string ExceptionMessage
        {
            get => this.GetProperty<string>(LogPropertyNames.EXCEPTION_MESSAGE);
            private set => this.SetProperty(LogPropertyNames.EXCEPTION_MESSAGE, value);
        }

        /// <summary>
        /// Gets the complete stack trace, if one exists, of the exception.
        /// </summary>
        /// <value>The stack trace.</value>
        public string StackTrace
        {
            get => this.GetProperty<string>(LogPropertyNames.EXCEPTION_STACK_TRACE);
            private set => this.SetProperty(LogPropertyNames.EXCEPTION_STACK_TRACE, value);
        }

        /// <summary>
        /// Gets the name of the <see cref="System.Type"/> of the exception.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.EXCEPTION_TYPE);
            private set => this.SetProperty(LogPropertyNames.EXCEPTION_TYPE, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Unhandled Exception | Type: '{_shortTypeName}', Message: '{this.ExceptionMessage}' ";
        }
    }
}