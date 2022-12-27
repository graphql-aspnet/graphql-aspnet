// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DirectiveExecution.Common
{
    using System;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;

    /// <summary>
    /// A base step for any rules targeting the validation of
    /// a request to invoke a directive.
    /// </summary>
    internal abstract class DirectiveValidationStep : IRuleStep<GraphDirectiveExecutionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveValidationStep"/> class.
        /// </summary>
        public DirectiveValidationStep()
        {
            this.RuleId = Guid.NewGuid();
        }

        /// <inheritdoc />
        public virtual bool ShouldExecute(GraphDirectiveExecutionContext context)
        {
            return context != null && !context.IsCancelled;
        }

        /// <inheritdoc />
        public abstract bool Execute(GraphDirectiveExecutionContext context);

        /// <inheritdoc />
        public virtual bool ShouldAllowChildContextsToExecute(GraphDirectiveExecutionContext context)
        {
            return true;
        }

        /// <inheritdoc />
        public Guid RuleId { get; }
    }
}