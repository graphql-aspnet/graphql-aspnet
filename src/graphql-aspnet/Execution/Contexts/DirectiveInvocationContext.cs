// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.Contexts
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <inheritdoc cref="IDirectiveInvocationContext" />
    [DebuggerDisplay("{Directive.Name} (Location: {Location})")]
    public class DirectiveInvocationContext : IDirectiveInvocationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveInvocationContext" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive.</param>
        /// <param name="location">The target location type of this invocation.</param>
        /// <param name="origin">The origin point in source document
        /// where this directive invocation appeared.</param>
        /// <param name="args">The collection of arguments
        /// obtained or parsed during source document construction.</param>
        public DirectiveInvocationContext(
            IDirective directiveType,
            DirectiveLocation location,
            SourceOrigin origin = default,
            IInputArgumentCollection args = null)
        {
            this.Origin = origin;
            this.Location = location;
            this.Directive = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));
            this.Arguments = args ?? new InputArgumentCollection();
        }

        /// <inheritdoc />
        public SourceOrigin Origin { get; }

        /// <inheritdoc />
        public DirectiveLocation Location { get; }

        /// <inheritdoc />
        public IDirective Directive { get; }

        /// <inheritdoc />
        public IInputArgumentCollection Arguments { get; }
    }
}