// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.InputArguments
{
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;

    /// <summary>
    /// A factory capable of creating new argument collections.
    /// </summary>
    public static class InputArgumentCollectionFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IInputArgumentCollection"/> using the
        /// default implementation.
        /// </summary>
        /// <returns>IInputArgumentCollection.</returns>
        public static IInputArgumentCollection Create()
        {
            return new InputArgumentCollection();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="IInputArgumentCollection" /> using the
        /// default implementation.
        /// </summary>
        /// <param name="capacity">The initial capacity for the collection.</param>
        /// <returns>IInputArgumentCollection.</returns>
        public static IInputArgumentCollection Create(int capacity)
        {
            return new InputArgumentCollection(capacity);
        }
    }
}