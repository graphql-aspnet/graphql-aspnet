// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables
{
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A factory capable of creating new variable collections.
    /// </summary>
    public static class ResolvedVariableCollectionFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IResolvedVariableCollection"/> using the
        /// default implementation.
        /// </summary>
        /// <returns>IResolvedVariableCollection.</returns>
        public static IResolvedVariableCollection Create()
        {
            return new ResolvedVariableCollection();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="IResolvedVariableCollection" /> using the
        /// default implementation.
        /// </summary>
        /// <param name="capacity">The initial capacity for the collection.</param>
        /// <returns>IResolvedVariableCollection.</returns>
        public static IResolvedVariableCollection Create(int capacity)
        {
            return new ResolvedVariableCollection(capacity);
        }
    }
}