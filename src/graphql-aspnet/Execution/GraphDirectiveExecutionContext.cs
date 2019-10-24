// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A set of information needed to successiful execute a directive as part of a field resolution.
    /// </summary>
    [DebuggerDisplay("Directive Context: {Directive.Name}")]
    public class GraphDirectiveExecutionContext : IDirectiveInvocationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveExecutionContext"/> class.
        /// </summary>
        /// <param name="seenAtLocation">The seen at location.</param>
        /// <param name="graphType">Type of the graph.</param>
        /// <param name="origin">The origin.</param>
        public GraphDirectiveExecutionContext(DirectiveLocation seenAtLocation, IDirectiveGraphType graphType, SourceOrigin origin)
        {
            this.Location = seenAtLocation;
            this.Directive = graphType;
            this.Origin = origin;
            this.Arguments = new InputArgumentCollection();
        }

        /// <summary>
        /// Gets the location this directive was seen at when attached to the parent field request.
        /// </summary>
        /// <value>The location.</value>
        public DirectiveLocation Location { get; }

        /// <summary>
        /// Gets the directive in scope to be executed.
        /// </summary>
        /// <value>The directive.</value>
        public IDirectiveGraphType Directive { get; }

        /// <summary>
        /// Gets the origin in the soruce document where this directive was invoked.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }

        /// <summary>
        /// Gets a set of arguments that are needed to complete the operation.
        /// </summary>
        /// <value>The arguments.</value>
        public IInputArgumentCollection Arguments { get; }
    }
}