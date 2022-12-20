// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.FieldResolution.Common
{
    using System;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;

    /// <summary>
    /// A general rule step for resolving a field <see cref="GraphDataItem"/> through a runner.
    /// </summary>
    internal abstract class FieldResolutionStep : IRuleStep<FieldValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolutionStep"/> class.
        /// </summary>
        public FieldResolutionStep()
        {
            this.RuleId = Guid.NewGuid();
        }

        /// <inheritdoc />
        public virtual bool ShouldExecute(FieldValidationContext context)
        {
            var status = context?.DataItem?.Status;
            return status.HasValue && !status.Value.IsFinalized();
        }

        /// <inheritdoc />
        public abstract bool Execute(FieldValidationContext context);

        /// <inheritdoc />
        public virtual bool ShouldAllowChildContextsToExecute(FieldValidationContext context)
        {
            return true;
        }

        /// <inheritdoc />
        public Guid RuleId { get; }
    }
}