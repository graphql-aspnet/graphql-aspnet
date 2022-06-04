// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Attributes
{
    using GraphQL.AspNet.Directives;

    /// <summary>
    /// An attribute applied to a directive to instruct the runtime on when
    /// it should be invoked.
    /// </summary>
    public class DirectiveInvocationPhaseAttribute : BaseGraphAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveInvocationPhaseAttribute"/> class.
        /// </summary>
        /// <param name="phases">The phases under which the directive should be invoked.</param>
        public DirectiveInvocationPhaseAttribute(DirectiveInvocationPhase phases)
        {
            this.Phases = phases;
        }

        /// <summary>
        /// Gets the phases configured on this attribute.
        /// </summary>
        /// <value>The phases.</value>
        public DirectiveInvocationPhase Phases { get; }
    }
}