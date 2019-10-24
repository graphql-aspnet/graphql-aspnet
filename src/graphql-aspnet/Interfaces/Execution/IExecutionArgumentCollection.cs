// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// A collection of resolved arguments that can be directly used in the invocation of an action method,
    /// directive or property to resolve data.
    /// </summary>
    public interface IExecutionArgumentCollection : IReadOnlyDictionary<string, ExecutionArgument>
    {
        /// <summary>
        /// Augments the collection with a source data object for a specific field execution and returns
        /// a copy of itself with that data attached.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <returns>IExecutionArgumentCollection.</returns>
        IExecutionArgumentCollection WithSourceData(object sourceData);

        /// <summary>
        /// Adds the specified argument to the collection.
        /// </summary>
        /// <param name="argument">The argument.</param>
        void Add(ExecutionArgument argument);

        /// <summary>
        /// Attempts to retrieve and cast the given argument to the value provided.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="value">The value to be filled if cast.</param>
        /// <returns><c>true</c> if the argument was found and cast, <c>false</c> otherwise.</returns>
        bool TryGetArgument<T>(string argumentName, out T value);

        /// <summary>
        /// Prepares this colleciton of arguments for use in the provided method invocation generating
        /// a set of values that can be used to invoke the method.
        /// </summary>
        /// <param name="graphMethod">The graph method.</param>
        /// <returns>System.Object[].</returns>
        object[] PrepareArguments(IGraphMethod graphMethod);

        /// <summary>
        /// Gets the source data, if any, that is supplying values for this execution run.
        /// </summary>
        /// <value>The source data.</value>
        object SourceData { get; }
    }
}