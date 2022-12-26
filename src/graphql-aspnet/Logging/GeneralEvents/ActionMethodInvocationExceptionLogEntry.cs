﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.GeneralEvents
{
    using System;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// Recorded when the invocation of action method generated a known exception; generally
    /// related to target invocation errors.
    /// </summary>
    public class ActionMethodInvocationExceptionLogEntry : ActionMethodExceptionLogEntryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMethodInvocationExceptionLogEntry" /> class.
        /// </summary>
        /// <param name="method">The method being invoked.</param>
        /// <param name="request">The request being executed on the method.</param>
        /// <param name="exception">The exception that was thrown.</param>
        public ActionMethodInvocationExceptionLogEntry(
            IGraphFieldResolverMethod method,
            IDataRequest request,
            Exception exception)
            : base(
                LogEventIds.ControllerInvocationException,
                method,
                request,
                exception)
        {
        }
    }
}