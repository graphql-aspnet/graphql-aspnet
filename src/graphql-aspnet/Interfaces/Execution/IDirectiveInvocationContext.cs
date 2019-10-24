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
    /// A set of information needed to successiful execute a directive as part of a field resolution.
    /// </summary>
    public interface IDirectiveInvocationContext
    {
        /// <summary>
        /// Gets the location this directive was seen at when attached to the parent field request.
        /// </summary>
        /// <value>The location.</value>
        DirectiveLocation Location { get; }

        /// <summary>
        /// Gets the directive in scope to be executed.
        /// </summary>
        /// <value>The directive.</value>
        IDirectiveGraphType Directive { get; }

        /// <summary>
        /// Gets the origin in the soruce document where this directive was invoked.
        /// </summary>
        /// <value>The origin.</value>
        SourceOrigin Origin { get; }

        /// <summary>
        /// Gets a set of arguments that are needed to complete the operation.
        /// </summary>
        /// <value>The arguments.</value>
        IInputArgumentCollection Arguments { get; }
    }
}