﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.InputModel
{
    using System;

    /// <summary>
    /// A single error that occured on an input model.
    /// </summary>
    public class InputModelError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputModelError" /> class.
        /// </summary>
        /// <param name="memberName">Name of the object member that errored.</param>
        /// <param name="errorMessage">A human friendly error message.</param>
        /// <param name="exception">An exception thrown during the model validation, if any.</param>
        public InputModelError(string memberName, string errorMessage, Exception exception = null)
        {
            this.Exception = exception;
            this.MemberName = memberName?.Trim();
            this.ErrorMessage = errorMessage?.Trim();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputModelError" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="exception">The exception.</param>
        public InputModelError(string errorMessage, Exception exception = null)
        {
            this.Exception = exception;
            this.ErrorMessage = errorMessage?.Trim();
        }

        /// <summary>
        /// Gets the exception that was thrown to cause this error, if any.
        /// </summary>
        /// <value>The exception encountered during validation, if any.</value>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the name of the member to which this error message applies. May be null if the error is a general error.
        /// </summary>
        /// <value>The name of the object member that failed.</value>
        public string MemberName { get; }

        /// <summary>
        /// Gets the error message that was added to the model entry.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; }
    }
}