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
    /// <summary>
    /// Helper methods for working with the <see cref="DirectiveLifeCyclePhase"/> enumeration.
    /// </summary>
    public static class DirectiveLifeCycleHelpers
    {
        /// <summary>
        /// Determines whether the specified lifecycle is considered part of the
        /// "Execution" phase or not (vs. type system delcaration phase).
        /// </summary>
        /// <param name="lifecycle">The lifecycle to inspect.</param>
        /// <returns><c>true</c> if the specified lifecycle targets the execution phase; otherwise, <c>false</c>.</returns>
        public static bool IsExecutionPhase(this DirectiveLifeCyclePhase lifecycle)
        {
            switch (lifecycle)
            {
                case DirectiveLifeCyclePhase.BeforeResolution:
                case DirectiveLifeCyclePhase.AfterResolution:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified lifecycle is considered part of the
        /// "type system generation" phase or not (vs. field execution phase).
        /// </summary>
        /// <param name="lifecycle">The lifecycle value to inspect.</param>
        /// <returns><c>true</c> if the specified lifecycle targets the type system generation phase; otherwise, <c>false</c>.</returns>
        public static bool IsTypeSystemPhase(this DirectiveLifeCyclePhase lifecycle)
        {
            switch (lifecycle)
            {
                case DirectiveLifeCyclePhase.AlterTypeSystem:
                    return true;

                default:
                    return false;
            }
        }
    }
}