// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// A loggable item representing an exception that was thrown or generated.
    /// </summary>
    public class ExceptionLogItem : GraphLogPropertyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLogItem"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ExceptionLogItem(Exception exception)
        {
            this.ExceptionMessage = exception.Message;
            this.StackTrace = exception.StackTrace;
            this.TypeName = exception.GetType().FriendlyName(true);
            this.ShortTypeName = exception.GetType().FriendlyName();
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
        /// Gets name of the exception type without the namespace. This property is not recorded.
        /// </summary>
        /// <value>The short name of the type.</value>
        public string ShortTypeName { get; }
    }
}