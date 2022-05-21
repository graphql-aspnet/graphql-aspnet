// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An object that can inject the a late binding application of a <see cref="GraphDirective"/> to a
    /// <see cref="IGraphType"/> during <see cref="ISchema"/> construction.
    /// </summary>
    public interface IDirectiveApplicator
    {
        /// <summary>
        /// Assigns a set of arguments to all applications of this directive. If called more
        /// than once for any given directive application, the last set of arguments
        /// will be kept and others discarded.
        /// </summary>
        /// <param name="arguments">The arguments to apply to the directive when its
        /// executed.</param>
        /// <returns>IDirectiveInjector.</returns>
        IDirectiveApplicator WithArguments(params object[] arguments);
    }
}