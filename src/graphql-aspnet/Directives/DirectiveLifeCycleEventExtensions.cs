// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Directives
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// Helper methods for working with the <see cref="DirectiveLifeCycleEvent"/> enumeration.
    /// </summary>
    public static class DirectiveLifeCycleEventExtensions
    {
        /// <summary>
        /// Determines whether the specified lifecycle value is considered part of the
        /// "Execution" phase or not (vs. type system delcaration phase).
        /// </summary>
        /// <param name="lifecycle">The lifecycle to inspect.</param>
        /// <returns><c>true</c> if the specified lifecycle targets the execution phase; otherwise, <c>false</c>.</returns>
        public static bool IsExecutionPhase(this DirectiveLifeCycleEvent lifecycle)
        {
            switch (lifecycle)
            {
                case DirectiveLifeCycleEvent.BeforeResolution:
                case DirectiveLifeCycleEvent.AfterResolution:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified lifecycle event is considered part of the
        /// "type system generation" phase or not (vs. field execution phase).
        /// </summary>
        /// <param name="lifecycle">The lifecycle value to inspect.</param>
        /// <returns><c>true</c> if the specified lifecycle targets the type system generation phase; otherwise, <c>false</c>.</returns>
        public static bool IsTypeSystemPhase(this DirectiveLifeCycleEvent lifecycle)
        {
            switch (lifecycle)
            {
                case DirectiveLifeCycleEvent.AlterTypeSystem:
                    return true;

                default:
                    return false;
            }
        }
    }
}