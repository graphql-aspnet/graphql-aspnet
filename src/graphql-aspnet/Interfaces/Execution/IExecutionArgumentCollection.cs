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
    using GraphQL.AspNet.Execution.Contexts;

    /// <summary>
    /// A collection of resolved arguments that can be directly used in the invocation of an action method,
    /// directive or property to resolve data.
    /// </summary>
    public interface IExecutionArgumentCollection : IReadOnlyDictionary<string, ExecutionArgument>
    {
        /// <summary>
        /// Augments the collection with data related the specific resolution. This is used to extract and apply
        /// execution arguments supplied on the query to the target resolver.
        /// </summary>
        /// <param name="resolutionContext">The field or directive context being executed.</param>
        /// <returns>IExecutionArgumentCollection.</returns>
        IExecutionArgumentCollection ForContext(SchemaItemResolutionContext resolutionContext);

        /// <summary>
        /// Adds the specified argument value found at runtime.
        /// </summary>
        /// <param name="argument">The argument value.</param>
        void Add(ExecutionArgument argument);

        /// <summary>
        /// Attempts to retrieve and cast the given argument to the value provided.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="argumentName">Name of the argument as it appears in the schema.</param>
        /// <param name="value">The value to be filled if cast.</param>
        /// <returns><c>true</c> if the argument was found and cast, <c>false</c> otherwise.</returns>
        bool TryGetArgument<T>(string argumentName, out T value);

        /// <summary>
        /// Prepares this colleciton of arguments for use in the provided method invocation generating
        /// a set of values that can be used to invoke the method.
        /// </summary>
        /// <param name="graphMethod">The graph method.</param>
        /// <returns>System.Object[].</returns>
        object[] PrepareArguments(IGraphFieldResolverMetaData graphMethod);
    }
}