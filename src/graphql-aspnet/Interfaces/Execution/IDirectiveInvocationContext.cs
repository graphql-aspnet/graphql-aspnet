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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A set of information detailing a directive that should be invoked along with source information
    /// indicating where it originated.
    /// </summary>
    public interface IDirectiveInvocationContext
    {
        /// <summary>
        /// Gets the origin in the source document where this directive was invoked.
        /// </summary>
        /// <value>The origin.</value>
        SourceOrigin Origin { get; }

        /// <summary>
        /// Gets the <see cref="DirectiveLocation"/> in a source document from which this invocation context was generated.
        /// </summary>
        /// <value>The location.</value>
        DirectiveLocation Location { get; }

        /// <summary>
        /// Gets the directive type that should be invoked.
        /// </summary>
        /// <value>The directive.</value>
        IDirective Directive { get; }

        /// <summary>
        /// Gets a set of arguments that were supplied at the invocation site
        /// that are needed to complete the directive invocation.
        /// </summary>
        /// <value>The collection of arguments used to execute the request.</value>
        IInputArgumentCollection Arguments { get; }
    }
}