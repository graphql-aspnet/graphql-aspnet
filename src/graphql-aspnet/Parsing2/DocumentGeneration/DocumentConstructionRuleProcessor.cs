// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.DocumentGeneration
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction;

    /// <summary>
    /// A rule processor that handles rules related to constructing a query document from source text
    /// submitted by a user. I.E. these rules collectively build a usable query document for a target schema
    /// using the document syntax provided by the end user.
    /// </summary>
    internal sealed class DocumentConstructionRuleProcessor
    {
        private readonly DocumentConstructionRulePackage _rulePackage;

        private readonly int _maxDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionRuleProcessor"/> class.
        /// </summary>
        public DocumentConstructionRuleProcessor()
        {
            _maxDepth = RulesEngine.RuleProcessor.MaxProcessingDepth;
            _rulePackage = DocumentConstructionRulePackage.Instance;
        }

        /// <summary>
        /// Executes the single context against the rule set.
        /// </summary>
        /// <param name="initialContext">The initial, "Top level" context to execute against the rule
        /// set this instance contains.</param>
        /// <returns><c>true</c> if the context, at all levels, completed all steps successfully, <c>false</c> otherwise.</returns>
        public bool Execute(ref DocumentConstructionContext initialContext)
        {
            return this.ProcessContext(ref initialContext, 0);
        }

        /// <summary>
        /// Processes the single context instance against the rule package defined on this instance.
        /// </summary>
        /// <param name="context">The context to process.</param>
        /// <param name="currentDepth">A counter to use as an exit condition should
        /// nesting tolerance exceed an expected value.</param>
        /// <returns><c>true</c> if the context completed its step set successfully, <c>false</c> otherwise.</returns>
        private bool ProcessContext(ref DocumentConstructionContext context, int currentDepth = 0)
        {
            if (currentDepth > _maxDepth)
            {
                throw new GraphExecutionException(
                    $"When processing a rule set of type '{this.GetType().FriendlyName()}'" +
                    $"the execution context depth exceeded a maximum of {_maxDepth}. Check your context " +
                    "parent/child relationships and ensure no circular references exist. If your specific use case " +
                    $"warrants a greater processing depth you can change the max depth at {nameof(RulesEngine.RuleProcessor)}.{nameof(RulesEngine.RuleProcessor.MaxProcessingDepth)} during " +
                    "application configuration/startup.");
            }

            var completedAllSteps = true;
            var allowChildrenToExecute = true;
            foreach (var step in _rulePackage.FetchRules(context.ActiveNode.NodeType))
            {
                if (step.ShouldExecute(context))
                {
                    completedAllSteps = step.Execute(ref context);
                    if (!completedAllSteps)
                        break;
                }

                allowChildrenToExecute = step.ShouldAllowChildContextsToExecute(context) && allowChildrenToExecute;
            }

            if (completedAllSteps && allowChildrenToExecute)
                completedAllSteps = this.ProcessChildContexts(ref context, currentDepth);

            return completedAllSteps;
        }

        /// <summary>
        /// Attempts to create and execute a collection of child contexts related
        /// to the provided "parent" context.
        /// </summary>
        /// <param name="parentContext">The context for which children should be generated and executed.</param>
        /// <param name="parentDepth">The nesting depth of the executing parent context.</param>
        /// <returns><c>true</c> if all children completed all steps successfully, <c>false</c> otherwise.</returns>
        private bool ProcessChildContexts(ref DocumentConstructionContext parentContext, int parentDepth)
        {
            var activeNode = parentContext.ActiveNode;
            if (activeNode.Coordinates.ChildBlockLength == 0)
                return true;

            var completedAllSteps = true;
            for (var i = 0; i < activeNode.Coordinates.ChildBlockLength; i++)
            {
                var childNode = parentContext.SyntaxTree.NodePool[activeNode.Coordinates.ChildBlockIndex][i];
                var childContext = parentContext.CreateChildContext(childNode);
                completedAllSteps = this.ProcessContext(ref childContext, parentDepth + 1) && completedAllSteps;
            }

            return completedAllSteps;
        }
    }
}