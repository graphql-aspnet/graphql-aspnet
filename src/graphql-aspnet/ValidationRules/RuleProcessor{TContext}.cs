// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A processor that will execute a set of rules against a context.
    /// </summary>
    /// <typeparam name="TContext">The type of the context being processed by this rule set.</typeparam>
    internal abstract class RuleProcessor<TContext>
        where TContext : IContextGenerator<TContext>
    {
        private readonly int _maxDepth;
        private readonly IRulePackage<TContext> _rulePackage;
        private readonly bool _childrenFirst;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleProcessor{TContext}" /> class.
        /// </summary>
        /// <param name="rulePackage">The rule package this instance will execute.</param>
        /// <param name="childrenFirst">if set to <c>true</c> child contexts of the current context will be generated
        /// and executed before the current context. <c>false</c> to execute children after the currently scoped context.</param>
        protected RuleProcessor(IRulePackage<TContext> rulePackage, bool childrenFirst = false)
        {
            _maxDepth = RuleProcessor.MaxProcessingDepth;
            _rulePackage = Validation.ThrowIfNullOrReturn(rulePackage, nameof(rulePackage));
            _childrenFirst = childrenFirst;
        }

        /// <summary>
        /// Executes the specified initial context.
        /// </summary>
        /// <param name="initialContexts">The initial, "Top level" set of contexts to execute against the rule
        /// set this instance contains.</param>
        /// <returns><c>true</c> if all contexts at all levels completed all steps successfully, <c>false</c> otherwise.</returns>
        public bool Execute(IEnumerable<TContext> initialContexts)
        {
            var completedAllSteps = true;
            foreach (var context in initialContexts)
            {
                completedAllSteps = this.ProcessContext(context) && completedAllSteps;
            }

            return completedAllSteps;
        }

        /// <summary>
        /// Processes the single context instance against the rule package defined on this instance.
        /// </summary>
        /// <param name="context">The context to process.</param>
        /// <param name="currentDepth">A counter to use as an exit condition should
        /// nesting tolerance exceed an expected value.</param>
        /// <returns><c>true</c> if the context completed its step set successfully, <c>false</c> otherwise.</returns>
        private bool ProcessContext(TContext context, int currentDepth = 0)
        {
            if (currentDepth > _maxDepth)
            {
                throw new GraphExecutionException($"When processing a rule set of type '{this.GetType().FriendlyName()}'" +
                    $"the execution context depth exceeded a maximum of {_maxDepth}. Check your context " +
                    "parent/child relationships and ensure no circular references exist. If your specific use case " +
                    $"warrants a greater processing depth you can change the max depth at {nameof(RuleProcessor)}.{nameof(RuleProcessor.MaxProcessingDepth)} during " +
                    "application configuration/startup.");
            }

            var completedAllSteps = true;
            if (context != null)
            {
                if (_childrenFirst)
                    completedAllSteps = this.ProcessChildContexts(context, currentDepth);

                var allowChildrenToExecute = true;
                foreach (var step in _rulePackage.FetchRules(context))
                {
                    if (step.ShouldExecute(context))
                    {
                        completedAllSteps = step.Execute(context);
                        if (!completedAllSteps)
                            break;
                    }

                    allowChildrenToExecute = step.ShouldAllowChildContextsToExecute(context) && allowChildrenToExecute;
                }

                if (!_childrenFirst && completedAllSteps && allowChildrenToExecute)
                    completedAllSteps = this.ProcessChildContexts(context, currentDepth);
            }

            return completedAllSteps;
        }

        /// <summary>
        /// Attempts to create and execution a collection of child contexts related to the given "parent" context.
        /// </summary>
        /// <param name="parentContext">The context for which children should be generated and executed.</param>
        /// <param name="parentDepth">The nesting depth of the executing parent context.</param>
        /// <returns><c>true</c> if all children completed all steps successfully, <c>false</c> otherwise.</returns>
        private bool ProcessChildContexts(TContext parentContext, int parentDepth)
        {
            var completedAllSteps = true;
            foreach (var childContext in parentContext.CreateChildContexts())
                completedAllSteps = this.ProcessContext(childContext, parentDepth + 1) && completedAllSteps;

            return completedAllSteps;
        }
    }
}