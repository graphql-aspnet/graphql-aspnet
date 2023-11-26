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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// A manager that will allow GraphQL field resolvers to take isolated control
    /// and be the only field "resolving" at a given point in time.
    /// </summary>
    /// <remarks>The default implementation is a simple wrapper on a <see cref="SemaphoreSlim" />.</remarks>
    public interface IGraphQLFieldResolverIsolationManager
    {
        /// <summary>
        /// Determines if the given field should execute in a isolated mode on the target
        /// schema.
        /// </summary>
        /// <param name="schema">The schema governing the request.</param>
        /// <param name="fieldSource">The source of the field or directive being checked.</param>
        /// <returns><c>true</c> if the field should execute in an isolated mode, <c>false</c> otherwise.</returns>
        bool ShouldIsolate(ISchema schema, GraphFieldSource fieldSource);

        /// <summary>
        /// A call that will complete when the current request has been granted control
        /// by the isolation manager.
        /// </summary>
        /// <returns>Task.</returns>
        Task WaitAsync();

        /// <summary>
        /// Releases the caller's hold on the isolation context, allowing another
        /// field to take control if necessary.
        /// </summary>
        void Release();
    }
}