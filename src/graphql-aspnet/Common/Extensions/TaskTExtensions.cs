// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Helpful method for working with <see cref="Task"/> and <see cref="Task{T}"/>.
    /// </summary>
    public static class TaskTExtensions
    {
        /// <summary>
        /// Attempts to extract the result of a boxed <see cref="Task{TResult}" /> as a given <paramref name="expectedType"/>.
        /// Returns null if the task does not declare a result or the retrieved result is not castable to the expected type.
        /// </summary>
        /// <param name="completedTask">The task.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <returns>System.Object.</returns>
        [DebuggerStepThrough]
        public static object ResultOfTypeOrNull(this Task completedTask, Type expectedType)
        {
            Validation.ThrowIfNull(completedTask, nameof(completedTask));
            Validation.ThrowIfNull(expectedType, nameof(expectedType));

            if (completedTask.IsFaulted)
                return null;

            var type = completedTask.GetType();
            if (!type.IsGenericType || expectedType == null)
            {
                return null;
            }

            var result = type.GetProperty("Result")?.GetValue(completedTask);
            if (result != null && Validation.IsCastable(result.GetType(), expectedType))
                return result;

            return null;
        }

        /// <summary>
        /// Attempts to extract the result of a boxed <see cref="Task{TResult}" /> as a <typeparamref name="T"/>.
        /// Returns null if the task does not declare a result or the retrieved result is not castable to the expected type.
        /// </summary>
        /// <typeparam name="T">The expected type to extract from the task.</typeparam>
        /// <param name="completedTask">The task.</param>
        /// <returns>System.Object.</returns>
        [DebuggerStepThrough]
        public static object ResultOfTypeOrNull<T>(this Task completedTask)
        {
            return ResultOfTypeOrNull(completedTask, typeof(T));
        }

        /// <summary>
        /// Unwraps the first found, internal, thrown exception from a task bypassing the aggregate exception.
        /// </summary>
        /// <param name="completedTask">The completed task.</param>
        /// <returns>Exception.</returns>
        [DebuggerStepThrough]
        public static Exception UnwrapException(this Task completedTask)
        {
            if (!completedTask.IsFaulted)
                return null;

            return completedTask.Exception?.InnerExceptions.FirstOrDefault();
        }
    }
}